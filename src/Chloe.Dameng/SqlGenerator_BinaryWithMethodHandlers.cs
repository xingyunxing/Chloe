using Chloe.DbExpressions;
using Chloe.RDBMS;
using System.Reflection;

namespace Chloe.Dameng
{
    partial class SqlGenerator : SqlGeneratorBase
    {
        static Dictionary<MethodInfo, Action<DbBinaryExpression, SqlGeneratorBase>> InitBinaryWithMethodHandlers()
        {
            var binaryWithMethodHandlers = new Dictionary<MethodInfo, Action<DbBinaryExpression, SqlGeneratorBase>>
            {
                { PublicConstants.MethodInfo_String_Concat_String_String, StringConcat },
                { PublicConstants.MethodInfo_String_Concat_Object_Object, StringConcat }
            };

            var ret = PublicHelper.Clone(binaryWithMethodHandlers);
            return ret;
        }

        static void StringConcat(DbBinaryExpression exp, SqlGeneratorBase generator)
        {
            generator.SqlBuilder.Append("CONCAT(");
            exp.Left.Accept(generator);
            generator.SqlBuilder.Append(",");
            exp.Right.Accept(generator);
            generator.SqlBuilder.Append(")");
        }
    }
}
