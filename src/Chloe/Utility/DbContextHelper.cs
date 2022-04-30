using Chloe.Descriptors;
using Chloe.Infrastructure;
using Chloe.Reflection;
using Chloe.Utility;
using System.Collections;
using System.Linq.Expressions;
using System.Reflection;

namespace Chloe
{
    internal class DbContextHelper
    {
        static MethodInfo _saveMethod;
        static DbContextHelper()
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

        public static async Task<TEntity> Save<TEntity>(IDbContext dbContext, TEntity entity, bool @async)
        {
            PublicHelper.CheckNull(entity, nameof(entity));

            if (dbContext.Session.IsInTransaction)
            {
                await Save(dbContext, entity, null, @async);
                return entity;
            }

            using (var tran = dbContext.BeginTransaction())
            {
                await Save(dbContext, entity, null, @async);
                tran.Commit();
            }

            return entity;
        }
        static async Task Save<TEntity>(IDbContext dbContext, TEntity entity, TypeDescriptor declaringTypeDescriptor, bool @async)
        {
            if (@async)
            {
                await dbContext.InsertAsync<TEntity>(entity);
            }
            else
            {
                dbContext.Insert(entity);
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

                await SaveOneToOne(dbContext, navPropertyDescriptor, entity, typeDescriptor, @async);
            }

            for (int i = 0; i < typeDescriptor.CollectionPropertyDescriptors.Count; i++)
            {
                //entity.List
                CollectionPropertyDescriptor collectionPropertyDescriptor = typeDescriptor.CollectionPropertyDescriptors[i];
                await SaveCollection(dbContext, collectionPropertyDescriptor, entity, typeDescriptor, @async);
            }
        }
        static async Task SaveOneToOne(IDbContext dbContext, ComplexPropertyDescriptor navPropertyDescriptor, object owner, TypeDescriptor ownerTypeDescriptor, bool @async)
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
            //DbContext.Save(navValue, ownerTypeDescriptor, @async);
            Task task = (Task)saveMethod.FastInvoke(null, dbContext, navValue, ownerTypeDescriptor, @async);
            await task;
        }
        static async Task SaveCollection(IDbContext dbContext, CollectionPropertyDescriptor collectionPropertyDescriptor, object owner, TypeDescriptor ownerTypeDescriptor, bool @async)
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
                Task task = (Task)saveMethod.FastInvoke(null, dbContext, element, ownerTypeDescriptor, @async);
                await task;
            }
        }

        public static async Task<TEntity> QueryByKey<TEntity>(IDbContext dbContext, object key, string table, LockType @lock, bool tracking, bool @async)
        {
            Expression<Func<TEntity, bool>> condition = PrimaryKeyHelper.BuildCondition<TEntity>(key);
            var q = dbContext.Query<TEntity>(table, @lock).Where(condition);

            if (tracking)
                q = q.AsTracking();

            if (@async)
                return await q.FirstOrDefaultAsync();

            return q.FirstOrDefault();
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
