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

        public async Task PushToResizeQueue(BlobInformation blobInformation)
        {
            try {
                CloudStorageAccount account;

                if (CloudStorageAccount.TryParse(StorageConnectionString(), out account)) {
                    CloudQueueClient queueClient = account.CreateCloudQueueClient();
                    CloudQueue resizeRequestQueue = queueClient.GetQueueReference(AppSettings.ResizeQueueName);
                    resizeRequestQueue.CreateIfNotExists();

                    var queueMessage = new CloudQueueMessage(JsonConvert.SerializeObject(blobInformation));
                    await resizeRequestQueue.AddMessageAsync(queueMessage);
                }
            }
            catch (Exception ex) {
                Trace.TraceError("Exception in QueueManager.PushToQueue => " + ex.Message);
            }

        }

        public async Task PushToDeleteQueue(BlobInformation blobInformation)
        {
            try {
                CloudStorageAccount account;

                if (CloudStorageAccount.TryParse(StorageConnectionString(), out account)) {
                    CloudQueueClient queueClient = account.CreateCloudQueueClient();
                    CloudQueue deleteRequestQueue = queueClient.GetQueueReference(AppSettings.DeleteQueueName);
                    deleteRequestQueue.CreateIfNotExists();

                    var queueMessage = new CloudQueueMessage(JsonConvert.SerializeObject(blobInformation));
                    await deleteRequestQueue.AddMessageAsync(queueMessage);
                }
            }
            catch (Exception ex) {
                Trace.TraceError("Exception in QueueManager.PushToQueue => " + ex.Message);
            }
        }
    }
}
