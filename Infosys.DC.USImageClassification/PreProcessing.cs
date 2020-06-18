using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SSA.Common;
using SSA.Entity;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Infosys.DC.Preprocessing;

namespace Converge2020
{
    class PreProcessing
    {
        static void Main(string[] args)
        {
            try
            {

                MessageQueue messageQueue = new MessageQueue();
                // Dictionary<string, string> keyValuePairs = CommonHelper.GetConfigurationList(keyValuePairsSecrets[ConstantProperties.TableStorageConfigurationSecret], keyValuePairsSecrets);

                //Collection<DataUploadServiceBusProperties> messages = messageQueue.GetStorageQueueDataUploadMessages(Constants.TableStorageConfigurationSecret, Constants.ServiceBusDataQueueName).Result;
                //messages.Add(new DataUploadServiceBusProperties { DataProcessing = "New Import", DiskNumber = "Converge/Disk1", Location = "Eastern Hemisphere", Vendor = "Infosys", Id = 0, CreatedBy = "Infosys.DC.Preprocessing", Status = "Initiated" });
                //foreach (DataUploadServiceBusProperties message in messages)
                //{
                //  if (message.Status == Constants.ProgramInitiatedStatus)
                // {
                StarteBatchOperations();
                //}
                //}
                //}
            }
            catch (Exception ex)
            {
                //gingHelper.LogTraceException(ex, MethodBase.GetCurrentMethod().DeclaringType.FullName, MethodBase.GetCurrentMethod().Name);
            }
        }
        private static void StarteBatchOperations(DataUploadServiceBusProperties message)
        {
            try
            {
                //BatchOperations.TriggerAureBatch(message);
                BatchOperations.TriggerAureBatch();
            }
            catch (Exception ex)
            {
                //gingHelper.LogTraceException(ex, MethodBase.GetCurrentMethod().DeclaringType.FullName, MethodBase.GetCurrentMethod().Name);
            }
        }
        private static void StarteBatchOperations()
        {
            try
            {
                BatchOperations.TriggerAureBatch();
            }
            catch (Exception ex)
            {
                //gingHelper.LogTraceException(ex, MethodBase.GetCurrentMethod().DeclaringType.FullName, MethodBase.GetCurrentMethod().Name);
            }
        }
    }
}
