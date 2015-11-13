using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Azure;
using Microsoft.ServiceBus;
using Microsoft.ServiceBus.Messaging;
using Microsoft.WindowsAzure.Storage.Queue.Protocol;

namespace ContosoMoments.Common.Queue
{
    public class QueueManager
    {
        public void PushToQueue(BlobInformation blobInformation)
        {
            var namespaceManager = NamespaceManager.CreateFromConnectionString(AppSettings.ServiceBusConnectionString);

            if (!namespaceManager.QueueExists(AppSettings.ResizeQueueName))
            {
                namespaceManager.CreateQueue(AppSettings.ResizeQueueName);
            }
            QueueClient Client = QueueClient.CreateFromConnectionString(AppSettings.ServiceBusConnectionString, AppSettings.ResizeQueueName);
            Client.Send(new BrokeredMessage(blobInformation));
        }
    }
}
