using Chloe.DbExpressions;
using System.Collections;
using System.Data;
using System.Reflection;

namespace Chloe.RDBMS.MethodHandlers
{
    public class Contains_HandlerBase : MethodHandlerBase
    {
        public override bool CanProcess(DbMethodCallExpression exp)
        {
            MethodInfo method = exp.Method;

            if (exp.Method == PublicConstants.MethodInfo_String_Contains)
            {
                return true;
            }

            Type declaringType = method.DeclaringType;
            if (typeof(IList).IsAssignableFrom(declaringType) || (declaringType.IsGenericType && typeof(ICollection<>).MakeGenericType(declaringType.GetGenericArguments()).IsAssignableFrom(declaringType)))
            {
                return true;
            }
            if (method.IsStatic && declaringType == typeof(Enumerable) && exp.Arguments.Count == 2)
            {
                return true;
            }

            return false;
        }
        public override void Process(DbMethodCallExpression exp, SqlGeneratorBase generator)
        {
            MethodInfo method = exp.Method;

            if (exp.Method == PublicConstants.MethodInfo_String_Contains)
            {
                Method_String_Contains(exp, generator);
                return;
            }

            List<DbExpression> exps = new List<DbExpression>();
            IEnumerable values = null;
            DbExpression operand = null;

            Type declaringType = method.DeclaringType;

            if (typeof(IList).IsAssignableFrom(declaringType) || (declaringType.IsGenericType && typeof(ICollection<>).MakeGenericType(declaringType.GetGenericArguments()).IsAssignableFrom(declaringType)))
            {
                if (exp.Object.NodeType == DbExpressionType.SqlQuery)
                {
                    /*
                     * where Id in(select id from T)
                     */

                    operand = exp.Arguments[0];
                    In(generator, (DbSqlQueryExpression)exp.Object, operand);
                    return;
                }

                if (!exp.Object.IsEvaluable())
                    throw new NotSupportedException(exp.ToString());

                //list.Contains(value)
                values = DbExpressionExtension.Evaluate(exp.Object) as IEnumerable; //list
                operand = exp.Arguments[0]; //value
                goto constructInState;
            }
            if (method.IsStatic && declaringType == typeof(Enumerable) && exp.Arguments.Count == 2)
            {
                DbExpression arg0 = exp.Arguments[0];
                if (arg0.NodeType == DbExpressionType.SqlQuery)
                {
                    /* where Id in(select id from T) */

                    operand = exp.Arguments[1];
                    In(generator, (DbSqlQueryExpression)arg0, operand);
                    return;
                }

                if (!arg0.IsEvaluable())
                    throw PublicHelper.MakeNotSupportedMethodException(exp.Method);

                //Enumerable.Contains<TSource>(this IEnumerable<TSource> source, TSource value)
                values = DbExpressionExtension.Evaluate(arg0) as IEnumerable; //source
                operand = exp.Arguments[1]; //value
                goto constructInState;
            }

            throw PublicHelper.MakeNotSupportedMethodException(exp.Method);

        constructInState:
            DbType? dbType = null;
            if (operand.NodeType == DbExpressionType.ColumnAccess)
            {
                DbColumnAccessExpression dbColumn = (DbColumnAccessExpression)operand;
                dbType = dbColumn.Column.DbType;
            }

            foreach (object value in values)
            {
                if (value == null)
                    exps.Add(new DbConstantExpression(null, operand.Type));
                else
                {
                    Type valueType = value.GetType();
                    if (valueType.IsEnum)
                        valueType = Enum.GetUnderlyingType(valueType);

                    if (PublicHelper.IsToStringableNumericType(valueType))
                        exps.Add(new DbConstantExpression(value));
                    else
                        exps.Add(new DbParameterExpression(value, value.GetType(), dbType));
                }
            }

            In(generator, exps, operand);
        }

        protected virtual void Method_String_Contains(DbMethodCallExpression exp, SqlGeneratorBase generator)
        {
            PublicHelper.MakeNotSupportedMethodException(exp.Method);
        }

        /// <summary>
        /// in (1,2,3...)
        /// </summary>
        /// <param name="generator"></param>
        /// <param name="elementExps"></param>
        /// <param name="operand"></param>
        protected virtual void In(SqlGeneratorBase generator, List<DbExpression> elementExps, DbExpression operand)
        {
            if (elementExps.Count == 0)
            {
                generator.SqlBuilder.Append("1=0");
                return;
            }

            int maxInItems = generator.Options.MaxInItems;

            if (elementExps.Count > maxInItems)
                generator.SqlBuilder.Append("(");

            int batches = 0;
            int currentInItems = 0;
            for (int i = 0; i < elementExps.Count; i++)
            {
                if (currentInItems == 0)
                {
                    if (batches > 0)
                    {
                        generator.SqlBuilder.Append(" OR ");
                    }

                    operand.Accept(generator);
                    generator.SqlBuilder.Append(" IN (");
                }

                if (currentInItems > 0)
                    generator.SqlBuilder.Append(",");

                elementExps[i].Accept(generator);

                currentInItems++;
                if (currentInItems == maxInItems || i == (elementExps.Count - 1))
                {
                    generator.SqlBuilder.Append(")");
                    currentInItems = 0;
                    batches++;
                }
            }

            if (elementExps.Count > maxInItems)
                generator.SqlBuilder.Append(")");
        }

        /// <summary>
        /// in 子查询
        /// </summary>
        /// <param name="generator"></param>
        /// <param name="sqlQuery"></param>
        /// <param name="operand"></param>
        protected virtual void In(SqlGeneratorBase generator, DbSqlQueryExpression sqlQuery, DbExpression operand)
        {
            operand.Accept(generator);
            generator.SqlBuilder.Append(" IN (");
            sqlQuery.Accept(generator);
            generator.SqlBuilder.Append(")");
        }
    }
}
