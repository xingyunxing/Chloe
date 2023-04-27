using Chloe.DbExpressions;
using Chloe.Descriptors;
using Chloe.Exceptions;
using Chloe.Infrastructure;
using Chloe.InternalExtensions;
using Chloe.Reflection;
using Chloe.Utility;
using System.Collections;
using System.Linq.Expressions;
using System.Reflection;

namespace Chloe
{
    public class PublicHelper
    {
        static readonly HashSet<Type> NumericTypes;
        static readonly HashSet<Type> ToStringableNumericTypes;

        static PublicHelper()
        {
            HashSet<Type> numericTypes = new HashSet<Type>();
            numericTypes.Add(typeof(byte));
            numericTypes.Add(typeof(sbyte));
            numericTypes.Add(typeof(short));
            numericTypes.Add(typeof(ushort));
            numericTypes.Add(typeof(int));
            numericTypes.Add(typeof(uint));
            numericTypes.Add(typeof(long));
            numericTypes.Add(typeof(ulong));
            numericTypes.Add(typeof(float));
            numericTypes.Add(typeof(double));
            numericTypes.Add(typeof(decimal));
            NumericTypes.TrimExcess();


            ToStringableNumericTypes = new HashSet<Type>();
            ToStringableNumericTypes.Add(typeof(byte));
            ToStringableNumericTypes.Add(typeof(sbyte));
            ToStringableNumericTypes.Add(typeof(short));
            ToStringableNumericTypes.Add(typeof(ushort));
            ToStringableNumericTypes.Add(typeof(int));
            ToStringableNumericTypes.Add(typeof(uint));
            ToStringableNumericTypes.Add(typeof(long));
            ToStringableNumericTypes.Add(typeof(ulong));
            ToStringableNumericTypes.TrimExcess();
        }

        public static bool IsNumericType(Type type)
        {
            return PublicHelper.NumericTypes.Contains(type);
        }

        public static bool IsToStringableNumericType(Type type)
        {
            type = ReflectionExtension.GetUnderlyingType(type);
            return ToStringableNumericTypes.Contains(type);
        }

        /// <summary>
        /// 修正使用关系运算符时的 DbType，避免出现双边类型不一致时导致索引失效
        /// </summary>
        /// <param name="exp1"></param>
        /// <param name="exp2"></param>
        public static void AmendDbInfo(DbExpression exp1, DbExpression exp2)
        {
            DbColumnAccessExpression datumPointExp = null;
            DbParameterExpression expToAmend = null;

            DbExpression e = Trim_Nullable_Value(exp1);
            if (e.NodeType == DbExpressionType.ColumnAccess && exp2.NodeType == DbExpressionType.Parameter)
            {
                datumPointExp = (DbColumnAccessExpression)e;
                expToAmend = (DbParameterExpression)exp2;
            }
            else if ((e = Trim_Nullable_Value(exp2)).NodeType == DbExpressionType.ColumnAccess && exp1.NodeType == DbExpressionType.Parameter)
            {
                datumPointExp = (DbColumnAccessExpression)e;
                expToAmend = (DbParameterExpression)exp1;
            }
            else
                return;

            if (datumPointExp.Column.DbType != null)
            {
                if (expToAmend.DbType == null)
                    expToAmend.DbType = datumPointExp.Column.DbType;
            }
        }
        public static void AmendDbInfo(DbColumn column, DbExpression exp)
        {
            if (column.DbType == null || exp.NodeType != DbExpressionType.Parameter)
                return;

            DbParameterExpression expToAmend = (DbParameterExpression)exp;

            if (expToAmend.DbType == null)
                expToAmend.DbType = column.DbType;
        }
        static DbExpression Trim_Nullable_Value(DbExpression exp)
        {
            DbMemberExpression memberExp = exp as DbMemberExpression;
            if (memberExp == null)
                return exp;

            if (memberExp.Member.Name == "Value" && ReflectionExtension.IsNullable(memberExp.Expression.Type))
                return memberExp.Expression;

            return exp;
        }

