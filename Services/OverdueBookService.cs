using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using LibraryManagementSystem.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace LibraryManagementSystem.Services
{
    public class OverdueBookService : BackgroundService
    {
        private readonly IServiceProvider _services;
        private readonly ILogger<OverdueBookService> _logger;
        private readonly TimeSpan _checkInterval = TimeSpan.FromHours(24); // Check once per day

        public OverdueBookService(
            IServiceProvider services,
            ILogger<OverdueBookService> logger)
        {
            _services = services;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Overdue Book Service running.");

            using var timer = new PeriodicTimer(_checkInterval);

            try
            {
                while (await timer.WaitForNextTickAsync(stoppingToken))
                {
                    await CheckOverdueBooks(stoppingToken);
                }
            }
            catch (OperationCanceledException)
            {
                _logger.LogInformation("Overdue Book Service is stopping.");
            }
        }

        private async Task CheckOverdueBooks(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Checking for overdue books");

            using var scope = _services.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

            var now = DateTime.UtcNow;

            // Find all loans that are due today or earlier and not returned yet
            var overdueLoans = await dbContext.BookLoans
                .Where(bl => bl.DueDate <= now && bl.ReturnDate == null && bl.Status != "Overdue")
                .ToListAsync(stoppingToken);

            if (overdueLoans.Any())
            {
                _logger.LogInformation("Found {Count} overdue books", overdueLoans.Count);

                foreach (var loan in overdueLoans)
                {
                    loan.Status = "Overdue";
                    loan.UpdatedAt = now;
                }

                await dbContext.SaveChangesAsync(stoppingToken);
                _logger.LogInformation("Updated {Count} loans to overdue status", overdueLoans.Count);
            }
            else
            {
                _logger.LogInformation("No overdue books found");
            }
        }
    }

    public static class OverdueBookServiceExtensions
    {
        public static IServiceCollection AddOverdueBookService(this IServiceCollection services)
        {
            services.AddHostedService<OverdueBookService>();
            return services;
        }
    }
}