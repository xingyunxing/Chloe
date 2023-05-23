﻿using Chloe.RDBMS.DDL;
using Chloe.Descriptors;
using Chloe.Reflection;
using System.Xml.Linq;

namespace Chloe.Oracle.DDL
{
    public class OracleTableGenerator : TableGenerator
    {
        public OracleTableGenerator(IDbContext dbContext) : base(dbContext)
        {

        }
        public OracleTableGenerator(IDbContext dbContext, TableGenerateOptions options) : base(dbContext, options)
        {
        }

        public override List<string> GenCreateTableScript(TypeDescriptor typeDescriptor, string tableName, TableCreateMode createMode = TableCreateMode.CreateIfNotExists)
        {
            tableName = string.IsNullOrEmpty(tableName) ? typeDescriptor.Table.Name : tableName;

            List<string> sqlList = new List<string>();

            if (createMode == TableCreateMode.CreateIfNotExists)
            {
                bool tableExists = this.TableExists(tableName);
                if (tableExists)
                {
                    return sqlList;
                }
            }
            else if (createMode == TableCreateMode.CreateNew)
            {
                bool tableExists = this.TableExists(tableName);
                if (tableExists)
                    sqlList.Add($"DROP TABLE {this.QuoteName(tableName)}");
            }

            StringBuilder sb = new StringBuilder();
            sb.Append($"CREATE TABLE {this.QuoteName(tableName)}(");

            string c = "";
            foreach (var propertyDescriptor in typeDescriptor.PrimitivePropertyDescriptors.OrderBy(a => GetTypeInheritLayer(a.Property.DeclaringType)))
            {
                sb.AppendLine(c);
                sb.Append($"  {this.BuildColumnPart(propertyDescriptor)}");
                c = ",";
            }

            sb.AppendLine();
            sb.Append(")");

            sqlList.Add(sb.ToString());

            if (typeDescriptor.PrimaryKeys.Count > 0)
            {
                string key = typeDescriptor.PrimaryKeys.First().Column.Name;
                sqlList.Add($"ALTER TABLE {this.QuoteName(tableName)} ADD CHECK ({this.QuoteName(key)} IS NOT NULL)");

                sqlList.Add($"ALTER TABLE {this.QuoteName(tableName)} ADD PRIMARY KEY ({this.QuoteName(key)})");
            }

            if (typeDescriptor.AutoIncrement != null)
            {
                string seqName = typeDescriptor.AutoIncrement.Definition.SequenceName;
                if (string.IsNullOrEmpty(seqName))
                {
                    seqName = $"{tableName.ToUpper()}";
                }

                bool seqExists = this.SequenceExists(seqName);
                if (!seqExists)
                {
                    string seqScript = this.BuildCreateSequenceSql(seqName);
                    sqlList.Add(seqScript);
                }

                string triggerName = $"{seqName.ToUpper()}_TRIGGER";
                string createTrigger = $@"create or replace trigger {triggerName} before insert on {tableName.ToUpper()} for each row 
begin 
select {seqName.ToUpper()}.nextval into :new.{typeDescriptor.AutoIncrement.Column.Name} from dual;
end;";

                sqlList.Add(createTrigger);
            }

            var seqProperties = typeDescriptor.PrimitivePropertyDescriptors.Where(a => a.HasSequence());
            foreach (var seqProperty in seqProperties)
            {
                if (seqProperty == typeDescriptor.AutoIncrement)
                {
                    continue;
                }

                string seqName = seqProperty.Definition.SequenceName;
                bool seqExists = this.SequenceExists(seqName);

                if (!seqExists)
                {
                    string seqScript = this.BuildCreateSequenceSql(seqName);
                    sqlList.Add(seqScript);
                }
            }

            XDocument commentDoc = GetAssemblyCommentDoc(typeDescriptor.Definition.Type.Assembly);
            sqlList.AddRange(this.GenColumnCommentScripts(typeDescriptor, commentDoc));

            return sqlList;
        }

