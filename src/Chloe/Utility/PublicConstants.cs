using Chloe.DbExpressions;
using System.Linq.Expressions;
using System.Reflection;

namespace Chloe
{
    public static class PublicConstants
    {
        static PublicConstants()
        {
            Expression<Func<bool>> e = () => Enumerable.Contains((IEnumerable<int>)null, 0);
            MethodInfo method_Enumerable_Contains = (e.Body as MethodCallExpression).Method.GetGenericMethodDefinition();
            MethodInfo_Enumerable_Contains = method_Enumerable_Contains;

            MethodInfo_Sql_Sum_DecimalN = typeof(Sql).GetMethod(nameof(Sql.Sum), new Type[1] { typeof(decimal?) });

            Expression<Func<long, long>> longCountExp = a => Sql.LongCount<long>(a);
            MethodInfo_Sql_LongCount = (longCountExp.Body as MethodCallExpression).Method.GetGenericMethodDefinition();
        }

        public static readonly object[] EmptyArray = new object[0];

        #region Types
        public static readonly Type TypeOfVoid = typeof(void);
        public static readonly Type TypeOfInt16 = typeof(Int16);
        public static readonly Type TypeOfInt32 = typeof(Int32);
        public static readonly Type TypeOfInt64 = typeof(Int64);
        public static readonly Type TypeOfDecimal = typeof(Decimal);
        public static readonly Type TypeOfDouble = typeof(Double);
        public static readonly Type TypeOfSingle = typeof(Single);
        public static readonly Type TypeOfBoolean = typeof(Boolean);
        public static readonly Type TypeOfBoolean_Nullable = typeof(Boolean?);
        public static readonly Type TypeOfDateTime = typeof(DateTime);
        public static readonly Type TypeOfGuid = typeof(Guid);
        public static readonly Type TypeOfByte = typeof(Byte);
        public static readonly Type TypeOfChar = typeof(Char);
        public static readonly Type TypeOfString = typeof(String);
        public static readonly Type TypeOfObject = typeof(Object);
        public static readonly Type TypeOfByteArray = typeof(Byte[]);
        public static readonly Type TypeOfTimeSpan = typeof(TimeSpan);

        public static readonly Type TypeOfSql = typeof(Sql);
        public static readonly Type TypeOfMath = typeof(Math);
        #endregion


        #region Sql
        public static readonly MethodInfo MethodInfo_Sql_IsEqual = typeof(Sql).GetMethod(nameof(Sql.IsEqual));
        public static readonly MethodInfo MethodInfo_Sql_IsNotEqual = typeof(Sql).GetMethod(nameof(Sql.IsNotEqual));
        public static readonly MethodInfo MethodInfo_Sql_NextValueForSequence = typeof(Sql).GetMethod(nameof(Sql.NextValueForSequence));

        public static MethodInfo MethodInfo_Sql_Sum_DecimalN;
        public static MethodInfo MethodInfo_Sql_LongCount;
        #endregion


        #region string
        public static readonly PropertyInfo PropertyInfo_String_Length = typeof(string).GetProperty(nameof(string.Length));

        public static readonly MethodInfo MethodInfo_String_Concat_String_String = typeof(string).GetMethod(nameof(string.Concat), new Type[] { typeof(string), typeof(string) });
        public static readonly MethodInfo MethodInfo_String_Concat_Object_Object = typeof(string).GetMethod(nameof(string.Concat), new Type[] { typeof(object), typeof(object) });
        public static readonly MethodInfo MethodInfo_String_Trim = typeof(string).GetMethod(nameof(string.Trim), Type.EmptyTypes);
        public static readonly MethodInfo MethodInfo_String_TrimStart = typeof(string).GetMethod(nameof(string.TrimStart), new Type[] { typeof(char[]) });
        public static readonly MethodInfo MethodInfo_String_TrimEnd = typeof(string).GetMethod(nameof(string.TrimEnd), new Type[] { typeof(char[]) });
        public static readonly MethodInfo MethodInfo_String_StartsWith = typeof(string).GetMethod(nameof(string.StartsWith), new Type[] { typeof(string) });
        public static readonly MethodInfo MethodInfo_String_EndsWith = typeof(string).GetMethod(nameof(string.EndsWith), new Type[] { typeof(string) });
        public static readonly MethodInfo MethodInfo_String_Contains = typeof(string).GetMethod(nameof(string.Contains), new Type[] { typeof(string) });
        public static readonly MethodInfo MethodInfo_String_IsNullOrEmpty = typeof(string).GetMethod(nameof(string.IsNullOrEmpty), new Type[] { typeof(string) });
        public static readonly MethodInfo MethodInfo_String_ToUpper = typeof(string).GetMethod(nameof(string.ToUpper), Type.EmptyTypes);
        public static readonly MethodInfo MethodInfo_String_ToLower = typeof(string).GetMethod(nameof(string.ToLower), Type.EmptyTypes);
        public static readonly MethodInfo MethodInfo_String_Substring_Int32 = typeof(string).GetMethod(nameof(string.Substring), new Type[] { typeof(Int32) });
        public static readonly MethodInfo MethodInfo_String_Substring_Int32_Int32 = typeof(string).GetMethod(nameof(string.Substring), new Type[] { typeof(Int32), typeof(Int32) });
        public static readonly MethodInfo MethodInfo_String_Replace = typeof(string).GetMethod(nameof(string.Replace), new Type[] { typeof(string), typeof(string) });
        #endregion