        public static void CheckNull(object obj, string paramName = null)
        {
            if (obj == null)
                throw new ArgumentNullException(paramName);
        }
        public static bool AreEqual(object obj1, object obj2)
        {
            if (obj1 == null && obj2 == null)
                return true;

            if (obj1 != null)
            {
                return obj1.Equals(obj2);
            }

            if (obj2 != null)
            {
                return obj2.Equals(obj1);
            }

            return object.Equals(obj1, obj2);
        }
        public static DbMethodCallExpression MakeNextValueForSequenceDbExpression(PrimitivePropertyDescriptor propertyDescriptor, string defaultSequenceSchema)
        {
            string sequenceSchema = propertyDescriptor.Definition.SequenceSchema;
            sequenceSchema = string.IsNullOrEmpty(sequenceSchema) ? defaultSequenceSchema : sequenceSchema;
            return MakeNextValueForSequenceDbExpression(propertyDescriptor.PropertyType, propertyDescriptor.Definition.SequenceName, sequenceSchema);
        }
        public static DbMethodCallExpression MakeNextValueForSequenceDbExpression(Type retType, string sequenceName, string sequenceSchema)
        {
            MethodInfo nextValueForSequenceMethod = PublicConstants.MethodInfo_Sql_NextValueForSequence.MakeGenericMethod(retType);
            List<DbExpression> arguments = new List<DbExpression>(2) { new DbConstantExpression(sequenceName), new DbConstantExpression(sequenceSchema) };

            DbMethodCallExpression getNextValueForSequenceExp = new DbMethodCallExpression(null, nextValueForSequenceMethod, arguments);
            return getNextValueForSequenceExp;
        }
        public static object ConvertObjectType(object obj, Type conversionType)
        {
            if (obj == null)
                return null;

            Type objType = obj.GetType();

            if (objType == conversionType)
                return obj;

            conversionType = conversionType.GetUnderlyingType();
            if (objType != conversionType)
                return Convert.ChangeType(obj, conversionType);

            return obj;
        }

        public static List<T> Clone<T>(List<T> source, int? capacity = null)
        {
            return source.Clone(capacity);
        }
        public static List<T> CloneAndAppendOne<T>(List<T> source, T t)
        {
            return source.CloneAndAppendOne(t);
        }
        public static Dictionary<TKey, TValue> Clone<TKey, TValue>(Dictionary<TKey, TValue> source)
        {
            return source.Clone();
        }

        public static void EnsureHasPrimaryKey(TypeDescriptor typeDescriptor)
        {
            if (!typeDescriptor.HasPrimaryKey())
                throw new ChloeException(string.Format("The entity type '{0}' does not define any primary key.", typeDescriptor.Definition.Type.FullName));
        }

        public static DbParam[] BuildParams(DbContextProvider dbContextProvider, object parameter)
        {
            if (parameter == null)
                return new DbParam[0];

            if (parameter is IEnumerable<DbParam>)
            {
                return ((IEnumerable<DbParam>)parameter).ToArray();
            }

            List<DbParam> parameters = new List<DbParam>();
            Type parameterType = parameter.GetType();
            var props = parameterType.GetProperties();
            foreach (var prop in props)
            {
                if (prop.GetGetMethod() == null || !MappingTypeSystem.IsMappingType(prop.GetMemberType()))
                {
                    continue;
                }

                object value = ReflectionExtension.FastGetMemberValue(prop, parameter);

                string paramName = dbContextProvider.DatabaseProvider.CreateParameterName(prop.Name);

                DbParam p = new DbParam(paramName, value, prop.PropertyType);
                parameters.Add(p);
            }

            return parameters.ToArray();
        }

        public static void NotNullCheck(PrimitivePropertyDescriptor propertyDescriptor, object val)
        {
            if (!propertyDescriptor.IsNullable && val == null)
            {
                throw new ChloeException($"The property '{propertyDescriptor.Property.Name}' can not be null.");
            }
        }

        public static object IncreaseRowVersionNumber(object val)
        {
            if (val.GetType() == PublicConstants.TypeOfInt32)
            {
                return (int)val + 1;
            }

            return (long)val + 1;
        }

        public static DbExpression MakeCondition(PairList<PrimitivePropertyDescriptor, object> propertyValuePairs, DbTable dbTable)
        {
            DbExpression conditionExp = null;
            foreach (var pair in propertyValuePairs)
            {
                PrimitivePropertyDescriptor propertyDescriptor = pair.Item1;
                object val = pair.Item2;

                DbExpression left = new DbColumnAccessExpression(dbTable, propertyDescriptor.Column);
                DbExpression right = DbExpression.Parameter(val, propertyDescriptor.PropertyType, propertyDescriptor.Column.DbType);
                DbExpression equalExp = new DbEqualExpression(left, right);
                conditionExp = conditionExp.And(equalExp);
            }

            return conditionExp;
        }

