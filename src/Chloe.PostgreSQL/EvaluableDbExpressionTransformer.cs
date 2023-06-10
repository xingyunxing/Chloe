using Chloe.Visitors;
using Chloe.DbExpressions;
using System.Reflection;

namespace Chloe.PostgreSQL
{
    class EvaluableDbExpressionTransformer : EvaluableDbExpressionTransformerBase
    {
        static HashSet<MemberInfo> _toTranslateMembers = new HashSet<MemberInfo>();
        static EvaluableDbExpressionTransformer _transformer = new EvaluableDbExpressionTransformer();

        static EvaluableDbExpressionTransformer()
        {
            _toTranslateMembers.Add(PublicConstants.PropertyInfo_String_Length);

            _toTranslateMembers.Add(PublicConstants.PropertyInfo_DateTime_Now);
            //_toTranslateMembers.Add(PublicConstants.PropertyInfo_DateTime_UtcNow);
            _toTranslateMembers.Add(PublicConstants.PropertyInfo_DateTime_Today);
            _toTranslateMembers.Add(PublicConstants.PropertyInfo_DateTime_Date);

            _toTranslateMembers.Add(PublicConstants.PropertyInfo_DateTime_Year);
            _toTranslateMembers.Add(PublicConstants.PropertyInfo_DateTime_Month);
            _toTranslateMembers.Add(PublicConstants.PropertyInfo_DateTime_Day);
            _toTranslateMembers.Add(PublicConstants.PropertyInfo_DateTime_Hour);
            _toTranslateMembers.Add(PublicConstants.PropertyInfo_DateTime_Minute);
            _toTranslateMembers.Add(PublicConstants.PropertyInfo_DateTime_Second);
            _toTranslateMembers.Add(PublicConstants.PropertyInfo_DateTime_Millisecond);
            _toTranslateMembers.Add(PublicConstants.PropertyInfo_DateTime_DayOfWeek);

            _toTranslateMembers.TrimExcess();
        }

        public EvaluableDbExpressionTransformer()
        {
            this.ToTranslateMembers = _toTranslateMembers;
            this.MethodHandlers = SqlGenerator.MethodHandlerDic;
        }

        public static DbExpression Transform(DbExpression exp)
        {
            return exp.Accept(_transformer);
        }
    }
}
