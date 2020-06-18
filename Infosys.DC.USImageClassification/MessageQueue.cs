using Microsoft.Azure.ServiceBus;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Queue;
using Newtonsoft.Json;
using SSA.Entity;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SSA.Common
{
    public class MessageQueue
    {
        private IQueueClient queueClient;
        private Collection<DataUploadServiceBusProperties> dataUploadServiceBuses;
        //private Collection<CountryMovementServiceBusProperties> countryMovementServiceBuses;

        public async Task<Collection<DataUploadServiceBusProperties>> GetServiceBusMessages(string serviceBusConnection, string serviceBusQueue)
        {
            dataUploadServiceBuses = new Collection<DataUploadServiceBusProperties>();
            try
            {
                queueClient = new QueueClient(serviceBusConnection, serviceBusQueue);
                RegisterOnMessageHandlerAndReceiveMessages();
                await Task.Delay(TimeSpan.FromSeconds(30));
                await queueClient.CloseAsync();
                return dataUploadServiceBuses;
            }
            catch (Exception ex)
            {
                //LoggingHelper.LogTraceException(ex, MethodBase.GetCurrentMethod().DeclaringType.FullName, MethodBase.GetCurrentMethod().Name);
                throw;
            }
        }
        private void RegisterOnMessageHandlerAndReceiveMessages()
        {
            try
            {
                var messageHandlerOptions = new MessageHandlerOptions(ExceptionReceivedHandler)
                {
                    MaxConcurrentCalls = 1,
                    AutoComplete = false
                };
                queueClient.RegisterMessageHandler(ProcessMessagesAsync, messageHandlerOptions);
            }
            catch (Exception ex)
            {
                //LoggingHelper.LogTraceException(ex, MethodBase.GetCurrentMethod().DeclaringType.FullName, MethodBase.GetCurrentMethod().Name);
                throw;
            }
        }
        private async Task ProcessMessagesAsync(Message message, CancellationToken token)
        {
            try
            {
                string body = Encoding.UTF8.GetString(message.Body);
                DataUploadServiceBusProperties dataUploadService = JsonConvert.DeserializeObject<DataUploadServiceBusProperties>(body);
                dataUploadServiceBuses.Add(dataUploadService);
                await queueClient.CompleteAsync(message.SystemProperties.LockToken);
            }
            catch (Exception ex)
            {
                //LoggingHelper.LogTraceException(ex, MethodBase.GetCurrentMethod().DeclaringType.FullName, MethodBase.GetCurrentMethod().Name);
                throw;
            }
        }
        private Task ExceptionReceivedHandler(ExceptionReceivedEventArgs exceptionReceivedEventArgs)
        {
            var context = exceptionReceivedEventArgs.ExceptionReceivedContext;
            //Console.WriteLine("Exception context for troubleshooting:");
            //Console.WriteLine($"- Endpoint: {context.Endpoint}");
            //Console.WriteLine($"- Entity Path: {context.EntityPath}");
            //Console.WriteLine($"- Executing Action: {context.Action}");

            string errorMessage = string.Format("Endpoint: {0} Entity Path: {1} Executing Action: {2}", context.Endpoint, context.EntityPath, context.Action);
            string method = string.Format("{0}.{1}", MethodBase.GetCurrentMethod().DeclaringType.FullName, MethodBase.GetCurrentMethod().Name);
            Exception exception = new Exception(errorMessage);
            //LoggingHelper.LogTraceException(exception, MethodBase.GetCurrentMethod().DeclaringType.FullName, MethodBase.GetCurrentMethod().Name);

            return Task.CompletedTask;
        }






        private async Task SetStorageQueueMessage(string storageConnectionString, string storageQueueName, CloudQueueMessage cloudQueue)
        {
            try
            {
                CloudStorageAccount storageAccount = CloudStorageAccount.Parse(storageConnectionString);

                CloudQueueClient queueClient = storageAccount.CreateCloudQueueClient();
                CloudQueue queue = queueClient.GetQueueReference(storageQueueName);
                queue.CreateIfNotExistsAsync();
                await queue.AddMessageAsync(cloudQueue);
            }
            catch (Exception ex)
            {
                //LoggingHelper.LogTraceException(ex, MethodBase.GetCurrentMethod().DeclaringType.FullName, MethodBase.GetCurrentMethod().Name);
                throw;
            }
        }
        public async Task<Collection<DataUploadServiceBusProperties>> GetStorageQueueDataUploadMessages(string storageQueueConnection, string queueName)
        {
            try
            {
                dataUploadServiceBuses = new Collection<DataUploadServiceBusProperties>();
                CloudQueueMessage queueMessage = await GetStorageMessage(storageQueueConnection, queueName);
                if (queueMessage != null)
                {
                    DataUploadServiceBusProperties dataUploadService = JsonConvert.DeserializeObject<DataUploadServiceBusProperties>(queueMessage.AsString);
                    dataUploadServiceBuses.Add(dataUploadService);
                }
                return dataUploadServiceBuses;
            }
            catch (Exception ex)
            {
                //LoggingHelper.LogTraceException(ex, MethodBase.GetCurrentMethod().DeclaringType.FullName, MethodBase.GetCurrentMethod().Name);
                throw;
            }
        }
        private async Task<CloudQueueMessage> GetStorageMessage(string storageConnectionString, string storageQueueName)
        {
            try
            {
                CloudStorageAccount storageAccount = CloudStorageAccount.Parse(storageConnectionString);
                CloudQueueClient queueClient = storageAccount.CreateCloudQueueClient();
                CloudQueue queue = queueClient.GetQueueReference(storageQueueName);
                CloudQueueMessage retrievedMessage = await queue.GetMessageAsync();
                if (retrievedMessage != null)
                {
                    await queue.DeleteMessageAsync(retrievedMessage);
                }
                return retrievedMessage;
            }
            catch (Exception ex)
            {
                //LoggingHelper.LogTraceException(ex, MethodBase.GetCurrentMethod().DeclaringType.FullName, MethodBase.GetCurrentMethod().Name);
                throw;
            }
        }



     
    }
}


