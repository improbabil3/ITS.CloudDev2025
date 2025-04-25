using Azure.Data.Tables;
using Azure.Storage.Blobs;
using ITS.CloudDev2025.API.DTOs;
using ITS.CloudDev2025.API.Entities;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace ITS.CloudDev2025.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class StorageController : ControllerBase
    {
        private readonly ILogger _logger;
        private readonly string _blobStorageConnectionString;
        private readonly string _customerTableName;

        public StorageController(ILogger<StorageController> logger, IConfiguration configuration)
        {
            _logger = logger;
            _blobStorageConnectionString = configuration["BlobStorage:ConnectionString"];
            _customerTableName = configuration["BlobStorage:CustomerTableName"];

        }

        /// <summary>
        /// Get a blob file from the specified container in Azure Blob Storage.
        /// </summary>
        /// <param name="containerName">The container name.</param>
        /// <param name="blobName">The blob file name.</param>
        /// <returns>Blob sas uri.</returns>
        /// <exception cref="InvalidOperationException"></exception>
        [HttpGet("blob/{containerName}/{blobName}")]
        [ProducesResponseType<Uri>(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public IActionResult GetBlobByName(string containerName, string blobName)
        {
            // Create a BlobServiceClient
            var blobServiceClient = new BlobServiceClient(_blobStorageConnectionString);

            // Get the container client
            var containerClient = blobServiceClient.GetBlobContainerClient(containerName);

            // Check if container exists
            if (!containerClient.Exists())
                throw new InvalidOperationException($"Container '{containerName}' does not exist.");

            var blobClient = containerClient.GetBlobClient(blobName);

            Uri sasUri = blobClient.GenerateSasUri(Azure.Storage.Sas.BlobSasPermissions.Write, DateTimeOffset.UtcNow.AddMinutes(10));

            return Ok(sasUri);
        }

        /// <summary>
        /// Upload a blob to the specified container in Azure Blob Storage.
        /// </summary>
        /// <param name="file">The file from httprequest.</param>
        /// <param name="containerName">The container name.</param>
        /// <returns>Blob created</returns>
        /// <exception cref="InvalidOperationException"></exception>
        [HttpPost("upload")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public IActionResult UploadBlob([FromBody] IFormFile file, [FromQuery] string containerName)
        {
            if (string.IsNullOrWhiteSpace(containerName))
                throw new InvalidOperationException("Container name is invalid.");

            if (file == null || file.Length == 0)
                throw new InvalidOperationException("File is invalid.");

            // Create a BlobServiceClient
            var blobServiceClient = new BlobServiceClient(_blobStorageConnectionString);

            // Get the container client
            var containerClient = blobServiceClient.GetBlobContainerClient(containerName);

            // Check if container exists
            if (!containerClient.Exists())
                throw new InvalidOperationException($"Container '{containerName}' does not exist.");

            var blobClient = containerClient.GetBlobClient(file.FileName);
            using (var stream = file.OpenReadStream())
            {
                blobClient.Upload(stream);
            }

            return Ok(true);
        }

        /// <summary>
        /// Save a customer to the Table Storage.
        /// </summary>
        /// <param name="name">The user name.</param>
        /// <param name="surname">The user surname.</param>
        /// <param name="email">The user email.</param>
        /// <param name="phoneNumber">The user phone number.</param>
        /// <returns>Customer id created.</returns>
        /// <exception cref="InvalidOperationException"></exception>
        [HttpPost("customer/save")]
        [ProducesResponseType<string>(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> SaveCustomer(string name, string surname, string email, string phoneNumber)
        {
            // Create a new TableClient
            var tableClient = new TableClient(_blobStorageConnectionString, _customerTableName);

            if (tableClient == null)
                throw new InvalidOperationException($"Table '{_customerTableName}' does not exist.");

            // Create a new customer entity
            Customer customer = Customer.Create(name, surname, email, phoneNumber);

            // Insert the entity into the table
            await tableClient.AddEntityAsync(customer);

            return Ok(customer.RowKey);
        }
    }
}
