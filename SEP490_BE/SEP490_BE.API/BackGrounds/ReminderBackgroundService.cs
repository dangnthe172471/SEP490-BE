using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.DependencyInjection;
using SEP490_BE.BLL.IServices.IManagerService;

namespace SEP490_BE.API.BackGrounds
{
    public class ReminderBackgroundService : BackgroundService
    {
        private readonly IServiceScopeFactory _scopeFactory;

        public ReminderBackgroundService(IServiceScopeFactory scopeFactory)
        {
            _scopeFactory = scopeFactory;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            var nextRun = GetNextRunTime();

            while (!stoppingToken.IsCancellationRequested)
            {
                var now = DateTime.Now;
                var delay = nextRun - now;

                if (delay > TimeSpan.Zero)
                {
                    await Task.Delay(delay, stoppingToken);
                }

                // Gửi thông báo
                using (var scope = _scopeFactory.CreateScope())
                {
                    var notificationService = scope.ServiceProvider.GetRequiredService<INotificationService>();
                    await notificationService.SendAppointmentReminderAsync();
                }

               
                nextRun = nextRun.Date.AddDays(1).AddHours(10);
            }
        }

        private DateTime GetNextRunTime()
        {
            var now = DateTime.Now;
            var todayAt10 = now.Date.AddHours(10); 

            if (now >= todayAt10)
            {
                
                return now;
            }

            // Nếu đã qua 10h rồi thì chuyển sang 10h ngày mai
            return todayAt10.AddDays(1);
        }
    }
}
