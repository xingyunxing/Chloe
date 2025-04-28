using System.Linq.Expressions;

namespace Chloe.Entity
{
    public interface IEntityTypeBuilder
    {
        EntityType EntityType { get; }
        IEntityTypeBuilder MapTo(string table);
        IEntityTypeBuilder HasSchema(string schema);
        IEntityTypeBuilder HasAnnotation(object value);
        IEntityTypeBuilder Ignore(string property);
        IEntityTypeBuilder HasQueryFilter(LambdaExpression filter);
        IPrimitivePropertyBuilder Property(string property);
        IComplexPropertyBuilder HasOne(string property);
        IComplexPropertyBuilder HasOne(string property, string foreignKey);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="property"></param>
        /// <param name="foreignKey">外键</param>
        /// <param name="otherSideKey">关联到对端的属性。不指定则默认关联对端的主键</param>
        /// <returns></returns>
        IComplexPropertyBuilder HasOne(string property, string foreignKey, string otherSideKey);

        ICollectionPropertyBuilder HasMany(string property);
    }

    public interface IEntityTypeBuilder<TEntity> : IEntityTypeBuilder
    {
        new IEntityTypeBuilder<TEntity> MapTo(string table);
        new IEntityTypeBuilder<TEntity> HasSchema(string schema);
        new IEntityTypeBuilder<TEntity> HasAnnotation(object value);
        IEntityTypeBuilder<TEntity> Ignore(Expression<Func<TEntity, object>> property);
        IEntityTypeBuilder<TEntity> HasQueryFilter(Expression<Func<TEntity, bool>> filter);
        IPrimitivePropertyBuilder<TProperty, TEntity> Property<TProperty>(Expression<Func<TEntity, TProperty>> property);
        IComplexPropertyBuilder<TProperty, TEntity> HasOne<TProperty>(Expression<Func<TEntity, TProperty>> property);

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="TProperty"></typeparam>
        /// <typeparam name="TForeignKey">外键</typeparam>
        /// <param name="property"></param>
        /// <param name="foreignKey">外键</param>
        /// <returns></returns>
        IComplexPropertyBuilder<TProperty, TEntity> HasOne<TProperty, TForeignKey>(Expression<Func<TEntity, TProperty>> property, Expression<Func<TEntity, TForeignKey>> foreignKey);

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="TProperty"></typeparam>
        /// <typeparam name="TForeignKey">外键</typeparam>
        /// <typeparam name="TOtherSideKey">对端的属性</typeparam>
        /// <param name="property"></param>
        /// <param name="foreignKey">外键</param>
        /// <param name="otherSideKey">关联到对端的属性。不指定则默认关联对端的主键</param>
        /// <returns></returns>
        IComplexPropertyBuilder<TProperty, TEntity> HasOne<TProperty, TForeignKey, TOtherSideKey>(Expression<Func<TEntity, TProperty>> property, Expression<Func<TEntity, TForeignKey>> foreignKey, Expression<Func<TProperty, TOtherSideKey>> otherSideKey);

        ICollectionPropertyBuilder<TProperty, TEntity> HasMany<TProperty>(Expression<Func<TEntity, TProperty>> property);
    }
}
