using System;
using System.Collections;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.ServiceBus;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.Extensions.Logging;

namespace DeadLetterRepro
{
    public static class Function1
    {
        [FunctionName("HttpTrigger")]
        public static async Task<IActionResult> RunHttp(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = null)]
            HttpRequest req, ILogger log, CancellationToken token)
        {
            token.Register(() => log.LogWarning("Cancellation requested for http 1"));
            log.LogInformation("C# HTTP trigger function processed a request.");
            await Task.Delay(100);
            return new OkObjectResult("Success");
        }

        [FunctionName("QueueMessages")]
        public static async Task<IActionResult> RunQueue(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = null)]
            HttpRequest req, ILogger log, CancellationToken token)
        {
            token.Register(() => log.LogWarning("Cancellation requested for http 2"));
            log.LogInformation("C# HTTP trigger function processed a request.");
            
            var queueClient = new QueueClient(GetSetting("ServiceBusConnection"), "myqueue");
            for (int i = 0; i < 50; ++i)
            {
                await QueueMessage(queueClient);
            }
            return new OkObjectResult("Success");
        }

        [FunctionName("ServiceBusTrigger")]
        public static async Task RunSb([ServiceBusTrigger("myqueue", Connection = "ServiceBusConnection")]string myQueueItem, ILogger log, CancellationToken token)
        {
            token.Register(() => log.LogWarning("Cancellation requested for service bus"));
            log.LogInformation($"Starting to process {myQueueItem}");
            await Task.Delay(3500);
            log.LogInformation($"Done with step 1 for {myQueueItem}");
            await Task.Delay(3500);
            log.LogInformation($"Done processing {myQueueItem}");

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
