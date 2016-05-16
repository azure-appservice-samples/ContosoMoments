﻿using ContosoMoments.Common.Models;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Queue;
using Newtonsoft.Json;
using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace ContosoMoments.Common
{
    public class QueueManager
    {
        private static string StorageConnectionString()
        {
            return $"DefaultEndpointsProtocol=https;AccountName={AppSettings.StorageAccountName};AccountKey={AppSettings.StorageAccountKey}";
        }

        public static async Task PushToDeleteQueue(string blobName)
        {
            var payload = new JObject();
            payload["ImageId"] = blobName;
                 
            await PushToQueue(AppSettings.DeleteQueueName, payload.ToString());
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