        string SqlName(string name)
        {
            OracleContext dbContext = (this.DbContext as OracleContext);
            OracleContextProvider dbContextProvider = (OracleContextProvider)dbContext.DefaultDbContextProvider;
            if (dbContextProvider.ConvertToUppercase)
                return name.ToUpper();

            return name;
        }
        string QuoteName(string name)
        {
            OracleContext dbContext = (this.DbContext as OracleContext);
            OracleContextProvider dbContextProvider = (OracleContextProvider)dbContext.DefaultDbContextProvider;
            return Utils.QuoteName(name, dbContextProvider.ConvertToUppercase);
        }

        bool TableExists(string tableName)
        {
            bool exists = this.DbContext.SqlQuery<int>($"select count(1) from user_tables where TABLE_NAME = '{this.SqlName(tableName)}'").First() > 0;
            return exists;
        }
        bool SequenceExists(string seqName)
        {
            bool exists = this.DbContext.SqlQuery<int>($"select count(1) from user_sequences where SEQUENCE_NAME='{seqName}'").First() > 0;
            return exists;
        }
        string BuildCreateSequenceSql(string seqName)
        {
            string seqScript = $"CREATE SEQUENCE {this.QuoteName(seqName)} INCREMENT BY 1 MINVALUE 1 MAXVALUE 9999999999999999999999999999 START WITH 1 CACHE 20";

            return seqScript;
        }

        string BuildColumnPart(PrimitivePropertyDescriptor propertyDescriptor)
        {
            string part = $"{this.QuoteName(propertyDescriptor.Column.Name)} {this.GetDataTypeName(propertyDescriptor)}";

            if (!propertyDescriptor.IsPrimaryKey)
            {
                if (!propertyDescriptor.IsNullable)
                {
                    part += " NOT NULL";
                }
                else
                {
                    part += " NULL";
                }
            }

            return part;
        }
        string GetDataTypeName(PrimitivePropertyDescriptor propertyDescriptor)
        {
            if (propertyDescriptor.TryGetAnnotation(typeof(DataTypeAttribute), out var annotation))
            {
                return (annotation as DataTypeAttribute).Name;
            }

            Type type = propertyDescriptor.PropertyType.GetUnderlyingType();
            if (type.IsEnum)
            {
                type = type.GetEnumUnderlyingType();
            }

            if (type == typeof(string))
            {
                int stringLength;
                if (propertyDescriptor.IsPrimaryKey)
                {
                    stringLength = propertyDescriptor.Column.Size ?? this.Options.DefaultStringKeyLength;
                }
                else
                {
                    stringLength = propertyDescriptor.Column.Size ?? this.Options.DefaultStringLength;
                }
                return $"NVARCHAR2({stringLength})";
            }


            if (type == typeof(byte))
            {
                return "NUMBER(3,0)";
            }

            if (type == typeof(Int16))
            {
                return "NUMBER(5,0)";
            }

            if (type == typeof(int))
            {
                return "NUMBER(10,0)";
            }

            if (type == typeof(long))
            {
                return "NUMBER(19,0)";
            }

            if (type == typeof(float))
            {
                return "BINARY_FLOAT";
            }

            if (type == typeof(double))
            {
                return "BINARY_DOUBLE";
            }

            if (type == typeof(decimal))
            {
                return "NUMBER";
            }

            if (type == typeof(bool))
            {
                return "NUMBER(10,0)";
            }

            if (type == typeof(DateTime))
            {
                return "DATE";
            }

            if (type == typeof(Guid))
            {
                return "BLOB";
            }

            throw new NotSupportedException(type.FullName);
        }

        List<string> GenColumnCommentScripts(TypeDescriptor typeDescriptor, XDocument commentDoc)
        {
            return typeDescriptor.PrimitivePropertyDescriptors.Select(a => this.GenCommentScript(a, commentDoc)).Where(a => !string.IsNullOrEmpty(a)).ToList();
        }
        string GenCommentScript(PrimitivePropertyDescriptor propertyDescriptor, XDocument commentDoc)
        {
            string comment = FindComment(propertyDescriptor, commentDoc);
            if (string.IsNullOrEmpty(comment))
                return null;

            string tableName = propertyDescriptor.DeclaringTypeDescriptor.Table.Name;
            string columnName = propertyDescriptor.Column.Name;
            string str = $"COMMENT ON COLUMN {this.QuoteName(tableName)}.{this.QuoteName(columnName)} IS '{comment}'";

            return str;
        }
    }
}
