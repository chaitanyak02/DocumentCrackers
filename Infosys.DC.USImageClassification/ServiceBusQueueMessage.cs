//using Microsoft.Azure.ServiceBus;
//using Newtonsoft.Json;
//using SSA.Entity;
//using SSA.Logging;
//using System;
//using System.Collections.Generic;
//using System.Collections.ObjectModel;
//using System.Linq;
//using System.Reflection;
//using System.Text;
//using System.Threading;
//using System.Threading.Tasks;

//namespace BP.Upstream.Exploration.SubsurfaceScanning
//{
//    public class ServiceBusQueueMessage
//    {
//        private IQueueClient queueClient;
//        private Collection<ServiceBusQueue> messageEntities;
//        private Collection<MessageEntity> countryMessageEntities;
//        public async Task<Collection<ServiceBusQueue>> GetServiceBusMessages(string serviceBusConnection,string serviceBusQueue)
//        {
//            messageEntities = new Collection<ServiceBusQueue>();
//            try
//            {             
//                queueClient = new QueueClient(serviceBusConnection, serviceBusQueue);
//                RegisterOnMessageHandlerAndReceiveMessages();
//                await Task.Delay(TimeSpan.FromSeconds(30));
//                await queueClient.CloseAsync();
//                return messageEntities;
//            }
//            catch (Exception ex)
//            {
//                Logging.LogTraceException(ex, MethodBase.GetCurrentMethod().DeclaringType.FullName, MethodBase.GetCurrentMethod().Name);
//                throw;
//            }
//        }
//        private void RegisterOnMessageHandlerAndReceiveMessages()
//        {
//            try
//            {
//                var messageHandlerOptions = new MessageHandlerOptions(ExceptionReceivedHandler)
//                {
//                    MaxConcurrentCalls = 1,
//                    AutoComplete = false,
//                    MaxAutoRenewDuration = TimeSpan.FromMinutes(1)
//                };
//                queueClient.RegisterMessageHandler(ProcessMessagesAsync, messageHandlerOptions);
//            }
//            catch(Exception ex)
//            {
//                string method = string.Format("{0}.{1}", MethodBase.GetCurrentMethod().DeclaringType.FullName, MethodBase.GetCurrentMethod().Name);
//                ApplicationLogging.TraceExceptionLogging(ex, method, ex.InnerException.Message);
//                throw;
//            }
//        }
//        private async Task ProcessMessagesAsync(Message message, CancellationToken token)
//        {
//            try
//            {
//                string body = Encoding.UTF8.GetString(message.Body);
//                ServiceBusQueue messagequeue = JsonConvert.DeserializeObject<ServiceBusQueue>(body);
//                messageEntities.Add(messagequeue);
//                await queueClient.CompleteAsync(message.SystemProperties.LockToken);
//            }
//            catch (Exception ex)
//            {
//                string method = string.Format("{0}.{1}", MethodBase.GetCurrentMethod().DeclaringType.FullName, MethodBase.GetCurrentMethod().Name);
//                ApplicationLogging.TraceExceptionLogging(ex, method, ex.InnerException.Message);
//                throw;
//            }
//        }
//        private Task ExceptionReceivedHandler(ExceptionReceivedEventArgs exceptionReceivedEventArgs)
//        {
//            var context = exceptionReceivedEventArgs.ExceptionReceivedContext;
//            //Console.WriteLine("Exception context for troubleshooting:");
//            //Console.WriteLine($"- Endpoint: {context.Endpoint}");
//            //Console.WriteLine($"- Entity Path: {context.EntityPath}");
//            //Console.WriteLine($"- Executing Action: {context.Action}");
//            string errorMessage = string.Format("Endpoint: {0} Entity Path: {1} Executing Action: {2}", context.Endpoint, context.EntityPath, context.Action);
//            string method = string.Format("{0}.{1}", MethodBase.GetCurrentMethod().DeclaringType.FullName, MethodBase.GetCurrentMethod().Name);
//            ApplicationLogging.TraceExceptionLogging(exceptionReceivedEventArgs.Exception, method, errorMessage);
            
