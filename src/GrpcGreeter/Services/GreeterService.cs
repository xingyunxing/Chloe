using Shared.Contracts;
using ProtoBuf.Grpc;
using GrpcGreeter.Services;
using Grpc.AspNetCore.Server;

public class GreeterService : IGreeterService
{
    static bool _enabled = false;

    IDal? _dal;
    public GreeterService(IServiceProvider serviceProvider, IDal dal, IGrpcServiceActivator<GreeterService> serviceActivator)
    {
        if (_enabled == false)
        {
            _enabled = true;
            var s = serviceProvider.GetService<GreeterService>();
            //var ss = serviceActivator.Create(serviceProvider);
            _enabled = true;
        }

        //  var factory = serviceProvider.GetService<IFactory<Dal>>();
        //this._dal =   factory.Create(serviceProvider);
        this._dal = dal;
    }

    public Task<HelloReply> SayHelloAsync(HelloRequest request, CallContext context = default)
    {
        return Task.FromResult(
                new HelloReply
                {
                    Message = $"Hello {request.Name} {this._dal.Name}"
                });
    }
}
