using Chloe.DbExpressions;

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
    }
}
