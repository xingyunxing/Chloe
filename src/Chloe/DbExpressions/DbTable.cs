namespace Chloe.DbExpressions
{
    [System.Diagnostics.DebuggerDisplay("Name = {Name}")]
    public class DbTable
    {
        string _name;
        string _schema;

        public DbTable(string name) : this(name, null)
        {

        }

        public DbTable(string name, string schema)
        {
            if (string.IsNullOrEmpty(name))
            {
                throw new ArgumentException("Table name could not be null or empty.");
            }

            this._name = name;
            this._schema = schema;
        }

        public string Name { get { return this._name; } }
        public string Schema { get { return this._schema; } }
    }

    public class DbTableEqualityComparer : IEqualityComparer<DbTable>
    {
        public static DbTableEqualityComparer Instance { get; } = new DbTableEqualityComparer();

        public bool Equals(DbTable left, DbTable right)
        {
            if (left == right)
                return true;

            if (left == null || right == null)
                return false;

            return left.Name == right.Name && left.Schema == right.Schema;
        }

        public int GetHashCode(DbTable obj)
        {
            HashCode hash = new HashCode();
            hash.Add(obj.Schema);
            hash.Add(obj.Name);
            return hash.ToHashCode();
        }
    }

    public struct DbTableKey : IEquatable<DbTableKey>
    {
        DbTable _dbTable;

        public DbTableKey(DbTable dbTable)
        {
            this._dbTable = dbTable;
        }

        public override bool Equals(object? obj)
        {
            return obj is DbTableKey other && Equals(other);
        }

        public bool Equals(DbTableKey other)
        {
            return DbTableEqualityComparer.Instance.Equals(this._dbTable, other._dbTable);
        }

        public override int GetHashCode()
        {
            return DbTableEqualityComparer.Instance.GetHashCode(this._dbTable);
        }
    }
}
