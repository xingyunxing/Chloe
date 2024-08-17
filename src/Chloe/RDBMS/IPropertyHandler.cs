using Chloe.DbExpressions;
using System.Reflection;

namespace Chloe.RDBMS
{
    /// <summary>
    /// 属性解析处理器。
    /// </summary>
    public interface IPropertyHandler
    {
        /// <summary>
        /// 判断是否可以解析传入的属性。
        /// </summary>
        /// <param name="exp"></param>
        /// <returns></returns>
        bool CanProcess(DbMemberAccessExpression exp);

        /// <summary>
        /// 解析传入的属性。
        /// </summary>
        /// <param name="exp"></param>
        /// <param name="generator"></param>
        void Process(DbMemberAccessExpression exp, SqlGeneratorBase generator);
    }

    public class PropertyHandlerBase : IPropertyHandler
    {
        public virtual MemberInfo GetCanProcessProperty()
        {
            throw new NotImplementedException();
        }

        public virtual bool CanProcess(DbMemberAccessExpression exp)
        {
            MemberInfo canProcessProperty = this.GetCanProcessProperty();
            if (canProcessProperty == exp.Member)
                return true;

            return false;
        }

        public virtual void Process(DbMemberAccessExpression exp, SqlGeneratorBase generator)
        {
            throw new NotSupportedException($"Does not support property '{exp.Member.DeclaringType.FullName}.{exp.Member.Name}'.");
        }
    }
}
