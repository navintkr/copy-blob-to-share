using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
using Microsoft.WindowsAzure.Storage.File;
using Microsoft.WindowsAzure.Storage.Core;

namespace coyblobtoshare
{
    public static class Function1
    {
        [FunctionName("Function1")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Function, "get", "post", Route = null)] HttpRequest req,
            ILogger log)
        {
            log.LogInformation("C# HTTP trigger function processed a request.");

            CloudStorageAccount storageAccount = CloudStorageAccount.Parse("<connection string from storage account>");

            var blobClient = storageAccount.CreateCloudBlobClient();
            CloudBlobContainer cloudBlobContainer = blobClient.GetContainerReference("<source container>");
            CloudBlockBlob sourceCloudBlockBlob = cloudBlobContainer.GetBlockBlobReference("<Source Blob name>");

            //Note that if the file share is in a different storage account, you should use CloudStorageAccount storageAccount2 = CloudStorageAccount.Parse("the other storage connection string"), then use storageAccount2 for the file share.
            CloudFileClient fileClient = storageAccount.CreateCloudFileClient();
            CloudFileShare share = fileClient.GetShareReference("<fileshare name>");
            CloudFile destFile = share.GetRootDirectoryReference().GetFileReference("<File Nmae to be in Destination>");

            //Create a SAS for the source blob
            string blobSas = sourceCloudBlockBlob.GetSharedAccessSignature(new SharedAccessBlobPolicy()
            {
                Permissions = SharedAccessBlobPermissions.Read,
                SharedAccessExpiryTime = DateTime.UtcNow.AddHours(24)
            });

            Uri blobSasUri = new Uri(sourceCloudBlockBlob.StorageUri.PrimaryUri.ToString() + blobSas);
            await destFile.StartCopyAsync(blobSasUri);


            string responseMessage = "This HTTP triggered function executed successfully.";

            return new OkObjectResult(responseMessage);
        }
    }
}
