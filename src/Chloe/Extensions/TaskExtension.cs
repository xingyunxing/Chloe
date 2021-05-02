using System.Threading.Tasks;

namespace Chloe.Threading.Tasks
{
    public static class TaskExtension
    {
        public static TResult GetResult<TResult>(this Task<TResult> task)
        {
            return task.Result;
        }
        public static void GetResult(this Task task)
        {
            task.Wait();
        }

#if !netfx
        public static TResult GetResult<TResult>(this ValueTask<TResult> task)
        {
            return task.Result;
        }
#endif
    }
}
