using Domain.Interfaces.Service;
using Hangfire;
using Microsoft.Extensions.Localization;
using Service;

namespace API.Jobs
{
    public class AutoCreateVoucher : IHostedService
    {
        private readonly IBackgroundJobClient _backgroundJobs;
        private readonly IServiceProvider _serviceProvider;

        public AutoCreateVoucher(
            IBackgroundJobClient backgroundJob,
            IServiceProvider serviceProvider)
        {
            _backgroundJobs = backgroundJob;
            _serviceProvider = serviceProvider;
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            await CheckAndScheduleVoucherGeneration();
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }

        public async Task ExecuteGenerateVoucher()
        {
            using var scope = _serviceProvider.CreateScope();
            var schreduleService = scope.ServiceProvider.GetRequiredService<IBotSmsService>();
            var _localizer = scope.ServiceProvider.GetRequiredService<IStringLocalizer<BotSmsService>>();
            await schreduleService.GenerateVoucher(_localizer);

            // Schedule the next execution
            var cache = scope.ServiceProvider.GetRequiredService<ICacheService>();
            var jobIdKey = "generateVoucher";
            await cache.Delete(jobIdKey);

            try
            {
                BackgroundJob.Delete(await cache.Get(jobIdKey));
            }
            catch (Exception)
            {
            }
            finally
            {
                await CheckAndScheduleVoucherGeneration();
            }
        }

        public async Task CheckAndScheduleVoucherGeneration()
        {
            using var scope = _serviceProvider.CreateScope();
            var cache = scope.ServiceProvider.GetRequiredService<ICacheService>();

            var jobIdKey = "generateVoucher";
            var now = DateTime.Now;

            // Calculate the next execution date (4 AM tomorrow)
            var nextExecutionDate = now.Date.AddDays(1).AddHours(11);

            // Retrieve the stored job ID from Redis
            var jobId = await cache.Get(jobIdKey);

            // If no job ID is stored or if the job doesn't exist, schedule a new job
            if (string.IsNullOrEmpty(jobId))
            {
                // Schedule the new job
                var random = new Random();
                var minutesToAdd = random.Next(0, 121); // Random minutes between 0 and 120 (2 hours)
                nextExecutionDate = nextExecutionDate.AddMinutes(minutesToAdd);

                var delay = nextExecutionDate - now;

                var newJobId = BackgroundJob.Schedule(() => ExecuteGenerateVoucher(), delay);
                //var delayHrs = (now.Hour - nextExecutionDate.Hour);
                // Store the new job ID in Redis
                await cache.Set(jobIdKey, newJobId, 24);
            }
        }
    }
}
