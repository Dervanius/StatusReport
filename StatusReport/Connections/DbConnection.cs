using System.Data.SqlClient;

namespace StatusReport.Connections
{
    public class DbConnection
    {
        private static string _serverConnection = "Data Source=transferareplica.database.windows.net;Initial Catalog=olb-prod;User ID=olbuser;Password=1.Cycle.789;Connect Timeout=30;Encrypt=False;TrustServerCertificate=False;ApplicationIntent=ReadOnly;MultiSubnetFailover=False";

        public SqlConnection GetServerConnection() 
        {
            return new SqlConnection(_serverConnection);
        }
    }
}
