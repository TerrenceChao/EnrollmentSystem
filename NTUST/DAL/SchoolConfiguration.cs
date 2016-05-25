using System.Data.Entity;
using System.Data.Entity.SqlServer;
using System.Data.Entity.Infrastructure.Interception;

namespace NTUST.DAL
{
    public class SchoolConfiguration : DbConfiguration
    {
        public SchoolConfiguration()
        {
            SetExecutionStrategy("System.Data.SqlClient", () => new SqlAzureExecutionStrategy());

            //Catch DB log information
            DbInterception.Add(new SchoolInterceptorTransientErrors());
            DbInterception.Add(new SchoolInterceptorLogging());
        }
    }
}