        public static void CauseErrorIfOptimisticUpdateFailed(int rowsAffected)
        {
            if (rowsAffected <= 0)
                throw new OptimisticConcurrencyException();
        }

        public static DbTable CreateDbTable(TypeDescriptor typeDescriptor, string table)
        {
            return typeDescriptor.GenDbTable(table);
        }

        public static bool Is_Sql_IsEqual_Method(MethodInfo method)
        {
            if (method.DeclaringType == PublicConstants.TypeOfSql && method.Name == nameof(Sql.IsEqual))
            {
                return true;
            }

            return false;
        }
        public static bool Is_Sql_IsNotEqual_Method(MethodInfo method)
        {
            if (method.DeclaringType == PublicConstants.TypeOfSql && method.Name == nameof(Sql.IsNotEqual))
            {
                return true;
            }

            return false;
        }

        public static bool Is_Contains_MethodCall(MethodCallExpression exp)
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
        public static bool Is_List_Contains_MethodCall(MethodCallExpression exp)
        {
            MethodInfo method = exp.Method;

            Type declaringType = method.DeclaringType;
            if (typeof(IList).IsAssignableFrom(declaringType) || (declaringType.IsGenericType && typeof(ICollection<>).MakeGenericType(declaringType.GetGenericArguments()).IsAssignableFrom(declaringType)))
            {
                return true;
            }

            return false;
        }
        public static bool Is_Enumerable_Contains_MethodCall(MethodCallExpression exp)
        {
            MethodInfo method = exp.Method;

            Type declaringType = method.DeclaringType;
            if (method.IsStatic && declaringType == typeof(Enumerable) && exp.Arguments.Count == 2)
            {
                return true;
            }

            return false;
        }

        public static bool Is_Sql_IsEqual_MethodCall(MethodCallExpression exp)
        {
            MethodInfo method = exp.Method;
            return Is_Sql_IsEqual_Method(method);
        }
        public static bool Is_Sql_IsNotEqual_MethodCall(MethodCallExpression exp)
        {
            MethodInfo method = exp.Method;
            return Is_Sql_IsNotEqual_Method(method);
        }
        public static bool Is_Sql_Compare_MethodCall(MethodCallExpression exp)
        {
            MethodInfo method = exp.Method;

            if (method.DeclaringType == PublicConstants.TypeOfSql && method.Name == nameof(Sql.Compare))
            {
                return true;
            }

            return false;
        }
        public static bool Is_Instance_Equals_MethodCall(MethodCallExpression exp)
        {
            MethodInfo method = exp.Method;

            if (method.ReturnType != PublicConstants.TypeOfBoolean || method.IsStatic || method.GetParameters().Length != 1)
                return false;

            return true;
        }

        public static bool Is_In_Extension_MethodCall(MethodCallExpression exp)
        {
            MethodInfo method = exp.Method;
            /* public static bool In<T>(this T obj, IEnumerable<T> source) */
            if (method.IsGenericMethod && method.ReturnType == PublicConstants.TypeOfBoolean)
            {
                Type[] genericArguments = method.GetGenericArguments();
                ParameterInfo[] parameters = method.GetParameters();
                Type genericType = genericArguments[0];
                if (genericArguments.Length == 1 && parameters.Length == 2 && parameters[0].ParameterType == genericType)
                {
                    Type secondParameterType = parameters[1].ParameterType;
                    if (typeof(IEnumerable<>).MakeGenericType(genericType).IsAssignableFrom(secondParameterType))
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        public static NotSupportedException MakeNotSupportedMethodException(MethodInfo method)
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

            return new NotSupportedException(string.Format("Does not support method '{0}.{1}({2})'.", method.DeclaringType.Name, method.Name, sb.ToString()));
        }

        public static void EnsureTrimCharArgumentIsSpaces(DbExpression exp)
        {
            if (!exp.IsEvaluable())
                throw new NotSupportedException();

            var arg = exp.Evaluate();
            if (arg == null)
                throw new ArgumentNullException();

            var chars = arg as char[];
            if (chars.Length != 1 || chars[0] != ' ')
            {
                throw new NotSupportedException();
            }
        }
    }
}
