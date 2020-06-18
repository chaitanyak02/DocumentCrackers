using System;
using System.Collections.Generic;
using System.Text;
using SSA.Entity;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

using System.IO;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
using Microsoft.Azure.Management.ResourceManager.Models;
using Microsoft.Azure.Management.ResourceManager;
using Microsoft.Rest.Azure.Authentication;
using Microsoft.Azure.Management.ResourceManager.Fluent;
using System.Threading.Tasks;
using Microsoft.Azure.Batch;
using System.Threading;
using Microsoft.Azure.Batch.Auth;
using System.IdentityModel.Tokens;
using Microsoft.IdentityModel.Clients.ActiveDirectory;
using AuthenticationContext = Microsoft.IdentityModel.Clients.ActiveDirectory.AuthenticationContext;
using Microsoft.Azure.Batch.Common;
using System.Diagnostics;
using System.Linq;

namespace Infosys.DC.Preprocessing
{
    class BatchCreater
    {
        private string subscriptionId;
        private string clientId;
        private string clientSecret;
        private string resourceGroupName;
        private string deploymentName;
        private string pathToTemplateFile;
        private string pathToParameterFile;
        private string tenantId;

        public string SubscriptionId { get => subscriptionId; set => subscriptionId = value; }
        public string ClientId { get => clientId; set => clientId = value; }
        public string ClientSecret { get => clientSecret; set => clientSecret = value; }
        public string ResourceGroupName { get => resourceGroupName; set => resourceGroupName = value; }
        public string DeploymentName { get => deploymentName; set => deploymentName = value; }
        public string PathToTemplateFile { get => pathToTemplateFile; set => pathToTemplateFile = value; }
        public string PathToParameterFile { get => pathToParameterFile; set => pathToParameterFile = value; }
        public string TenantId { get => tenantId; set => tenantId = value; }

        private static int i = 0;
        public void CreateBatch(BatchParams prmBatchParameters)
        {
            // Try to obtain the service credentials
            var serviceCreds = ApplicationTokenProvider.LoginSilentAsync(TenantId, ClientId, ClientSecret).GetAwaiter().GetResult();

            // Read the template and parameter file contents
            JObject templateFileContents = GetJsonFileContents(PathToTemplateFile);
            //JObject parameterFileContents = GetJsonFileContents(PathToParameterFile);

            JObject parameterFileContents = new JObject();

            var paramString = prmBatchParameters.GetBatchParamJson();
            parameterFileContents = JObject.Parse(paramString);

            // Create the resource manager client
            var resourceManagementClient = new Microsoft.Azure.Management.ResourceManager.ResourceManagementClient(serviceCreds);
            resourceManagementClient.SubscriptionId = SubscriptionId;

            // Start a deployment
            DeployTemplate(resourceManagementClient, ResourceGroupName, DeploymentName,
                templateFileContents, parameterFileContents);
        }

        /// <summary>
        /// Reads a JSON file from the specified path
        /// </summary>
        /// <param name="pathToJson">The full path to the JSON file</param>
        /// <returns>The JSON file contents</returns>
        private JObject GetJsonFileContents(string pathToJson)
        {
            JObject templatefileContent = new JObject();
            using (StreamReader file = File.OpenText(pathToJson))
            {
                using (JsonTextReader reader = new JsonTextReader(file))
                {
                    templatefileContent = (JObject)JToken.ReadFrom(reader);
                    return templatefileContent;
                }
            }
        }


