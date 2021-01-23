using System;
using System.Collections.Generic;
using System.Text;

namespace Chloe
{
    public static class ChloeDiagnosticListenerNames
    {
        public const string ChloePrefix = "Chloe";

        public const string DiagnosticListenerName = "ChloeDiagnosticListener";

        public const string ReaderExecuting = ChloePrefix + ".ReaderExecuting";
        public const string ReaderExecuted = ChloePrefix + ".ReaderExecuted";

        public const string NonQueryExecuting = ChloePrefix + ".NonQueryExecuting";
        public const string NonQueryExecuted = ChloePrefix + ".NonQueryExecuted";

        public const string ScalarExecuting = ChloePrefix + ".ScalarExecuting";
        public const string ScalarExecuted = ChloePrefix + ".ScalarExecuted";

    }

    public class ChloeDbCommandEventData
    {
        public string CommandText { get; set; }

        public long? ElapsedTime { get; set; }

        public Exception Exception { get; set; }

        public List<DbCommandParam> Parameters { get; set; } = new List<DbCommandParam>();
    }

    public class DbCommandParam
    {
        public string Name { get; set; }
        public object Value { get; set; }
        public System.Data.DbType DbType { get; set; }
    }
}
