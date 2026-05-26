using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using KcfMonitoringSystem.Application.Services;
using KcfMonitoringSystem.Application.Interfaces.Services;
using Microsoft.Extensions.Hosting;

namespace KcfMonitoringSystem.Tests;

public class EndpointTestsBase : WebApplicationFactory<Program>
{
    public Mock<IMachineService> MachineServiceMock { get; } = new();
    public Mock<IUserService> UserServiceMock { get; } = new();
    public Mock<IProductService> ProductServiceMock { get; } = new();
    public Mock<IProductionService> ProductionServiceMock { get; } = new();
    public Mock<IStatusService> StatusServiceMock { get; } = new();

    protected override IHost CreateHost(IHostBuilder builder)
    {
        builder.ConfigureServices(services =>
        {
            // Remove existing registrations if necessary, or just override
            services.AddScoped(_ => MachineServiceMock.Object);
            services.AddScoped(_ => UserServiceMock.Object);
            services.AddScoped(_ => ProductServiceMock.Object);
            services.AddScoped(_ => ProductionServiceMock.Object);
            services.AddScoped(_ => StatusServiceMock.Object);
            
            // We might need to mock or use InMemory DB for AppDbContext if the endpoints hit it directly (they shouldn't if using services)
            // Program.cs has migrations/seeding in it, we might want to bypass that in tests.
        });

        return base.CreateHost(builder);
    }
}