        /// <summary>
        /// Starts a template deployment.
        /// </summary>
        /// <param name="resourceManagementClient">The resource manager client.</param>
        /// <param name="resourceGroupName">The name of the resource group.</param>
        /// <param name="deploymentName">The name of the deployment.</param>
        /// <param name="templateFileContents">The template file contents.</param>
        /// <param name="parameterFileContents">The parameter file contents.</param>
        private static void DeployTemplate(Microsoft.Azure.Management.ResourceManager.ResourceManagementClient resourceManagementClient,
            string resourceGroupName, string deploymentName, JObject templateFileContents,
            JObject parameterFileContents)
        {
            Console.WriteLine(string.Format("Starting template deployment '{0}' in resource group '{1}'", deploymentName, resourceGroupName));
            var deployment = new Deployment();

            deployment.Properties = new Microsoft.Azure.Management.ResourceManager.Models.DeploymentProperties
            {
                Mode = Microsoft.Azure.Management.ResourceManager.Models.DeploymentMode.Incremental,
                Template = templateFileContents,
                Parameters = parameterFileContents  //parameterFileContents["parameters"].ToObject<JObject>()
            };

            var deploymentResult = resourceManagementClient.Deployments.CreateOrUpdateAsync(resourceGroupName, deploymentName, deployment);
            //Console.WriteLine(string.Format("Deployment status: {0}", deploymentResult.ProvisioningState));
        }
    }

    class BatchParams
    {
        private string batchAccount;
        private string vNET;
        private string subNET;
        private string location;
        private string poolname;
        private string vmSize;
        private int targetNodes;
        private int maxTasks;
        private string sku;
        private string nodeAgentSkuId;
        private string resizeTimeout;
        private JObject resourceFilesSettings;
        private string storageURL;

        public string BatchAccount { get => batchAccount; set => batchAccount = value; }
        public string VNET { get => vNET; set => vNET = value; }
        public string SubNET { get => subNET; set => subNET = value; }
        public string Location { get => location; set => location = value; }
        public string Poolname { get => poolname; set => poolname = value; }
        public string VmSize { get => vmSize; set => vmSize = value; }
        public int TargetNodes { get => targetNodes; set => targetNodes = value; }
        public int MaxTasks { get => maxTasks; set => maxTasks = value; }
        public string Sku { get => sku; set => sku = value; }
        public string NodeAgentSkuId { get => nodeAgentSkuId; set => nodeAgentSkuId = value; }
        public string ResizeTimeout { get => resizeTimeout; set => resizeTimeout = value; }
        public string StorageURL { get => storageURL; set => storageURL = value; }
        public JObject ResourceFiles { get => resourceFilesSettings; set => resourceFilesSettings = value; }

        public string GetBatchParamJson()
        {
            object data = new
            {
                batchAccount = new
                {
                    value = BatchAccount
                },
                vNET = new
                {
                    value = VNET
                },
                subNET = new
                {
                    value = SubNET
                },
                location = new
                {
                    value = Location
                },
                poolname = new
                {
                    value = Poolname
                },
                vmSize = new
                {
                    value = VmSize
                },
                targetNodes = new
                {
                    value = TargetNodes
                },
                maxTasks = new
                {
                    value = MaxTasks
                },
                sku = new
                {
                    value = Sku
                },
                nodeAgentSkuId = new
                {
                    value = NodeAgentSkuId
                },
                resizeTimeout = new
                {
                    value = ResizeTimeout
                },
                storageURL = new
                {
                    value = StorageURL
                },
                resourceFilesSettings = new
                {

                    value = ResourceFiles
                }
            };
            var returnValue = JsonConvert.SerializeObject(data, Formatting.Indented);
            return returnValue;
        }


       

        public JObject GetResourceParamJson(string[] resourcefiles, string strStorgeAccount, string strKey)
        {

            var jsonObject = new JObject();
            dynamic resourcefileObject = jsonObject;

            resourcefileObject.resourcefiles = new JArray() as dynamic;

            dynamic resourcefile = new JObject();

            for (int i = 0; i < resourcefiles.Length; i++)
            {
                if (i != 0)
                {
                    resourcefile = new JObject();
                }

                resourcefile.blobSource = strStorgeAccount + resourcefiles[i] + strKey;
                resourcefile.filePath = resourcefiles[i];
                resourcefileObject.resourcefiles.Add(resourcefile);
            }
            return jsonObject;
        }

    }
    public static class BatchOperations
    {
        private const string BatchAccountName = "";
        //private const string BatchAccountName = "";
        //private const string BatchAccountUrl = "";
        //private const string BatchAccountKey ="";
        private const string BatchAccountKey = "";
        //private const string BatchAccountKey = "";
        private const string BatchAccountUrl = "";
        private static string PoolId = "Converge2020Pool";
        private const string StorageAccountName = "";
        private const string StorageAccountKey = "";
        //private const string StorageAccountName = "";
        //private const string StorageAccountKey = "key";

