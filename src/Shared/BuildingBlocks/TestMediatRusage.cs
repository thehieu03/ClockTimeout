using BuildingBlocks.Extensions;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;

namespace BuildingBlocks;

public static class TestMediatRusage
{
    public static void TestConfiguration()
    {
        var services=new ServiceCollection();
        // Test with single assembly
        services.AddMediatRWithBehaviors(Assembly.GetExecutingAssembly());
        // Test with multiple assemblies
        // ✅ Test với multiple assemblies
        // services.AddMediatRWithBehaviors(
        //     Assembly.GetExecutingAssembly(),
        //     typeof(SomeOtherClass).Assembly
        // );
        //var serviceProvider = services.BuildServiceProvider();

        //// Verify MediatR is registered
        //var mediator = serviceProvider.GetService<IMediator>();
        //Console.WriteLine($"MediatR registered: {mediator != null}");
    }
}
