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
            HashCode hash = new HashCode();
            hash.Add(this._schema);
            hash.Add(".");
            hash.Add(this._name);
            return hash.ToHashCode();
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
