using System.Threading.Tasks;

namespace Chloe.Threading.Tasks
{
    public static class TaskExtension
    {
        public static TResult GetResult<TResult>(this Task<TResult> task)
        {
            return task.GetAwaiter().GetResult();
        }
        public static void GetResult(this Task task)
        {
            task.GetAwaiter().GetResult();
        }

#if !netfx
        public static TResult GetResult<TResult>(this ValueTask<TResult> task)
        {
            return task.GetAwaiter().GetResult();
        }
#endif
    }
}
