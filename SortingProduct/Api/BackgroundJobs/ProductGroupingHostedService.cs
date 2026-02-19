using SortingProduct.Application.Services;

namespace SortingProduct.Api.BackgroundJobs;

public sealed class ProductGroupingHostedService : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;

    public ProductGroupingHostedService(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        using var timer = new PeriodicTimer(TimeSpan.FromMinutes(5));

        while (await timer.WaitForNextTickAsync(stoppingToken))
        {
            using var scope = _serviceProvider.CreateScope();
            var groupingService = scope.ServiceProvider.GetRequiredService<ProductGroupingService>();
            await groupingService.CreateGroupsAsync(stoppingToken);
        }
    }
}
