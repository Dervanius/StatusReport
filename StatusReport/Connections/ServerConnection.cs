using System.Data.SqlClient;

namespace StatusReport.Connections
{
    public class ServerConnection : DbConnection
    {
        public SqlConnection GetDbConnection() 
        {
            return GetServerConnection();
        }
    }
}
