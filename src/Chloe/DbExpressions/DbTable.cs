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

        public override int GetHashCode()
        {
#if !NET46 && !NETSTANDARD2

            HashCode hash = new HashCode();
            hash.Add(this._name);
            hash.Add(this._schema);

            return hash.ToHashCode();
#else
            return string.Format("{0}-{1}", this._name, this._schema).GetHashCode();
#endif
        }

        public override bool Equals(object obj)
        {
            DbTable dbTable = obj as DbTable;
            if (dbTable == null)
            {
                return false;
            }

            return this._name == dbTable._name && this._schema == dbTable._schema;
        }
    }
}
