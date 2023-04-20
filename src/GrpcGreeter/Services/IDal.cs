namespace GrpcGreeter.Services
{
    public interface IDal
    {
        string Name { get; }
    }
    public class Dal : IDal
    {
        public Dal()
        {
            this.Name = Guid.NewGuid().ToString();
        }
        public string Name { get; set; }
    }

    public interface IFactory<T>
    {
        T Create(IServiceProvider serviceProvider);
    }

    public class DefaultFactory<T> : IFactory<T>
    {
        public T Create(IServiceProvider serviceProvider)
        {
            var service = serviceProvider.GetService<T>();

            return service;
        }
    }
}
