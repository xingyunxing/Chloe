using Chloe.Core.Visitors;
using Chloe.Descriptors;
using Chloe.Extensions;
using Chloe.Infrastructure;
using Chloe.Reflection;
using Chloe.Utility;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;

namespace Chloe
{
    internal class Helpers
    {
        public static KeyValuePairList<JoinType, Expression> ResolveJoinInfo(LambdaExpression joinInfoExp)
        {
            /*
             * Usage:
             * var view = context.JoinQuery<User, City, Province, User, City>((user, city, province, user1, city1) => new object[] 
             * { 
             *     JoinType.LeftJoin, user.CityId == city.Id, 
             *     JoinType.RightJoin, city.ProvinceId == province.Id,
             *     JoinType.InnerJoin,user.Id==user1.Id,
             *     JoinType.FullJoin,city.Id==city1.Id
             * }).Select((user, city, province, user1, city1) => new { User = user, City = city, Province = province, User1 = user1, City1 = city1 });
             * 
             * To resolve join infomation:
             * JoinType.LeftJoin, user.CityId == city.Id               index of joinType is 0
             * JoinType.RightJoin, city.ProvinceId == province.Id      index of joinType is 2
             * JoinType.InnerJoin,user.Id==user1.Id                    index of joinType is 4
             * JoinType.FullJoin,city.Id==city1.Id                     index of joinType is 6
            */

            NewArrayExpression body = joinInfoExp.Body as NewArrayExpression;

            if (body == null)
            {
                throw new ArgumentException(string.Format("Invalid join infomation '{0}'. The correct usage is like: {1}", joinInfoExp, "context.JoinQuery<User, City>((user, city) => new object[] { JoinType.LeftJoin, user.CityId == city.Id })"));
            }

            KeyValuePairList<JoinType, Expression> ret = new KeyValuePairList<JoinType, Expression>();

            if ((joinInfoExp.Parameters.Count - 1) * 2 != body.Expressions.Count)
            {
                throw new ArgumentException(string.Format("Invalid join infomation '{0}'.", joinInfoExp));
            }

            for (int i = 0; i < joinInfoExp.Parameters.Count - 1; i++)
            {
                /*
                 * 0  0
                 * 1  2
                 * 2  4
                 * 3  6
                 * ...
                 */
                int indexOfJoinType = i * 2;

                Expression joinTypeExpression = body.Expressions[indexOfJoinType];
                object inputJoinType = ExpressionEvaluator.Evaluate(joinTypeExpression);
                if (inputJoinType == null || inputJoinType.GetType() != typeof(JoinType))
                    throw new ArgumentException(string.Format("Not support '{0}', please pass correct type of 'Chloe.JoinType'.", joinTypeExpression));

                /*
                 * The next expression of join type must be join condition.
                 */
                Expression joinCondition = body.Expressions[indexOfJoinType + 1].StripConvert();

                if (joinCondition.Type != PublicConstants.TypeOfBoolean)
                {
                    throw new ArgumentException(string.Format("Not support '{0}', please pass correct join condition.", joinCondition));
                }

                ParameterExpression[] parameters = joinInfoExp.Parameters.Take(i + 2).ToArray();

                List<Type> typeArguments = parameters.Select(a => a.Type).ToList();
                typeArguments.Add(PublicConstants.TypeOfBoolean);

                Type delegateType = Utils.GetFuncDelegateType(typeArguments.ToArray());
                LambdaExpression lambdaOfJoinCondition = Expression.Lambda(delegateType, joinCondition, parameters);

                ret.Add((JoinType)inputJoinType, lambdaOfJoinCondition);
            }

            return ret;
        }

        static MethodInfo _saveMethod;
        static Helpers()
        {
            Expression<Func<Task>> e = () => Save<string>(null, string.Empty, null, false);
            MethodInfo method = (e.Body as MethodCallExpression).Method;
            _saveMethod = method.GetGenericMethodDefinition();
        }

        static MethodInfo GetSaveMethod(Type entityType)
        {
            MethodInfo method = _saveMethod.MakeGenericMethod(entityType);
            return method;
        }

