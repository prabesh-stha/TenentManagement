using Microsoft.Data.SqlClient;

namespace TenentManagement.Services.Database
{
    public class DatabaseConnection(IConfiguration configuration)
    {
        private readonly IConfiguration _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));

        public SqlConnection GetConnection()
        {
            string? connectionString = _configuration.GetConnectionString("DefaultConnection");
            if (string.IsNullOrEmpty(connectionString))
            {
                throw new InvalidOperationException("Connection string 'DefaultConnection' is not configured.");
            }
            return new SqlConnection(connectionString);
        }

    }
}
