using Chloe.Reflection;

namespace Chloe.Threading.Tasks
{
    public static class TaskExtension
    {
        public static object GetCompletedTaskResult(this Task task)
        {
            var result = task.GetType().GetProperty(nameof(Task<object>.Result)).FastGetMemberValue(task);
            return result;
        }

        public static TResult GetResult<TResult>(this Task<TResult> task)
        {
            if (task.IsCompleted && !task.IsFaulted)
            {
                return task.Result;
            }

            return task.GetAwaiter().GetResult();
        }
        public static void GetResult(this Task task)
        {
            task.GetAwaiter().GetResult();
        }

#if !netfx
        public static TResult GetResult<TResult>(this ValueTask<TResult> task)
        {
            if (task.IsCompleted && !task.IsFaulted)
            {
                return task.Result;
            }

            return task.GetAwaiter().GetResult();
        }

        public static void GetResult(this ValueTask task)
        {
            task.GetAwaiter().GetResult();
        }
#endif
    }
}

namespace System.Threading.Tasks
{
#if netfx
    internal static class TaskExtension
    {
        public static Task<TResult> AsTask<TResult>(this Task<TResult> task)
        {
            return task;
        }
    }
#endif
}
