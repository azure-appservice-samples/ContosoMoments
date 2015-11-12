using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Azure;
using Microsoft.ServiceBus.Messaging;
using Microsoft.WindowsAzure.Storage.Queue.Protocol;

namespace ContosoMoments.Common.Queue
{
    public class QueueManager
    {
        public void PushToQueue(BlobInformation blobInformation)
        {
            string connectionString = CloudConfigurationManager.GetSetting("Microsoft.ServiceBus.ConnectionString");
            QueueClient Client = QueueClient.CreateFromConnectionString(connectionString, AppSettings.ResizeQueueName);
            Client.Send(new BrokeredMessage(blobInformation));
        }
    }
}
