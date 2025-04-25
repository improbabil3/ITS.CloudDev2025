using Azure.Data.Tables;
using Azure;

namespace ITS.CloudDev2025.API.Entities
{
    public class Customer : ITableEntity
    {
        public string PartitionKey { get; set; }
        public string RowKey { get; set; }

        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
        public string PhoneNumber { get; set; }
        public DateTimeOffset? Timestamp { get; set; }
        public ETag ETag { get; set; }

        /// <summary>
        /// Constructor for the Customer class.
        /// </summary>
        /// <param name="firstName">Customer name.</param>
        /// <param name="lastName">Customer surname.</param>
        /// <param name="email">Customer email.</param>
        /// <param name="phoneNumber">Customer phone number.</param>
        protected Customer(string firstName, string lastName, string email, string phoneNumber)
        {
            PartitionKey = CalculatePartitionKey(firstName, lastName);
            RowKey = $"{Guid.NewGuid()}";
            FirstName = firstName;
            LastName = lastName;
            Email = email;
            PhoneNumber = phoneNumber;
        }

        /// <summary>
        /// Creates a new instance of the Customer class.
        /// </summary>
        /// <param name="firstName">Customer name.</param>
        /// <param name="lastName">Customer surname.</param>
        /// <param name="email">Customer email.</param>
        /// <param name="phoneNumber">Customer phone number.</param>
        /// <returns></returns>
        public static Customer Create(string firstName, string lastName, string email, string phoneNumber)
        {
            return new Customer(firstName, lastName, email, phoneNumber);
        }

        /// <summary>
        /// Calculates the partition key based on the first name and last name.
        /// </summary>
        /// <param name="firstName">The customer first name.</param>
        /// <param name="lastName">The customer last name.</param>
        /// <returns></returns>
        private static string CalculatePartitionKey(string firstName, string lastName)
        {
            // Example logic to generate a partition key based on firstName and lastName  
            return $"{lastName?.ToUpperInvariant()}_{firstName?.ToUpperInvariant()}";
        }
    }
}
