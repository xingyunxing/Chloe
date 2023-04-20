// See https://aka.ms/new-console-template for more information
using Grpc.Net.Client;
using ProtoBuf.Grpc.Client;
using Shared.Contracts;

namespace GrpcGreeterClient;

internal class Program
{
    private static async Task Main(string[] args)
    {
        while (true)
        {
            Console.ReadKey();

            try
            {
                using var channel = GrpcChannel.ForAddress("http://localhost:5000");
                var client = channel.CreateGrpcService<IGreeterService>();

                var reply = await client.SayHelloAsync(
                    new HelloRequest { Name = "GreeterClient" });

                Console.WriteLine($"Greeting: {reply.Message}");
                Console.WriteLine("Press any key to exit...");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"{ex.Message}");
            }
            //Console.ReadKey();
        }
    }
}


