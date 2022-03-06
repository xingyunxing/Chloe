namespace Chloe.Annotations
{
    /// <summary>
    /// 唯一索引
    /// </summary>
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = false)]
    public class UniqueIndexAttribute : Attribute
    {
    }
}