        //public static async void TriggerAureBatch(DataUploadServiceBusProperties message)
        public static async void TriggerAureBatch()
        {
            try
            { 
            //BatchArmTemplate();
            if (String.IsNullOrEmpty(BatchAccountName) || String.IsNullOrEmpty(BatchAccountKey) || String.IsNullOrEmpty(BatchAccountUrl) ||
               String.IsNullOrEmpty(StorageAccountName) || String.IsNullOrEmpty(StorageAccountKey))
            {
                throw new InvalidOperationException("One ore more account credential strings have not been populated. Please ensure that your Batch and Storage account credentials have been specified.");
            }
                Console.WriteLine("Sample start: {0}", DateTime.Now);
                Console.WriteLine();
                Stopwatch timer = new Stopwatch();
                timer.Start();

                string storageConnectionString =
                        $"DefaultEndpointsProtocol=https;AccountName={StorageAccountName};AccountKey={StorageAccountKey}";

            // Retrieve the storage account
            CloudStorageAccount storageAccount = CloudStorageAccount.Parse(storageConnectionString);

            // Create the blob client, for use in obtaining references to blob storage containers
            CloudBlobClient blobClient = storageAccount.CreateCloudBlobClient();

            // Use the blob client to create the containers in Azure Storage if they don't yet exist
            const string appContainerName = "converge-application";
            const string inputContainerName = "converge-input";
            const string outputContainerName = "converge-output";
            const string PDfContainerName = "converge-pdfimages";
            const string ClusterContainerName = "converge-clusterimages";


                //Create all containers
                CloudBlobContainer appContainer = blobClient.GetContainerReference(appContainerName);
            appContainer.CreateIfNotExists();
            CloudBlobContainer inputContainer = blobClient.GetContainerReference(inputContainerName);
            inputContainer.CreateIfNotExists();
            CloudBlobContainer outputContainer = blobClient.GetContainerReference(outputContainerName);
            outputContainer.CreateIfNotExists();

            CloudBlobContainer PDfContainer = blobClient.GetContainerReference(PDfContainerName);
            PDfContainer.CreateIfNotExists();

            CloudBlobContainer ClusterContainer = blobClient.GetContainerReference(ClusterContainerName);
            ClusterContainer.CreateIfNotExists();


                // Paths to the Python Files.
                List<string> applicationFilePaths = new List<string>
            {
              
                 Environment.CurrentDirectory+"\\PyFiles\\UnsupervisedImageClassification.py",
                 Environment.CurrentDirectory+"\\PyFiles\\AzureDownloadFile.py",
                 Environment.CurrentDirectory+"\\PyFiles\\ExtractmagesFromPDF.py",
                 Environment.CurrentDirectory+"\\PyFiles\\StartAzureBatch.py",
                 Environment.CurrentDirectory+"\\PyFiles\\UploadFile.py",
                 Environment.CurrentDirectory+"\\PyFiles\\Log.py"

            };
            List<ResourceFile> applicationFiles = await UploadFilesToContainerAsync(blobClient, appContainerName, applicationFilePaths);
                // Obtain a shared access signature that provides write access to the output container to which
                // the tasks will upload their output.
                string outputContainerSasUrl = GetContainerSasUrl(blobClient, outputContainerName, SharedAccessBlobPermissions.Write);

                // Create a BatchClient. We'll now be interacting with the Batch service in addition to Storage
                BatchSharedKeyCredentials cred = new BatchSharedKeyCredentials(BatchAccountUrl, BatchAccountName, BatchAccountKey);

               

                    using (BatchClient batchClient = BatchClient.Open(cred))
                    {
                    // Create the pool that will contain the compute nodes that will execute the tasks.
                    // The ResourceFile collection that we pass in is used for configuring the pool's StartTask
                    // which is executed each time a node first joins the pool (or is rebooted or reimaged).

                    //uncomment this while deployiong

                        //CreatePoolIfNotExistAsync(batchClient, PoolId, applicationFiles);

                        string DiskNames = "Disk1";
                    
                   
                    
                    int JobCounter = 0;
                    int GlobalJobCounter = 0;
                    List<String> lstJobids = new List<string>();
                    String Blob_Name = string.Empty;
                    string[] blobstring = null;

                    
                    foreach (string DiskName in DiskNames.Split(','))
                    {
                        string JobId = PoolId + "_" + DiskName + "_" + (GlobalJobCounter).ToString();

                     

                
                        CloudBlobContainer container = null;
                        CloudBlobClient BlobClient = null;

                       
                        
                        


                        //Checking if the job is already present 
                        JobId = JobId.Replace(" ", "_");
                        CreateJobAsync(batchClient, JobId, PoolId);
                        lstJobids.Add(JobId);

                        // Get a flat listing of all the block blobs in the specified container
                        CloudBlobDirectory blobDirectory = container.GetDirectoryReference(DiskName);
                        //var bloblist = blobDirectory.ListBlobs(true).ToList();

                        blobDirectory = container.GetDirectoryReference(DiskName);
                        List<IListBlobItem> bloblist = new List<IListBlobItem>();
                       


                               
                        AddTasks(batchClient, JobId, DiskName );
                        //resourceFiles = new List<ResourceFile>();
                        //JobCounter = 0;
                        JobId = PoolId + "_" + DiskName + "_" + (++GlobalJobCounter).ToString();
                        CreateJobAsync(batchClient, JobId, PoolId);
                        lstJobids.Add(JobId);

                              

                                // Save blob contents to a file in the specified folder


                                // Monitor task success/failure, specifying a maximum amount of time to wait for the tasks to complete

                                JobCounter++;

                            }
                        



                      

                    }


                    

                    // Print out some timing info
                    timer.Stop();
                    Console.WriteLine();
                    Console.WriteLine("Sample end: {0}", DateTime.Now);
                    Console.WriteLine("Elapsed time: {0}", timer.Elapsed);

                  
                
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        
           
            finally
            {
                Console.WriteLine();
                Console.WriteLine("Sample complete, hit ENTER to exit...");
                Console.ReadLine();
            }
        }
        private static async Task<bool> MonitorTasks(BatchClient batchClient, string jobId, TimeSpan timeout)
        {
            bool allTasksSuccessful = true;
            const string successMessage = "All tasks reached state Completed.";
            const string failureMessage = "One or more tasks failed to reach the Completed state within the timeout period.";

            // Obtain the collection of tasks currently managed by the job. Note that we use a detail level to
            // specify that only the "id" property of each task should be populated. Using a detail level for
            // all list operations helps to lower response time from the Batch service.
            ODATADetailLevel detail = new ODATADetailLevel(selectClause: "id");
            List<CloudTask> tasks = await batchClient.JobOperations.ListTasks(jobId, detail).ToListAsync();

            Console.WriteLine("Awaiting task completion, timeout in {0}...", timeout.ToString());

            // We use a TaskStateMonitor to monitor the state of our tasks. In this case, we will wait for all tasks to
            // reach the Completed state.
            TaskStateMonitor taskStateMonitor = batchClient.Utilities.CreateTaskStateMonitor();
            try
            {
                await taskStateMonitor.WhenAll(tasks, TaskState.Completed, timeout);
            }
            catch (TimeoutException)
            {
                await batchClient.JobOperations.TerminateJobAsync(jobId, failureMessage);
                Console.WriteLine(failureMessage);
                return false;
            }

            await batchClient.JobOperations.TerminateJobAsync(jobId, successMessage);

            // All tasks have reached the "Completed" state, however, this does not guarantee all tasks completed successfully.
            // Here we further check each task's ExecutionInfo property to ensure that it did not encounter a scheduling error
            // or return a non-zero exit code.

            // Update the detail level to populate only the task id and executionInfo properties.
            // We refresh the tasks below, and need only this information for each task.
            detail.SelectClause = "id, executionInfo";

            foreach (CloudTask task in tasks)
            {
                // Populate the task's properties with the latest info from the Batch service
                await task.RefreshAsync(detail);

                NodeFile nodFile = task.GetNodeFile(Microsoft.Azure.Batch.Constants.StandardOutFileName);
                Console.WriteLine("Task " + task.Id + " stdout:\n" + nodFile.ReadAsString());

                nodFile = task.GetNodeFile(Microsoft.Azure.Batch.Constants.StandardErrorFileName);
                Console.WriteLine("Task " + task.Id + " stderr:\n" + nodFile.ReadAsString());



                if (task.ExecutionInformation.Result == TaskExecutionResult.Failure)
                {
                    // A task with failure information set indicates there was a problem with the task. It is important to note that
                    // the task's state can be "Completed," yet still have encountered a failure.

                    allTasksSuccessful = false;

                    Console.WriteLine("WARNING: Task [{0}] encountered a failure: {1}", task.Id, task.ExecutionInformation.FailureInformation.Message);
                    if (task.ExecutionInformation.ExitCode != 0)
                    {
                        // A non-zero exit code may indicate that the application executed by the task encountered an error
                        // during execution. As not every application returns non-zero on failure by default (e.g. robocopy),
                        // your implementation of error checking may differ from this example.

                        Console.WriteLine("WARNING: Task [{0}] returned a non-zero exit code - this may indicate task execution or completion failure.", task.Id);
                    }
                }
            }

            if (allTasksSuccessful)
            {
                Console.WriteLine("Success! All tasks completed successfully within the specified timeout period.");
            }

            return allTasksSuccessful;
        }

        private static int i = 0;
        private static  void AddTasks(BatchClient batchClient, string jobId,  string DiskName)
        {

            Console.WriteLine("In AddTasksAsync...Disk Name", DiskName);
            //Console.WriteLine("Adding {0} tasks to job [{1}]...", inputFiles.Count, jobId);

            // Create a collection to hold the tasks that we'll be adding to the job
            List<CloudTask> tasks = new List<CloudTask>();


           

                string commandLine =
                @"cmd /c %AZ_BATCH_APP_PACKAGE_apppackage#1.0%\Python36_64\python.exe %AZ_BATCH_NODE_SHARED_DIR%\StartAzureBatch.py " + DiskName ;
                //@"cmd /c %AZ_BATCH_APP_PACKAGE_ssapythonapplication#1.0%\Python\python.exe %AZ_BATCH_NODE_SHARED_DIR%\StartBatchProcessing.py WH resdevfiles ironmountain IM4 720481562 720481562_2000086895_WLL0009013.TIF";

                CloudTask PythonTask = new CloudTask("PythonTask_" + i, commandLine);
                //PythonTask.ResourceFiles = new List<ResourceFile> { inputFile };
                tasks.Add(PythonTask);
                i++;
           
        }
        private static async Task<ResourceFile> getInputFiles(CloudBlobClient blobClient, string containerName, string VendorName, string DiskName, string HDID, string boxName, string fileName)
        {
            Console.WriteLine("In GetInputFiles boxName...", boxName);
            Console.WriteLine("Adding file {0} ]...", fileName);


            //string blobName = Path.GetFileName(filePath);

            CloudBlobContainer container = blobClient.GetContainerReference(containerName);
            CloudBlockBlob blobData;
            if (HDID != "No")
            {
                blobData = container.GetBlockBlobReference(VendorName + "/" + DiskName + "/" + HDID + "/" + boxName + "/" + fileName);
            }
            else
            {
                blobData = container.GetBlockBlobReference(VendorName + "/" + DiskName + "/" + boxName + "/" + fileName);
                //blobData = container.GetBlockBlobReference( DiskName + "/" + boxName + "/" + fileName);
            }
            // Set the expiry time and permissions for the blob shared access signature. In this case, no start time is specified,
            // so the shared access signature becomes valid immediately
            SharedAccessBlobPolicy sasConstraints = new SharedAccessBlobPolicy
            {
                SharedAccessExpiryTime = DateTime.UtcNow.AddHours(48),
                Permissions = SharedAccessBlobPermissions.Read
            };

            // Construct the SAS URL for blob
            string sasBlobToken = blobData.GetSharedAccessSignature(sasConstraints);
            string blobSasUri = $"{blobData.Uri}{sasBlobToken}";

            return new ResourceFile(blobSasUri.Replace(" ", "%20"), fileName);



        }
        private static void CreateJobAsync(BatchClient batchClient, string jobId, string poolId)
        {
            jobId = jobId.Replace(" ", string.Empty);
            CloudJob job =  GetJobIfExistAsync(batchClient, jobId);
            if (job != null)
            {

                Console.WriteLine("Job {0} already exists...Deleting and recreating it", jobId);
                batchClient.JobOperations.DeleteJobAsync(jobId);

                //Making the process to wait for 5 seconds becuase of batch error
                System.Threading.Thread.Sleep(10000);

                Console.WriteLine("Job {0} deleted", jobId);
                Console.WriteLine("Recreating Job {0}", jobId);
                CloudJob newjob = batchClient.JobOperations.CreateJob();
                newjob.Id = jobId;
                newjob.PoolInformation = new PoolInformation { PoolId = poolId };
                //job.PoolInformation = new PoolInformation { PoolId = "DotNetPoolwithSubnet_5" }; 
                //await newjob.CommitAsync();

            }
            else
            {
                Console.WriteLine("Creating job [{0}]...", jobId);

                CloudJob newjob = batchClient.JobOperations.CreateJob();
                newjob.Id = jobId;
                newjob.PoolInformation = new PoolInformation { PoolId = poolId };
                //job.PoolInformation = new PoolInformation { PoolId = "DotNetPoolwithSubnet_5" }; 

                 newjob.CommitAsync();
            }


        }
        public static  CloudJob GetJobIfExistAsync(BatchClient batchClient, string jobId)
        {
            Console.WriteLine("Checking for existing job {0}...", jobId);

            // Construct a detail level with a filter clause that specifies the job ID so that only
            // a single CloudJob is returned by the Batch service (if that job exists)
            ODATADetailLevel detail = new ODATADetailLevel(filterClause: string.Format("id eq '{0}'", jobId));
            List<CloudJob> jobs =  batchClient.JobOperations.ListJobs(detailLevel: detail).ToList();

            return jobs.FirstOrDefault();
        }
        private const string AuthorityUri = "";
        private const string BatchResourceUri = "";
        //private const string ClientId = "";
        private const string ClientId = "";
        //private const string ClientKey = "";
        private const string ClientKey = "";
        public static async Task<string> GetAuthenticationTokenAsync()
        {
            AuthenticationContext authContext = new AuthenticationContext(AuthorityUri);
            AuthenticationResult authResult = await authContext.AcquireTokenAsync(BatchResourceUri, new ClientCredential(ClientId, ClientKey));

            return authResult.AccessToken;
        }
        private  static void  CreatePoolIfNotExistAsync(BatchClient batchClient, string poolId, IList<ResourceFile> resourceFiles)
        {
            CloudPool pool = null;
            try
            {
                Console.WriteLine("Creating pool [{0}]...", poolId);

                pool = batchClient.PoolOperations.CreatePool(
                    poolId: poolId,
                    targetDedicatedComputeNodes: 2,                                             // 3 compute nodes
                    virtualMachineSize: "standard_d2_v3",                                       // single-core, 3.5 GB memory, 50 GB disk
                    cloudServiceConfiguration: new CloudServiceConfiguration(osFamily: "5"));   // Windows Server 2016
                //                                                                                // }
                //                                                                                //pool.NetworkConfiguration = new NetworkConfiguration()
                //                                                                                //{
                // 
                pool.ApplicationPackageReferences = new List<Microsoft.Azure.Batch.ApplicationPackageReference>
                {
                    new ApplicationPackageReference {
                        ApplicationId = "apppackage",
                        Version = "1.0" }
                };//    SubnetId = "/subscriptions/{subscriptionId}/resourceGroups/Default-Networking/providers/Microsoft.ClassicNetwork/virtualNetworks/{VnetId}/subnets/{SubnetId}"
                pool.MaxTasksPerComputeNode = 64;                                                                          //};

                pool.StartTask = new StartTask
                {
                    // Specify a command line for the StartTask that copies the task application files to the
                    // node's shared directory. Every compute node in a Batch pool is configured with a number
                    // of pre-defined environment variables that can be referenced by commands or applications
                    // run by tasks.

                    // Since a successful execution of robocopy can return a non-zero exit code (e.g. 1 when one or
                    // more files were successfully copied) we need to manually exit with a 0 for Batch to recognize
                    // StartTask execution success.
                    //This is Working
                    //////CommandLine = "cmd /c (robocopy %AZ_BATCH_TASK_WORKING_DIR% %AZ_BATCH_NODE_SHARED_DIR%) ^& IF %ERRORLEVEL% LEQ 1 exit 0",
                    //CommandLine = @"cmd /c (robocopy % AZ_BATCH_TASK_WORKING_DIR % % AZ_BATCH_NODE_SHARED_DIR %)& % AZ_BATCH_NODE_SHARED_DIR %\7z.exe x % AZ_BATCH_NODE_SHARED_DIR %\Tesseract - OCR.zip - o % AZ_BATCH_NODE_SHARED_DIR %\^ &IF % ERRORLEVEL % LEQ 1 exit 0",
                    CommandLine = @"cmd /c  (robocopy %AZ_BATCH_TASK_WORKING_DIR% %AZ_BATCH_NODE_SHARED_DIR%) ^& IF %ERRORLEVEL% LEQ 1 exit 0",
                    ResourceFiles = resourceFiles,
                    WaitForSuccess = true
                };
                
                 pool.Commit();
              
            }
            catch (BatchException be)
            {
                // Swallow the specific error code PoolExists since that is expected if the pool already exists
                if (be.RequestInformation?.BatchError != null && be.RequestInformation.BatchError.Code == BatchErrorCodeStrings.PoolExists)
                {
                    Console.WriteLine("The pool {0} already existed when we tried to create it", poolId);
                }
                else
                {
                    throw; // Any other exception is unexpected
                }
            }
        }
        private static string GetContainerSasUrl(CloudBlobClient blobClient, string containerName, SharedAccessBlobPermissions permissions)
        {
            // Set the expiry time and permissions for the container access signature. In this case, no start time is specified,
            // so the shared access signature becomes valid immediately
            SharedAccessBlobPolicy sasConstraints = new SharedAccessBlobPolicy
            {
                SharedAccessExpiryTime = DateTime.UtcNow.AddHours(96),
                Permissions = permissions
            };

            // Generate the shared access signature on the container, setting the constraints directly on the signature
            CloudBlobContainer container = blobClient.GetContainerReference(containerName);
            string sasContainerToken = container.GetSharedAccessSignature(sasConstraints);

            // Return the URL string for the container, including the SAS token
            return string.Format("{0}{1}", container.Uri, sasContainerToken);
        }
        private static async Task<List<ResourceFile>> UploadFilesToContainerAsync(CloudBlobClient blobClient, string inputContainerName, List<string> filePaths)
        {
            List<ResourceFile> resourceFiles = new List<ResourceFile>();

            foreach (string filePath in filePaths)
            {
                resourceFiles.Add(await UploadFileToContainerAsync(blobClient, inputContainerName, filePath));
            }

            return resourceFiles;
        }
        private static async Task<ResourceFile> UploadFileToContainerAsync(CloudBlobClient blobClient, string containerName, string filePath)
        {
            Console.WriteLine("Uploading file {0} to container [{1}]...", filePath, containerName);

            string blobName = Path.GetFileName(filePath);

            CloudBlobContainer container = blobClient.GetContainerReference(containerName);
            CloudBlockBlob blobData;
            
            blobData = container.GetBlockBlobReference(blobName);
            blobData.UploadFromFile(filePath);
            


            // Set the expiry time and permissions for the blob shared access signature. In this case, no start time is specified,
            // so the shared access signature becomes valid immediately
            SharedAccessBlobPolicy sasConstraints = new SharedAccessBlobPolicy
            {
                SharedAccessExpiryTime = DateTime.UtcNow.AddHours(24),
                Permissions = SharedAccessBlobPermissions.Read
            };

            // Construct the SAS URL for blob
            string sasBlobToken = blobData.GetSharedAccessSignature(sasConstraints);
            string blobSasUri = $"{blobData.Uri}{sasBlobToken}";

            return new ResourceFile(blobSasUri, blobName);
        }
        private static void BatchArmTemplate()
        {

            BatchCreater dp = new BatchCreater();
            dp.SubscriptionId = Constants.SubscriptionId;
            dp.ClientId = Constants.AzureBatchClientID;
            dp.ClientSecret = Constants.AzureBatchClientSecret;
            dp.ResourceGroupName = "ZNE-USW0-N-20-SSA-UAT-RSG";
            dp.DeploymentName = Constants.AzureBatchDeploymentName;
            dp.PathToTemplateFile = Environment.CurrentDirectory + Constants.AzureBatchTemplate;
            dp.PathToParameterFile = Environment.CurrentDirectory + Constants.AzureBatchParameter;
            dp.TenantId = Constants.DirectoryId;

            BatchParams bparm = new BatchParams();
            bparm.BatchAccount = Constants.AzureBatchAccount;
            bparm.VNET = Constants.AzureBatchVNet;
            bparm.SubNET = Constants.AzureBatchSubnet;
            bparm.Location = Constants.AzureBatchLocation;
            bparm.Poolname = Constants.AzureBatchPoolName;
            bparm.VmSize = Constants.AzureBatchVMSize;
            bparm.TargetNodes = Convert.ToInt32(Constants.AzureBatchTargetNodes);
            bparm.MaxTasks = Convert.ToInt32(Constants.AzureBatchMaxTask);
            bparm.Sku = Constants.AzureBatchSku;
            bparm.NodeAgentSkuId = Constants.AzureBatchAgentSkuId;
            bparm.ResizeTimeout = Constants.AzureBatchResizeTimeout;

            dp.CreateBatch(bparm);

        }



        private static async Task CreateContainerIfNotExistAsync(CloudBlobClient blobClient, string containerName)
        {
            try {
                CloudBlobContainer container = blobClient.GetContainerReference(containerName);

                if (await container.CreateIfNotExistsAsync())
                {
                    Console.WriteLine("Container [{0}] created.", containerName);
                }
                else
                {
                    Console.WriteLine("Container [{0}] exists, skipping creation.", containerName);
                }
            }
            catch(Exception ex)
            {

            }
            
            }

        
    }
}

