using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;

namespace RaceDay.API.Services
{
    public class BlobStorageService
    {
        private readonly BlobServiceClient _blobServiceClient;
        private readonly string _containerName;

        public BlobStorageService(
            BlobServiceClient blobServiceClient,
            IConfiguration configuration)
        {
            _blobServiceClient = blobServiceClient;

            _containerName = configuration[
                "AzureBlobStorage:ContainerName"]
                ?? throw new InvalidOperationException(
                    "Azure Blob Storage container name is missing.");
        }

        public async Task<string> UploadImageAsync(
            Stream imageStream,
            string contentType)
        {
            var extension = contentType.ToLowerInvariant() switch
            {
                "image/jpeg" => ".jpg",
                "image/png" => ".png",
                "image/webp" => ".webp",
                _ => throw new ArgumentException(
                    "Unsupported image format.")
            };

            var containerClient =
                _blobServiceClient.GetBlobContainerClient(_containerName);

            await containerClient.CreateIfNotExistsAsync(
                PublicAccessType.None);

            string blobName = $"{Guid.NewGuid():N}{extension}";

            var blobClient = containerClient.GetBlobClient(blobName);

            await blobClient.UploadAsync(
                imageStream,
                new BlobUploadOptions
                {
                    HttpHeaders = new BlobHttpHeaders
                    {
                        ContentType = contentType
                    }
                });

            return blobName;
        }

        public async Task<BlobDownloadResult?> DownloadImageAsync(
            string blobName)
        {
            var containerClient =
                _blobServiceClient.GetBlobContainerClient(_containerName);

            var blobClient = containerClient.GetBlobClient(blobName);

            var exists = await blobClient.ExistsAsync();

            if (!exists.Value)
                return null;

            var download = await blobClient.DownloadContentAsync();

            return download.Value;
        }
    }
}