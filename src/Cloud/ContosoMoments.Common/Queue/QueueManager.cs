using ContosoMoments.Common.Models;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Queue;
using Newtonsoft.Json;
using System;
using System.Diagnostics;
using System.Threading.Tasks;

namespace ContosoMoments.Common
{
    public class QueueManager
    {
        private static string StorageConnectionString()
        {
            return $"DefaultEndpointsProtocol=https;AccountName={AppSettings.StorageAccountName};AccountKey={AppSettings.StorageAccountKey}";
        }

        public async Task PushToResizeQueue(BlobInformation blobInfo)
        {
            await PushToQueue(AppSettings.ResizeQueueName, JsonConvert.SerializeObject(blobInfo));
        }

        public async Task PushToDeleteQueue(BlobInformation blobInfo)
        {
            await PushToQueue(AppSettings.DeleteQueueName, JsonConvert.SerializeObject(blobInfo));
        }

        private async Task PushToQueue(string queueName, string data)
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
