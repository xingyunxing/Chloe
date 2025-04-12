using System.Reflection;

namespace Chloe.Oracle
{
    static class UtilConstants
    {
        public const string LeftQuoteChar = "\"";
        public const string RightQuoteChar = "\"";

        public const string ParameterNamePlaceholder = ":";
        public static readonly string ParameterNamePrefix = ParameterNamePlaceholder + "P_";
        public static readonly string OutputParameterNamePrefix = ParameterNamePlaceholder + "R_";

        #region MemberInfo constants

        /* TimeSpan */
        public static readonly PropertyInfo PropertyInfo_TimeSpan_TotalDays = typeof(TimeSpan).GetProperty("TotalDays");
        public static readonly PropertyInfo PropertyInfo_TimeSpan_TotalHours = typeof(TimeSpan).GetProperty("TotalHours");
        public static readonly PropertyInfo PropertyInfo_TimeSpan_TotalMinutes = typeof(TimeSpan).GetProperty("TotalMinutes");
        public static readonly PropertyInfo PropertyInfo_TimeSpan_TotalSeconds = typeof(TimeSpan).GetProperty("TotalSeconds");
        public static readonly PropertyInfo PropertyInfo_TimeSpan_TotalMilliseconds = typeof(TimeSpan).GetProperty("TotalMilliseconds");

        #endregion

    }
}