        #region DateTime
        public static readonly PropertyInfo PropertyInfo_DateTime_Now = typeof(DateTime).GetProperty(nameof(DateTime.Now));
        public static readonly PropertyInfo PropertyInfo_DateTime_UtcNow = typeof(DateTime).GetProperty(nameof(DateTime.UtcNow));
        public static readonly PropertyInfo PropertyInfo_DateTime_Today = typeof(DateTime).GetProperty(nameof(DateTime.Today));
        public static readonly PropertyInfo PropertyInfo_DateTime_Date = typeof(DateTime).GetProperty(nameof(DateTime.Date));
        public static readonly PropertyInfo PropertyInfo_DateTime_Year = typeof(DateTime).GetProperty(nameof(DateTime.Year));
        public static readonly PropertyInfo PropertyInfo_DateTime_Month = typeof(DateTime).GetProperty(nameof(DateTime.Month));
        public static readonly PropertyInfo PropertyInfo_DateTime_Day = typeof(DateTime).GetProperty(nameof(DateTime.Day));
        public static readonly PropertyInfo PropertyInfo_DateTime_Hour = typeof(DateTime).GetProperty(nameof(DateTime.Hour));
        public static readonly PropertyInfo PropertyInfo_DateTime_Minute = typeof(DateTime).GetProperty(nameof(DateTime.Minute));
        public static readonly PropertyInfo PropertyInfo_DateTime_Second = typeof(DateTime).GetProperty(nameof(DateTime.Second));
        public static readonly PropertyInfo PropertyInfo_DateTime_Millisecond = typeof(DateTime).GetProperty(nameof(DateTime.Millisecond));
        public static readonly PropertyInfo PropertyInfo_DateTime_DayOfWeek = typeof(DateTime).GetProperty(nameof(DateTime.DayOfWeek));

        public static readonly MethodInfo MethodInfo_DateTime_Subtract_DateTime = typeof(DateTime).GetMethod(nameof(DateTime.Subtract), new Type[] { typeof(DateTime) });
        #endregion

        #region DateTimeOffset
        public static readonly PropertyInfo PropertyInfo_DateTimeOffset_Now = typeof(DateTimeOffset).GetProperty(nameof(DateTimeOffset.Now));
        public static readonly PropertyInfo PropertyInfo_DateTimeOffset_UtcNow = typeof(DateTimeOffset).GetProperty(nameof(DateTimeOffset.UtcNow));
        public static readonly PropertyInfo PropertyInfo_DateTimeOffset_Date = typeof(DateTimeOffset).GetProperty(nameof(DateTimeOffset.Date));
        public static readonly PropertyInfo PropertyInfo_DateTimeOffset_Year = typeof(DateTimeOffset).GetProperty(nameof(DateTimeOffset.Year));
        public static readonly PropertyInfo PropertyInfo_DateTimeOffset_Month = typeof(DateTimeOffset).GetProperty(nameof(DateTimeOffset.Month));
        public static readonly PropertyInfo PropertyInfo_DateTimeOffset_Day = typeof(DateTimeOffset).GetProperty(nameof(DateTimeOffset.Day));
        public static readonly PropertyInfo PropertyInfo_DateTimeOffset_Hour = typeof(DateTimeOffset).GetProperty(nameof(DateTimeOffset.Hour));
        public static readonly PropertyInfo PropertyInfo_DateTimeOffset_Minute = typeof(DateTimeOffset).GetProperty(nameof(DateTimeOffset.Minute));
        public static readonly PropertyInfo PropertyInfo_DateTimeOffset_Second = typeof(DateTimeOffset).GetProperty(nameof(DateTimeOffset.Second));
        public static readonly PropertyInfo PropertyInfo_DateTimeOffset_Millisecond = typeof(DateTimeOffset).GetProperty(nameof(DateTimeOffset.Millisecond));
        public static readonly PropertyInfo PropertyInfo_DateTimeOffset_DayOfWeek = typeof(DateTimeOffset).GetProperty(nameof(DateTimeOffset.DayOfWeek));

        public static readonly MethodInfo MethodInfo_DateTimeOffset_Subtract_DateTime = typeof(DateTimeOffset).GetMethod(nameof(DateTimeOffset.Subtract), new Type[] { typeof(DateTimeOffset) });
        #endregion


        #region DbExpression
        public static readonly DbParameterExpression DbParameter_1 = DbExpression.Parameter(1);
        public static readonly DbConstantExpression DbConstant_Null_String = DbExpression.Constant(null, typeof(string));
        #endregion

        public static readonly MethodInfo MethodInfo_Guid_NewGuid = typeof(Guid).GetMethod(nameof(Guid.NewGuid));

        public static MethodInfo MethodInfo_Enumerable_Contains { get; private set; }
    }
}
