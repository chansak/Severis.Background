using Severis.Shared.Model;
using Microsoft.Extensions.Options;

namespace Severis.FileProcessor.Services
{
    public interface INotificationServices
    {
        Task NotifyMessage(PushNotification notification);
    }
    public class NotificationServices : INotificationServices {
        private readonly ILogger<NotificationServices> logger;
        private readonly IOptions<AppSettings> settings;
        private readonly HttpClient httpClient;

        public NotificationServices(ILogger<NotificationServices> logger, IOptions<AppSettings> settings, HttpClient httpClient)
        {
            this.logger = logger;
            this.settings = settings;
            this.httpClient = httpClient;
        }
        public async Task NotifyMessage(PushNotification notification)
        {
            var uri = this.settings.Value.SignalRNotificationUrl;
            var pairs = new List<KeyValuePair<string, string>>
            {
                new KeyValuePair<string, string>("ProcessType", notification.ProcessType.ToString()),
                new KeyValuePair<string, string>("ProcessStatus", notification.ProcessStatus.ToString()),
                new KeyValuePair<string, string>("TotalRows", notification.TotalRows.ToString()),
                new KeyValuePair<string, string>("CompletedRows", notification.CompletedRows.ToString()),
                new KeyValuePair<string, string>("PercentageOfWork", notification.PercentageOfWork.ToString()),
            };

            var content = new FormUrlEncodedContent(pairs);
            var response = await httpClient.PostAsync(uri,content);
            if (response.IsSuccessStatusCode) {
                //do somethings after the request has been done.
            }
        }
    }
    
}
