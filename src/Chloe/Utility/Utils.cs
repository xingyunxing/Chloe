using Chloe.DbExpressions;
using Chloe.QueryExpressions;
using System.Linq.Expressions;

namespace Chloe
{
    static class Utils
    {
        public static string GenerateUniqueColumnAlias(HashSet<string> aliasSet, bool autoAddToAliasSet = true, string defaultAlias = UtilConstants.DefaultColumnAlias)
        {
            string alias = defaultAlias;
            int i = 0;
            while (aliasSet.Contains(alias)) //HasSet 查找效率快
            {
                alias = defaultAlias + i.ToString();
                i++;
            }

            if (autoAddToAliasSet)
            {
                aliasSet.Add(alias);
            }

            return alias;
        }

        public static DbJoinType AsDbJoinType(this JoinType joinType)
        {
            switch (joinType)
            {
                case JoinType.InnerJoin:
                    return DbJoinType.InnerJoin;
                case JoinType.LeftJoin:
                    return DbJoinType.LeftJoin;
                case JoinType.RightJoin:
                    return DbJoinType.RightJoin;
                case JoinType.FullJoin:
                    return DbJoinType.FullJoin;
                default:
                    throw new NotSupportedException();
            }
        }

        public static Type GetFuncDelegateType(params Type[] typeArguments)
        {
            int parameters = typeArguments.Length;
            Type funcType = null;
            switch (parameters)
            {
                case 2:
                    funcType = typeof(Func<,>);
                    break;
                case 3:
                    funcType = typeof(Func<,,>);
                    break;
                case 4:
                    funcType = typeof(Func<,,,>);
                    break;
                case 5:
                    funcType = typeof(Func<,,,,>);
                    break;
                case 6:
                    funcType = typeof(Func<,,,,,>);
                    break;
                default:
                    throw new NotSupportedException();
            }

            return funcType.MakeGenericType(typeArguments);
        }
        public static bool IsAutoIncrementType(Type t)
        {
            return t == typeof(Int16) || t == typeof(Int32) || t == typeof(Int64);
        }

        public static bool IsIQueryType(Type type)
        {
            if (!type.IsGenericType)
                return false;

            Type queryType = typeof(IQuery<>);
            if (queryType == type.GetGenericTypeDefinition())
                return true;

            Type implementedInterface = type.GetInterface("IQuery`1");
            if (implementedInterface == null)
                return false;

            implementedInterface = implementedInterface.GetGenericTypeDefinition();
            return queryType == implementedInterface;
        }

        /// <summary>
        /// 判断是否是变量包装类型
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public static bool IsVariableWrapperType(Type type)
        {
            return type.Name.StartsWith("<>c__"); //<>c__DisplayClass1_0 变量包装类型
        }

        /// <summary>
        /// 判断是否是 Chloe.ConstantWrapper`1 类型
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public static bool IsChloeConstantWrapperType(Type type)
        {
            return type.Name == "ConstantWrapper`1" && type.Namespace == "Chloe";
        }

        /// <summary>
        /// 判断表达式树是否是对 QueryExpression 的包装对象
        /// </summary>
        /// <param name="exp"></param>
        /// <returns></returns>
        public static bool IsQueryExpressionWrapper(Expression exp)
        {
            if (exp.NodeType == ExpressionType.Convert)
            {
                Expression operandExp = (exp as UnaryExpression).Operand;
                if (operandExp.NodeType == ExpressionType.Constant)
                {
                    ConstantExpression constantExpression = (ConstantExpression)operandExp;
                    if (constantExpression.Value != null && (constantExpression.Value is QueryExpression))
                    {
                        return true;
                    }
                }
            }

            return false;
        }
    }
}
