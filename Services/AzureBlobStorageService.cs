using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Azure.Storage.Sas;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using System;
using System.IO;
using System.Threading.Tasks;

namespace WebApplication1.Services
{
    public class AzureBlobStorageService
    {
        private readonly BlobContainerClient _containerClient;

        public AzureBlobStorageService(IConfiguration configuration)
        {
            // Retrieve the connection string and container name from your configuration.
            string connectionString = configuration.GetConnectionString("AzureBlobStorage");
            string containerName = configuration["BlobContainerName"];

            // Create a BlobServiceClient and get a reference to a container.
            BlobServiceClient blobServiceClient = new BlobServiceClient(connectionString);
            _containerClient = blobServiceClient.GetBlobContainerClient(containerName);

            // Create the container if it does not already exist.
            _containerClient.CreateIfNotExists(PublicAccessType.None);
        }

        public async Task<string> UploadFileAsync(IFormFile file)
        {
            // Generate a unique name for the blob.
            string blobName = Guid.NewGuid().ToString() + Path.GetExtension(file.FileName);
            BlobClient blobClient = _containerClient.GetBlobClient(blobName);

            using (var stream = file.OpenReadStream())
            {
                await blobClient.UploadAsync(stream, new BlobHttpHeaders { ContentType = file.ContentType });
            }

            // Return the URI of the uploaded blob.
            return blobClient.Uri.ToString();
        }

        /// <summary>
        /// Generates a SAS URL for the specified blob with read permissions.
        /// </summary>
        /// <param name="blobName">The name of the blob (e.g., "myimage.jpg").</param>
        /// <returns>A SAS URL that grants temporary read access to the blob.</returns>
        public string GetBlobSasUri(string blobName)
        {
            // Get a BlobClient for the specified blob.
            BlobClient blobClient = _containerClient.GetBlobClient(blobName);

            // Check if the BlobClient is able to generate a SAS URI.
            if (blobClient.CanGenerateSasUri)
            {
                // Create a SAS token that expires in 30 minutes.
                BlobSasBuilder sasBuilder = new BlobSasBuilder
                {
                    BlobContainerName = _containerClient.Name,
                    BlobName = blobName,
                    Resource = "b", // "b" indicates a blob resource.
                    ExpiresOn = DateTimeOffset.UtcNow.AddMinutes(30)
                };

                // Grant read permissions.
                sasBuilder.SetPermissions(BlobSasPermissions.Read);

                // Generate and return the SAS URI.
                Uri sasUri = blobClient.GenerateSasUri(sasBuilder);
                return sasUri.ToString();
            }
            else
            {
                throw new InvalidOperationException("BlobClient cannot generate SAS URI. Ensure it was created with Shared Key credentials.");
            }
        }

        /// <summary>
        /// Generates a SAS URL for a blob using its full URL.
        /// </summary>
        /// <param name="blobUrl">The full URL of the blob.</param>
        /// <returns>A SAS URL that grants temporary read access to the blob.</returns>
        public string GetBlobSasUriFromUrl(string blobUrl)
        {
            // Extract the blob name from the URL.
            string blobName = new Uri(blobUrl).Segments[^1]; // The last segment is the blob name.
            return GetBlobSasUri(blobName);
        }
    }
}
