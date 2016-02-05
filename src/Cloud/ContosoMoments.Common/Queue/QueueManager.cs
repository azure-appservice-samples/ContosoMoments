using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Azure;
using Microsoft.WindowsAzure.Storage.Queue.Protocol;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Queue;
using Newtonsoft.Json;
using System.Diagnostics;

namespace ContosoMoments.Common.Queue
{
    public class QueueManager
    {
        public async Task PushToResizeQueue(BlobInformation blobInformation)
        {
            try
            {
                CloudStorageAccount account;
                string storageConnectionString = string.Format("DefaultEndpointsProtocol=https;AccountName={0};AccountKey={1}", AppSettings.StorageAccountName, AppSettings.StorageAccountKey);

                if (CloudStorageAccount.TryParse(storageConnectionString, out account))
                {
                    CloudQueueClient queueClient = account.CreateCloudQueueClient();
                    CloudQueue resizeRequestQueue = queueClient.GetQueueReference(AppSettings.ResizeQueueName);
                    resizeRequestQueue.CreateIfNotExists(); 

                    var queueMessage = new CloudQueueMessage(JsonConvert.SerializeObject(blobInformation));
                    await resizeRequestQueue.AddMessageAsync(queueMessage);
                }
            }
            catch (Exception ex)
            {
                Trace.TraceError("Exception in QueueManager.PushToQueue => " + ex.Message);
            }

        }

        public async Task PushToDeleteQueue(BlobInformation blobInformation)
        {
            try
            {
                CloudStorageAccount account;
                string storageConnectionString = string.Format("DefaultEndpointsProtocol=https;AccountName={0};AccountKey={1}", AppSettings.StorageAccountName, AppSettings.StorageAccountKey);

                if (CloudStorageAccount.TryParse(storageConnectionString, out account))
                {
                    CloudQueueClient queueClient = account.CreateCloudQueueClient();
                    CloudQueue deleteRequestQueue = queueClient.GetQueueReference(AppSettings.DeleteQueueName);
                    deleteRequestQueue.CreateIfNotExists();

                    var queueMessage = new CloudQueueMessage(JsonConvert.SerializeObject(blobInformation));
                    await deleteRequestQueue.AddMessageAsync(queueMessage);
                }
            }
            catch (Exception ex)
            {
                Trace.TraceError("Exception in QueueManager.PushToQueue => " + ex.Message);
            }

        }
    }
}
