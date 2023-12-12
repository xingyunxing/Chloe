using Chloe.Data;
using Chloe.DbExpressions;
using Chloe.Descriptors;
using Chloe.Exceptions;
using Chloe.Extensions;
using Chloe.Infrastructure;
using Chloe.RDBMS;
using Chloe.Reflection;
using Chloe.Utility;
using System.Collections;
using System.Collections.ObjectModel;
using System.Data;
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
            NumericTypes = new HashSet<Type>();
            NumericTypes.Add(typeof(byte));
            NumericTypes.Add(typeof(sbyte));
            NumericTypes.Add(typeof(short));
            NumericTypes.Add(typeof(ushort));
            NumericTypes.Add(typeof(int));
            NumericTypes.Add(typeof(uint));
            NumericTypes.Add(typeof(long));
            NumericTypes.Add(typeof(ulong));
            NumericTypes.Add(typeof(float));
            NumericTypes.Add(typeof(double));
            NumericTypes.Add(typeof(decimal));
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

        public static (DbExpression Left, DbExpression Right) AmendExpDbInfo(DbExpression left, DbExpression right)
        {
            left = DbExpressionExtension.StripInvalidConvert(left);
            right = DbExpressionExtension.StripInvalidConvert(right);
            PublicHelper.AmendDbInfo(left, right);

            return (left, right);
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

        public static Stack<DbExpression> GatherBinaryExpressionOperand(DbBinaryExpression exp)
        {
            DbExpressionType nodeType = exp.NodeType;

            Stack<DbExpression> items = new Stack<DbExpression>();
            items.Push(exp.Right);

            DbExpression left = exp.Left;
            while (left.NodeType == nodeType)
            {
                exp = (DbBinaryExpression)left;
                items.Push(exp.Right);
                left = exp.Left;
            }

            items.Push(left);
            return items;
        }
        public static DbCaseWhenExpression ConstructReturnCSharpBooleanCaseWhenExpression(DbExpression exp)
        {
            // case when 1>0 then 1 when not (1>0) then 0 else Null end
            DbCaseWhenExpression.WhenThenExpressionPair whenThenPair = new DbCaseWhenExpression.WhenThenExpressionPair(exp, DbConstantExpression.True);
            DbCaseWhenExpression.WhenThenExpressionPair whenThenPair1 = new DbCaseWhenExpression.WhenThenExpressionPair(DbExpression.Not(exp), DbConstantExpression.False);
            List<DbCaseWhenExpression.WhenThenExpressionPair> whenThenExps = new List<DbCaseWhenExpression.WhenThenExpressionPair>(2);
            whenThenExps.Add(whenThenPair);
            whenThenExps.Add(whenThenPair1);
            DbCaseWhenExpression caseWhenExpression = DbExpression.CaseWhen(whenThenExps, DbConstantExpression.Null, PublicConstants.TypeOfBoolean);

            return caseWhenExpression;
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
        public static void EnsurePrimaryKeyNotNull(PrimitivePropertyDescriptor propertyDescriptor, object val)
        {
            if (propertyDescriptor.IsPrimaryKey && val == null)
            {
                throw new ChloeException(string.Format("The primary key '{0}' could not be null.", propertyDescriptor.Property.Name));
            }
        }

        public static bool CanIgnoreInsert(InsertStrategy insertStrategy, object value)
        {
            bool ignoreNullValueInsert = (insertStrategy & InsertStrategy.IgnoreNull) == InsertStrategy.IgnoreNull;
            if (ignoreNullValueInsert && value == null)
            {
                return true;
            }

            bool ignoreEmptyStringValueInsert = (insertStrategy & InsertStrategy.IgnoreEmptyString) == InsertStrategy.IgnoreEmptyString;
            if (ignoreEmptyStringValueInsert && string.Empty.Equals(value))
            {
                return true;
            }

            return false;
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
        public static string AppendNotSupportedCastErrorMsg(Type sourceType, Type targetType)
        {
            return string.Format("Does not support the type '{0}' converted to type '{1}'.", sourceType.FullName, targetType.FullName);
        }

        /// <summary>
        /// 解析字段
        /// </summary>
        /// <param name="fieldsLambdaExpression">a => a.Name || a => new { a.Name, a.Age } || a => new object[] { a.Name, a.Age }</param>
        /// <returns></returns>
        public static List<MemberInfo> ResolveFields(LambdaExpression fieldsLambdaExpression)
        {
            ParameterExpression parameterExpression = fieldsLambdaExpression.Parameters[0];

            var body = ExpressionExtension.StripConvert(fieldsLambdaExpression.Body);

            if (body.NodeType == ExpressionType.MemberAccess)
            {
                //a => a.Name
                return new List<MemberInfo>(1) { PickMember(body, parameterExpression) };
            }

            ReadOnlyCollection<Expression> fieldExps = null;
            if (body.NodeType == ExpressionType.New)
            {
                NewExpression newExpression = (NewExpression)body;
                if (newExpression.Type.IsAnonymousType())
                {
                    //a => new { a.Name, a.Age }
                    fieldExps = newExpression.Arguments;
                }
                else
                {
                    throw new NotSupportedException(fieldsLambdaExpression.ToString());
                }
            }
            else if (body.NodeType == ExpressionType.NewArrayInit)
            {
                //a => new object[] { a.Name, a.Age }
                NewArrayExpression newArrayExpression = body as NewArrayExpression;
                if (newArrayExpression == null)
                    throw new NotSupportedException(fieldsLambdaExpression.ToString());

                fieldExps = newArrayExpression.Expressions;
            }
            else
            {
                throw new NotSupportedException(fieldsLambdaExpression.ToString());
            }

            List<MemberInfo> fields = new List<MemberInfo>(fieldExps.Count);

            foreach (var fieldExp in fieldExps)
            {
                fields.Add(PickMember(fieldExp, parameterExpression));
            }

            return fields;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="exp">a.Name</param>
        /// <param name="parameterExpression"></param>
        /// <returns></returns>
        /// <exception cref="NotSupportedException"></exception>
        static MemberInfo PickMember(Expression exp, ParameterExpression parameterExpression)
        {
            MemberExpression memberExp = ExpressionExtension.StripConvert(exp) as MemberExpression;
            if (memberExp == null)
                throw new NotSupportedException(exp.ToString());

            if (memberExp.Expression != parameterExpression)
                throw new NotSupportedException(exp.ToString());

            return memberExp.Member;
        }

        public static Action<TEntity, IDataReader> GetMapper<TEntity>(PrimitivePropertyDescriptor propertyDescriptor, int ordinal)
        {
            var dbValueReader = DataReaderConstant.GetDbValueReader(propertyDescriptor.PropertyType);

            Action<TEntity, IDataReader> mapper = (TEntity entity, IDataReader reader) =>
            {
                object value = dbValueReader.GetValue(reader, ordinal);
                if (value == null || value == DBNull.Value)
                    throw new ChloeException($"Unable to get the {propertyDescriptor.Property.Name} value from data reader.");

                propertyDescriptor.SetValue(entity, value);
            };

            return mapper;
        }

        public static Dictionary<string, IPropertyHandler[]> FindPropertyHandlers(Assembly assembly)
        {
            var propertyHandlerMap = new Dictionary<string, List<IPropertyHandler>>();

            var propertyHandlerTypes = assembly.GetTypes().Where(a => a.IsClass && !a.IsAbstract && typeof(IPropertyHandler).IsAssignableFrom(a) && a.Name.EndsWith("_Handler") && a.GetConstructor(Type.EmptyTypes) != null);

            foreach (Type propertyHandlerType in propertyHandlerTypes)
            {
                string handlePropertyName = propertyHandlerType.Name.Substring(0, propertyHandlerType.Name.Length - "_Handler".Length);

                List<IPropertyHandler> propertyHandlers;
                if (!propertyHandlerMap.TryGetValue(handlePropertyName, out propertyHandlers))
                {
                    propertyHandlers = new List<IPropertyHandler>();
                    propertyHandlerMap.Add(handlePropertyName, propertyHandlers);
                }

                propertyHandlers.Add((IPropertyHandler)Activator.CreateInstance(propertyHandlerType));
            }

            Dictionary<string, IPropertyHandler[]> ret = new Dictionary<string, IPropertyHandler[]>(propertyHandlerMap.Count);
            foreach (var kv in propertyHandlerMap)
            {
                ret.Add(kv.Key, kv.Value.ToArray());
            }

            return ret;
        }

        public static Dictionary<string, IMethodHandler[]> FindMethodHandlers(Assembly assembly)
        {
            var methodHandlerMap = new Dictionary<string, List<IMethodHandler>>();

            var methodHandlerTypes = assembly.GetTypes().Where(a => a.IsClass && !a.IsAbstract && typeof(IMethodHandler).IsAssignableFrom(a) && a.Name.EndsWith("_Handler") && a.GetConstructor(Type.EmptyTypes) != null);

            foreach (Type methodHandlerType in methodHandlerTypes)
            {
                string handleMethodName = methodHandlerType.Name.Substring(0, methodHandlerType.Name.Length - "_Handler".Length);

                List<IMethodHandler> methodHandlers;
                if (!methodHandlerMap.TryGetValue(handleMethodName, out methodHandlers))
                {
                    methodHandlers = new List<IMethodHandler>();
                    methodHandlerMap.Add(handleMethodName, methodHandlers);
                }

                methodHandlers.Add((IMethodHandler)Activator.CreateInstance(methodHandlerType));
            }

            Dictionary<string, IMethodHandler[]> ret = new Dictionary<string, IMethodHandler[]>(methodHandlerMap.Count);
            foreach (var kv in methodHandlerMap)
            {
                ret.Add(kv.Key, kv.Value.ToArray());
            }

            return ret;
        }
    }
}
