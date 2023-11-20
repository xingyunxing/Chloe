using System.Reflection;

namespace Chloe.Oracle
{
    static class UtilConstants
    {
        public const string LeftQuoteChar = "\"";
        public const string RightQuoteChar = "\"";

        public const string ParameterNamePlaceholer = ":";
        public static readonly string ParameterNamePrefix = ParameterNamePlaceholer + "P_";
        public static readonly string OutputParameterNamePrefix = ParameterNamePlaceholer + "R_";

        /// <summary>
        /// in 参数最大个数
        /// </summary>
        public static int MaxInItems = 1000;

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
