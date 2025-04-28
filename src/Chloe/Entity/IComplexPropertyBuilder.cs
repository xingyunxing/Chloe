using System.Linq.Expressions;

namespace Chloe.Entity
{
    public interface IComplexPropertyBuilder
    {
        IEntityTypeBuilder DeclaringBuilder { get; }
        ComplexProperty Property { get; }

        /// <summary>
        /// 设置外键
        /// </summary>
        /// <param name="foreignKey"></param>
        /// <returns></returns>
        IComplexPropertyBuilder WithForeignKey(string foreignKey);

        /// <summary>
        /// 设置关联到对端的属性。不指定则默认关联对端的主键
        /// </summary>
        /// <param name="otherSideKey"></param>
        /// <returns></returns>
        IComplexPropertyBuilder AssociateWithOtherSide(string otherSideKey);
    }

    public interface IComplexPropertyBuilder<TProperty, TEntity> : IComplexPropertyBuilder
    {
        new IEntityTypeBuilder<TEntity> DeclaringBuilder { get; }

        /// <summary>
        /// 设置外键
        /// </summary>
        /// <param name="foreignKey"></param>
        /// <returns></returns>
        new IComplexPropertyBuilder<TProperty, TEntity> WithForeignKey(string foreignKey);

        /// <summary>
        /// 设置关联到对端的属性
        /// </summary>
        /// <typeparam name="TForeignKey"></typeparam>
        /// <param name="foreignKey"></param>
        /// <returns></returns>
        IComplexPropertyBuilder<TProperty, TEntity> WithForeignKey<TForeignKey>(Expression<Func<TEntity, TForeignKey>> foreignKey);

        /// <summary>
        /// 设置关联到对端的属性。不指定则默认关联对端的主键
        /// </summary>
        /// <param name="otherSideKey">对端的属性</param>
        /// <returns></returns>
        new IComplexPropertyBuilder<TProperty, TEntity> AssociateWithOtherSide(string otherSideKey);

        /// <summary>
        /// 设置关联到对端的属性。不指定则默认关联对端的主键
        /// </summary>
        /// <typeparam name="TOtherSideKey"></typeparam>
        /// <param name="otherSideKey">对端的属性</param>
        /// <returns></returns>
        IComplexPropertyBuilder<TProperty, TEntity> AssociateWithOtherSide<TOtherSideKey>(Expression<Func<TProperty, TOtherSideKey>> otherSideKey);
    }
}
