using Chloe.DDL;
using Chloe.Descriptors;
using Chloe.Reflection;
using System.Xml.Linq;

namespace Chloe.Dameng.DDL
{
    public class DamengTableGenerator : TableGenerator
    {
        public DamengTableGenerator(IDbContext dbContext) : base(dbContext)
        {
        }

        public DamengTableGenerator(IDbContext dbContext, TableGenerateOptions options) : base(dbContext, options)
        {
        }

        public override List<string> GenCreateTableScript(TypeDescriptor typeDescriptor, string tableName, TableCreateMode createMode = TableCreateMode.CreateIfNotExists)
        {
            tableName = string.IsNullOrEmpty(tableName) ? typeDescriptor.Table.Name : tableName;

            var sb = new StringBuilder();
            sb.AppendLine("begin");
            if (createMode == TableCreateMode.CreateIfNotExists)
            {
                sb.Append($"execute immediate ' CREATE TABLE IF NOT EXISTS {Utils.QuoteName(tableName)}(");
            }
            else if (createMode == TableCreateMode.CreateNew)
            {
                sb.AppendLine($"execute immediate ' DROP TABLE IF EXISTS {Utils.QuoteName(tableName)}';");
                sb.Append($"execute immediate ' CREATE TABLE {Utils.QuoteName(tableName)}(");
            }
            else
            {
                sb.Append($"execute immediate ' CREATE TABLE {Utils.QuoteName(tableName)}(");
            }

            string c = "";
            foreach (var propertyDescriptor in typeDescriptor.PrimitivePropertyDescriptors.OrderBy(a => GetTypeInheritLayer(a.Property.DeclaringType)))
            {
                sb.AppendLine(c);
                sb.Append($"  {this.BuildColumnPart(propertyDescriptor)}");
                c = ",";
            }

            sb.AppendLine();
            sb.Append(")';");
            sb.AppendLine();
            sb.AppendLine("end");

            List<string> scripts = new List<string>();
            scripts.Add(sb.ToString());

            XDocument commentDoc = GetAssemblyCommentDoc(typeDescriptor.Definition.Type.Assembly);
            scripts.AddRange(GenColumnCommentScripts(typeDescriptor, commentDoc));
            return scripts;
        }

        private string BuildColumnPart(PrimitivePropertyDescriptor propertyDescriptor)
        {
            string part = $"{Utils.QuoteName(propertyDescriptor.Column.Name)} {this.GetDataTypeName(propertyDescriptor)}";

            if (propertyDescriptor.IsAutoIncrement)
            {
                part += " IDENTITY";
            }

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
            else
            {
            }

            return part;
        }

        private string GetDataTypeName(PrimitivePropertyDescriptor propertyDescriptor)
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

                return $"VARCHAR({stringLength})";
            }

            if (type == typeof(int))
            {
                return "INT";
            }

            if (type == typeof(byte))
            {
                return "BYTE";
            }

            if (type == typeof(Int16))
            {
                return "SMALLINT";
            }

            if (type == typeof(long))
            {
                return "BIGINT";
            }

            if (type == typeof(float))
            {
                return "FLOAT";
            }

            if (type == typeof(double))
            {
                return "DOUBLE";
            }

            if (type == typeof(decimal))
            {
                return "REAL";
            }

            if (type == typeof(bool))
            {
                return "BIT";
            }

            if (type == typeof(DateTime))
            {
                return "TIMESTAMP";
            }

            if (type == typeof(Guid))
            {
                return "VARCHAR(36)";
            }

            throw new NotSupportedException(type.FullName);
        }

        private static List<string> GenColumnCommentScripts(TypeDescriptor typeDescriptor, XDocument commentDoc)
        {
            return typeDescriptor.PrimitivePropertyDescriptors.Select(a => GenCommentScript(a, commentDoc)).Where(a => !string.IsNullOrEmpty(a)).ToList();
        }

        private static string GenCommentScript(PrimitivePropertyDescriptor propertyDescriptor, XDocument commentDoc)
        {
            string comment = FindComment(propertyDescriptor, commentDoc);
            if (string.IsNullOrEmpty(comment))
                return null;

            string tableName = propertyDescriptor.DeclaringTypeDescriptor.Table.Name;
            string columnName = propertyDescriptor.Column.Name;
            string str = $"COMMENT ON COLUMN {Utils.QuoteName(tableName)}.{Utils.QuoteName(columnName)} IS '{comment}'";

            return str;
        }
    }
}