        public static async Task<TEntity> Save<TEntity>(IDbContextProvider dbContextProvider, TEntity entity, bool @async)
        {
            PublicHelper.CheckNull(entity, nameof(entity));

            if (dbContextProvider.Session.IsInTransaction)
            {
                await Save(dbContextProvider, entity, null, @async);
                return entity;
            }

            dbContextProvider.Session.BeginTransaction();
            try
            {
                await Save(dbContextProvider, entity, null, @async);
                dbContextProvider.Session.CommitTransaction();
            }
            catch
            {
                dbContextProvider.Session.RollbackTransaction();
                throw;
            }

            return entity;
        }
        static async Task Save<TEntity>(IDbContextProvider dbContextProvider, TEntity entity, TypeDescriptor declaringTypeDescriptor, bool @async)
        {
            if (@async)
            {
                await dbContextProvider.InsertAsync<TEntity>(entity, null);
            }
            else
            {
                dbContextProvider.Insert(entity, null);
            }

            TypeDescriptor typeDescriptor = EntityTypeContainer.GetDescriptor(typeof(TEntity));

            for (int i = 0; i < typeDescriptor.ComplexPropertyDescriptors.Count; i++)
            {
                //entity.TOther
                ComplexPropertyDescriptor navPropertyDescriptor = typeDescriptor.ComplexPropertyDescriptors[i];

                if (declaringTypeDescriptor != null && navPropertyDescriptor.PropertyType == declaringTypeDescriptor.Definition.Type)
                {
                    continue;
                }

                await SaveOneToOne(dbContextProvider, navPropertyDescriptor, entity, typeDescriptor, @async);
            }

            for (int i = 0; i < typeDescriptor.CollectionPropertyDescriptors.Count; i++)
            {
                //entity.List
                CollectionPropertyDescriptor collectionPropertyDescriptor = typeDescriptor.CollectionPropertyDescriptors[i];
                await SaveCollection(dbContextProvider, collectionPropertyDescriptor, entity, typeDescriptor, @async);
            }
        }
        static async Task SaveOneToOne(IDbContextProvider dbContextProvider, ComplexPropertyDescriptor navPropertyDescriptor, object owner, TypeDescriptor ownerTypeDescriptor, bool @async)
        {
            /*
             * 1:1
             * T    <1:1>    TOther
             * T.TOther <--> TOther.T
             * T.Id <--> TOther.Id
             */

            //owner is T
            //navPropertyDescriptor is T.TOther
            //TypeDescriptor of T.TOther
            TypeDescriptor navTypeDescriptor = EntityTypeContainer.GetDescriptor(navPropertyDescriptor.PropertyType);
            //TOther.T
            ComplexPropertyDescriptor TOtherDotT = navTypeDescriptor.ComplexPropertyDescriptors.Where(a => a.PropertyType == ownerTypeDescriptor.Definition.Type).FirstOrDefault();

            bool isOneToOne = TOtherDotT != null;
            if (!isOneToOne)
                return;

            //instance of T.TOther
            object navValue = navPropertyDescriptor.GetValue(owner);
            if (navValue == null)
                return;

            //T.Id
            PrimitivePropertyDescriptor foreignKeyProperty = navPropertyDescriptor.ForeignKeyProperty;
            if (foreignKeyProperty.IsAutoIncrement || foreignKeyProperty.HasSequence())
            {
                //value of T.Id
                object foreignKeyValue = foreignKeyProperty.GetValue(owner);

                //T.TOther.Id = T.Id
                TOtherDotT.ForeignKeyProperty.SetValue(navValue, foreignKeyValue);
            }

            MethodInfo saveMethod = GetSaveMethod(navPropertyDescriptor.PropertyType);
            //DbContextProvider.Save(navValue, ownerTypeDescriptor, @async);
            Task task = (Task)saveMethod.FastInvoke(null, dbContextProvider, navValue, ownerTypeDescriptor, @async);
            await task;
        }
        static async Task SaveCollection(IDbContextProvider dbContextProvider, CollectionPropertyDescriptor collectionPropertyDescriptor, object owner, TypeDescriptor ownerTypeDescriptor, bool @async)
        {
            PrimitivePropertyDescriptor ownerKeyPropertyDescriptor = ownerTypeDescriptor.PrimaryKeys.FirstOrDefault();
            if (ownerKeyPropertyDescriptor == null)
                return;

            //T.Elements
            IList elementList = collectionPropertyDescriptor.GetValue(owner) as IList;
            if (elementList == null || elementList.Count == 0)
                return;

            TypeDescriptor elementTypeDescriptor = EntityTypeContainer.GetDescriptor(collectionPropertyDescriptor.ElementType);
            //Element.T
            ComplexPropertyDescriptor elementDotT = elementTypeDescriptor.ComplexPropertyDescriptors.Where(a => a.PropertyType == ownerTypeDescriptor.Definition.Type).FirstOrDefault();

            object ownerKeyValue = ownerKeyPropertyDescriptor.GetValue(owner);
            MethodInfo saveMethod = GetSaveMethod(collectionPropertyDescriptor.ElementType);
            for (int i = 0; i < elementList.Count; i++)
            {
                object element = elementList[i];
                if (element == null)
                    continue;

                //element.ForeignKey = T.Id
                elementDotT.ForeignKeyProperty.SetValue(element, ownerKeyValue);
                //DbContext.Save(element, ownerTypeDescriptor, @async);
                Task task = (Task)saveMethod.FastInvoke(null, dbContextProvider, element, ownerTypeDescriptor, @async);
                await task;
            }
        }

        public static async Task<TEntity> QueryByKey<TEntity>(IDbContextFacade dbContext, object key, string table, LockType @lock, bool tracking, bool @async)
        {
            Expression<Func<TEntity, bool>> condition = PrimaryKeyHelper.BuildCondition<TEntity>(key);
            var q = dbContext.Query<TEntity>(table, @lock).Where(condition);

            if (tracking)
                q = q.AsTracking();

            if (@async)
                return await q.FirstOrDefaultAsync();

            return q.FirstOrDefault();
        }
    }
}
