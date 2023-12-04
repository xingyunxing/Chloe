using Chloe.RDBMS.DDL;
using Chloe.Descriptors;
using Chloe.Reflection;
using System.Xml.Linq;

namespace Chloe.KingbaseES.DDL
{
    public class KingbaseESTableGenerator : TableGenerator
    {
        public KingbaseESTableGenerator(IDbContext dbContext) : base(dbContext)
        {

        }
        public KingbaseESTableGenerator(IDbContext dbContext, TableGenerateOptions options) : base(dbContext, options)
        {
        }

        string QuoteName(string name)
        {
            return name;
        }

        public override List<string> GenCreateTableScript(TypeDescriptor typeDescriptor, string tableName, TableCreateMode createMode = TableCreateMode.CreateIfNotExists)
        {
            tableName = string.IsNullOrEmpty(tableName) ? typeDescriptor.Table.Name : tableName;
            string schema = typeDescriptor.Table.Schema ?? "public";

            StringBuilder sb = new StringBuilder();

            if (createMode == TableCreateMode.CreateIfNotExists)
            {
                sb.Append($"CREATE TABLE IF NOT EXISTS {QuoteName(schema)}.{QuoteName(tableName)}(");
            }
            else if (createMode == TableCreateMode.CreateNew)
            {
                sb.AppendLine($"DROP TABLE IF EXISTS {QuoteName(schema)}.{QuoteName(tableName)};");
                sb.Append($"CREATE TABLE {QuoteName(schema)}.{QuoteName(tableName)}(");
            }
            else
            {
                sb.Append($"CREATE TABLE {QuoteName(schema)}.{QuoteName(tableName)}(");
            }


            string c = "";
            foreach (var propertyDescriptor in typeDescriptor.PrimitivePropertyDescriptors.OrderBy(a => GetTypeInheritLayer(a.Property.DeclaringType)))
            {
                sb.AppendLine(c);
                sb.Append($"  {this.BuildColumnPart(propertyDescriptor)}");
                c = ",";
            }

            if (typeDescriptor.PrimaryKeys.Count > 0)
            {
                string key = typeDescriptor.PrimaryKeys.First().Column.Name;
                sb.AppendLine(c);
                sb.Append($"  PRIMARY KEY ({QuoteName(key)})");
            }

            sb.AppendLine();
            sb.Append(");");

            List<string> scripts = new List<string>();
            scripts.Add(sb.ToString());

            XDocument commentDoc = GetAssemblyCommentDoc(typeDescriptor.Definition.Type.Assembly);
            scripts.AddRange(this.GenColumnCommentScripts(typeDescriptor, commentDoc));

            return scripts;
        }

        string BuildColumnPart(PrimitivePropertyDescriptor propertyDescriptor)
        {
            string part = $"{QuoteName(propertyDescriptor.Column.Name)} {this.GetDataTypeName(propertyDescriptor)}";

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

                return $"varchar({stringLength})";
            }

            if (type == typeof(int))
            {
                return propertyDescriptor.IsAutoIncrement ? "serial" : "integer";
            }

            if (type == typeof(byte))
            {
                return "smallint";
            }

            if (type == typeof(Int16))
            {
                return propertyDescriptor.IsAutoIncrement ? "smallserial" : "smallint";
            }

            if (type == typeof(long))
            {
                return propertyDescriptor.IsAutoIncrement ? "bigserial" : "bigint";
            }

            if (type == typeof(float))
            {
                return "real";
            }

            if (type == typeof(double))
            {
                return "double";
            }

            if (type == typeof(decimal))
            {
                return "numeric";
            }

            if (type == typeof(bool))
            {
                return "boolean";
            }

            if (type == typeof(DateTime))
            {
                return "datetime";
            }

            if (type == typeof(Guid))
            {
                return "uuid";
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

            string schema = propertyDescriptor.DeclaringTypeDescriptor.Table.Schema ?? "public";
            string tableName = propertyDescriptor.DeclaringTypeDescriptor.Table.Name;
            string columnName = propertyDescriptor.Column.Name;
            string str = $"COMMENT ON COLUMN {QuoteName(schema)}.{QuoteName(tableName)}.{QuoteName(columnName)} IS '{comment}';";

            return str;
        }
    }
}
