using System;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Azure.ServiceBus;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.Extensions.Logging;

namespace TimerTrigger
{
    public static class Function2
    {
        [FunctionName("TimerQueue")]
        public static async Task Run([TimerTrigger("*/5 * * * * *")] TimerInfo myTimer, ILogger log)
        {
            // Queue a message every 5 seconds
            var queueClient = new QueueClient(GetSetting("ServiceBusConnection"), "myqueue");
            await QueueMessage(queueClient);
            log.LogInformation($"C# Timer trigger function executed at: {DateTime.Now}");
        }

        private static string GetSetting(string settingName)
        {
            return Environment.GetEnvironmentVariable(settingName, EnvironmentVariableTarget.Process);
        }

        private static async Task QueueMessage(QueueClient queueClient)
        {
            var message = new Message(Encoding.UTF8.GetBytes(Guid.NewGuid().ToString()));
            await queueClient.SendAsync(message);
        }
    }
}
