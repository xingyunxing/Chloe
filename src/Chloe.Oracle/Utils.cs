using Chloe.Reflection;
using System.Reflection;

namespace Chloe.Oracle
{
    internal static class Utils
    {
        public static string ToMethodString(this MethodInfo method)
        {
            StringBuilder sb = new StringBuilder();
            ParameterInfo[] parameters = method.GetParameters();

            for (int i = 0; i < parameters.Length; i++)
            {
                ParameterInfo p = parameters[i];

                if (i > 0)
                    sb.Append(",");

                string s = null;
                if (p.IsOut)
                    s = "out ";

                sb.AppendFormat("{0}{1} {2}", s, p.ParameterType.Name, p.Name);
            }

            return string.Format("{0}.{1}({2})", method.DeclaringType.Name, method.Name, sb.ToString());
        }

        public static List<List<T>> InBatches<T>(List<T> source, int batchSize)
        {
            List<List<T>> batches = new List<List<T>>();

            List<T> batch = new List<T>(source.Count > batchSize ? batchSize : source.Count);
            for (int i = 0; i < source.Count; i++)
            {
                var item = source[i];
                batch.Add(item);
                if (batch.Count >= batchSize)
                {
                    batches.Add(batch);
                    batch = new List<T>();
                }
            }

            if (batch.Count > 0)
            {
                batches.Add(batch);
            }

            return batches;
        }

        public static string GenOutputColumnParameterName(string columnName)
        {
            return UtilConstants.OutputParameterNamePrefix + columnName;
        }

        public static string QuoteName(string name, bool convertToUppercase)
        {
            if (convertToUppercase)
                return string.Concat("\"", name.ToUpper(), "\"");

            return string.Concat("\"", name, "\"");
        }
    }
}
