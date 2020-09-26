using Chloe;
using Chloe.Descriptors;
using Chloe.Infrastructure;
using Chloe.SQLite;
using Chloe.Reflection;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChloeDemo
{
    class SQLiteDemo : DemoBase
    {
        public SQLiteDemo()
        {
            DbConfiguration.UseTypeBuilders(typeof(TestEntityMap));
        }

        protected override IDbContext CreateDbContext()
        {
            IDbContext dbContext = new SQLiteContext(new SQLiteConnectionFactory("Data Source=..\\..\\..\\Chloe.db;"));

            return dbContext;
        }

        public override void InitTable<TEntity>()
        {
            Type entityType = typeof(TEntity);

            string createTableScript = this.CreateTableScript(entityType);

            this.DbContext.Session.ExecuteNonQuery(createTableScript);
        }
        string CreateTableScript(Type entityType)
        {
            TypeDescriptor typeDescriptor = EntityTypeContainer.GetDescriptor(entityType);
            string tableName = typeDescriptor.Table.Name;

            StringBuilder sb = new StringBuilder();
            sb.Append($"CREATE TABLE IF NOT EXISTS {this.QuoteName(tableName)}(");

            string c = "";
            foreach (var propertyDescriptor in typeDescriptor.PrimitivePropertyDescriptors)
            {
                sb.AppendLine(c);
                sb.Append($"  {this.BuildColumnPart(propertyDescriptor)}");
                c = ",";
            }

            sb.AppendLine();
            sb.Append(");");

            return sb.ToString();
        }
        string QuoteName(string name)
        {
            return string.Concat("\"", name, "\"");
        }

        string BuildColumnPart(PrimitivePropertyDescriptor propertyDescriptor)
        {
            string part = $"{this.QuoteName(propertyDescriptor.Column.Name)} {this.GetMappedDbTypeName(propertyDescriptor)}";

            if (propertyDescriptor.IsPrimaryKey)
            {
                part += " PRIMARY KEY";
            }

            if (propertyDescriptor.IsAutoIncrement)
            {
                part += " AUTOINCREMENT";
            }

            if (!propertyDescriptor.IsNullable)
            {
                part += " NOT NULL";
            }
            else
            {
                part += " NULL";
            }

            return part;
        }

        string GetMappedDbTypeName(PrimitivePropertyDescriptor propertyDescriptor)
        {
            Type type = propertyDescriptor.PropertyType.GetUnderlyingType();
            if (type.IsEnum)
            {
                type = type.GetEnumUnderlyingType();
            }

            if (type == typeof(string))
            {
                int stringLength = propertyDescriptor.Column.Size ?? 4000;
                return $"NVARCHAR({stringLength})";
            }

            if (type == typeof(int))
            {
                if (propertyDescriptor.IsAutoIncrement)
                {
                    return "INTEGER";
                }

                return "INT";
            }

            if (type == typeof(byte))
            {
                return "TINYINT";
            }

            if (type == typeof(Int16))
            {
                return "SMALLINT";
            }

            if (type == typeof(long))
            {
                return "INT64";
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
                return "NUMERIC";
            }

            if (type == typeof(bool))
            {
                return "BOOL";
            }

            if (type == typeof(DateTime))
            {
                return "DATETIME";
            }

            if (type == typeof(Guid))
            {
                return "GUID";
            }

            throw new NotSupportedException(type.FullName);
        }
    }
}
