using ContosoMoments.Common.Models;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Queue;
using Newtonsoft.Json;
using System;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Configuration;
using Microsoft.Azure.Mobile.Server.Files;

namespace ContosoMoments.Common
{
    public class QueueManager
    {
        private static string StorageConnectionString()
        {
            return ConfigurationManager.ConnectionStrings[Constants.StorageConnectionStringName].ConnectionString;
        }

        public static async Task PushToDeleteQueue(string blobName)
        {
            await PushToQueue(AppSettings.DeleteQueueName, blobName);
        }

        private static async Task PushToQueue(string queueName, string data)
        {
            try {
                CloudStorageAccount account;

                if (CloudStorageAccount.TryParse(StorageConnectionString(), out account)) {
                    CloudQueueClient queueClient = account.CreateCloudQueueClient();
                    CloudQueue queue = queueClient.GetQueueReference(queueName);
                    queue.CreateIfNotExists();

                    var queueMessage = new CloudQueueMessage(data);
                    await queue.AddMessageAsync(queueMessage);
                }
            }
            catch (Exception ex) {
                Trace.TraceError("Exception in QueueManager.PushToQueue => " + ex.Message);
            }
        }
    }
}