//            return Task.CompletedTask;
//        }

//        public async Task SetServiceBusMessage(string serviceBusConnection, string serviceBusQueue, ServiceBusQueue queueMessage)
//        {
//            try
//            {
//                queueClient = new QueueClient(serviceBusConnection, serviceBusQueue);
//                Message message = new Message(Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(queueMessage)));
//                message.ContentType = ConstantProperties.MessageContentType;
//                await queueClient.SendAsync(message);
//            }
//            catch (Exception ex)
//            {
//                string method = string.Format("{0}.{1}", MethodBase.GetCurrentMethod().DeclaringType.FullName, MethodBase.GetCurrentMethod().Name);
//                ApplicationLogging.TraceExceptionLogging(ex, method, ex.InnerException.Message);
//                throw;
//            }
//        }
//        public async Task SetServiceBusCountryMessage(string serviceBusConnection, string serviceBusQueue, MessageEntity queueMessage)
//        {
//            try
//            {
//                queueClient = new QueueClient(serviceBusConnection, serviceBusQueue);
//                Message message = new Message(Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(queueMessage)));
//                message.ContentType = ConstantProperties.MessageContentType;
//                await queueClient.SendAsync(message);
//            }
//            catch(Exception ex)
//            {
//                string method = string.Format("{0}.{1}", MethodBase.GetCurrentMethod().DeclaringType.FullName, MethodBase.GetCurrentMethod().Name);
//                ApplicationLogging.TraceExceptionLogging(ex, method, ex.InnerException.Message);
//                throw;
//            }
//        }

//        public async Task<Collection<MessageEntity>> GetServiceBusCountryMessages(string serviceBusConnection, string serviceBusQueue)
//        {
//            countryMessageEntities = new Collection<MessageEntity>();
//            try
//            {
//                queueClient = new QueueClient(serviceBusConnection, serviceBusQueue);
//                RegisterOnMessageHandlerAndReceiveCountryMessages();
//                await Task.Delay(TimeSpan.FromSeconds(30));
//                await queueClient.CloseAsync();
//                return countryMessageEntities;
//            }
//            catch (Exception ex)
//            {
//                string method = string.Format("{0}.{1}", MethodBase.GetCurrentMethod().DeclaringType.FullName, MethodBase.GetCurrentMethod().Name);
//                ApplicationLogging.TraceExceptionLogging(ex, method, ex.InnerException.Message);
//                return countryMessageEntities;
//            }
//        }
//        private void RegisterOnMessageHandlerAndReceiveCountryMessages()
//        {
//            try
//            {
//                var messageHandlerOptions = new MessageHandlerOptions(ExceptionReceivedHandler)
//                {
//                    MaxConcurrentCalls = 1,
//                    AutoComplete = false,
//                    MaxAutoRenewDuration = TimeSpan.FromMinutes(1)
//                };
//                queueClient.RegisterMessageHandler(ProcessCountryMessagesAsync, messageHandlerOptions);
//            }
//            catch(Exception ex)
//            {
//                string method = string.Format("{0}.{1}", MethodBase.GetCurrentMethod().DeclaringType.FullName, MethodBase.GetCurrentMethod().Name);
//                ApplicationLogging.TraceExceptionLogging(ex, method, ex.InnerException.Message);
//                throw;
//            }
//        }
//        private async Task ProcessCountryMessagesAsync(Message message, CancellationToken token)
//        {
//            try
//            {
//                string body = Encoding.UTF8.GetString(message.Body);
//                MessageEntity messagequeue = JsonConvert.DeserializeObject<MessageEntity>(body);
//                countryMessageEntities.Add(messagequeue);
//                await queueClient.CompleteAsync(message.SystemProperties.LockToken);
//            }
//            catch (Exception ex)
//            {
//                string method = string.Format("{0}.{1}", MethodBase.GetCurrentMethod().DeclaringType.FullName, MethodBase.GetCurrentMethod().Name);
//                ApplicationLogging.TraceExceptionLogging(ex, method, ex.InnerException.Message);
//                throw;
//            }
//        }
//    }
//}
