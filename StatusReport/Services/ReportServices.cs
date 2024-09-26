using Dapper;
using StatusReport.Connections;
using System.Data;
using System.Threading.Tasks;

namespace StatusReport.Services
{
    public class ReportServices
    {
        private readonly ServerConnection _serverConnection;

        public ReportServices()
        {
            _serverConnection = new ServerConnection();
        }

              
        //GetClearedReport
        public async Task<IEnumerable<dynamic>> GetClearedReport(string jsonList, bool isSearchByBarcode)
        {
            using (var connection = _serverConnection.GetDbConnection())
            {

                string sql = @"WITH RankedEvents AS (
                            SELECT
		                    s.ExternalNumber,
		                    s.Id,
		                    m.awb,
		                    m.Nalog,
                            s.Barcodes,
		                    clu.Description as status,
		                    EventDate,
                            ROW_NUMBER() OVER (PARTITION BY ShipmentId ORDER BY EventDate desc) AS RowNum,
		                    clu.DisplayOrder,
		                    s.Weight
                            FROM ShipmentEvent se
	                        INNER JOIN Shipment s on s.id = se.ShipmentId
	                        INNER JOIN Manifest m on m.id = s.manifestid
	                        INNER JOIN CodeLookUp clu on clu.id = se.StatusCodeId ";

                if (isSearchByBarcode)
                {
                    sql += @" INNER JOIN (SELECT [value] FROM OPENJSON(@JsonList)) as jsons ON jsons.[value] = s.Barcodes";
                }
                else 
                {
                    sql += @" INNER JOIN (SELECT [value] FROM OPENJSON(@JsonList)) as jsons ON jsons.[value] = s.ExternalNumber ";
                }

                sql += @" WHERE se.StatusCodeId = 1244 ) 
                            SELECT 
                            AWB, 
                            Nalog, 
                            Barcodes, 
                            ExternalNumber, 
                            Status, 
                            EventDate, 
                            Weight 
                            FROM RankedEvents
                            WHERE
                            RowNum = 1
	                        ORDER BY Barcodes";

                var parameters = new { JsonList = jsonList };
                var result = await connection.QueryAsync(sql, parameters, commandTimeout:300);
                return result.ToList();
            }

        }

        //GetLastReport
        public async Task<IEnumerable<dynamic>> GetLastReport(string jsonList, bool isSearchByBarcode)
        {
            using (var connection = _serverConnection.GetDbConnection())
            {

                string sql = @"WITH RankedEvents AS (
                                SELECT
		                        m.AWB,
		                        m.Nalog,
                                Barcodes,
		                        s.ExternalNumber,
		                        clu.Description as status
		                        ,EventDate,
		                        s.Weight,
                                ROW_NUMBER() OVER (PARTITION BY ShipmentId ORDER BY EventDate desc) AS RowNum,
		                        clu.DisplayOrder
                                FROM ShipmentEvent se
	                            INNER JOIN Shipment s on s.id = se.ShipmentId
	                            INNER JOIN CodeLookUp clu on clu.id = s.StatusCodeId";

                if (isSearchByBarcode)
                {
                    sql += @" INNER JOIN (SELECT [value] FROM OPENJSON(@JsonList)) as jsons ON jsons.[value] = s.Barcodes ";
                }
                else 
                {
                    sql += @" INNER JOIN (SELECT [value] FROM OPENJSON(@JsonList)) as jsons ON jsons.[value] = s.ExternalNumber ";
                }

                

	            sql += @" INNER JOIN Manifest m on m.id = s.manifestid
                        )
                        SELECT
	                    AWB,
	                    Nalog,
                        Barcodes,
	                    ExternalNumber,
	                    Status,
	                    EventDate,
	                    Weight
                        FROM RankedEvents
                        WHERE RowNum = 1
                        ORDER BY EventDate ";

                var parameters = new { JsonList = jsonList };
                var result = await connection.QueryAsync(sql, parameters, commandTimeout: 300);
                return result.ToList();
            }
        }

        //Get Temp
        public async Task<IEnumerable<dynamic>> GetTemporaryReport(string jsonList, bool isSearchByBarcode)
        {
            using (var connection = _serverConnection.GetDbConnection())
            {

                string sql = @"SELECT 
                            m.AWB,
                            m.Nalog,
                            s.Barcodes,
                            s.ExternalNumber,
                            c.Description as Status, 
                            s.CreatedOn as EventDate,
                            s.Weight 
                            FROM Shipment s
                            INNER JOIN CodeLookUp c ON s.StatusCodeId = c.Id";
                           
                if (isSearchByBarcode)
                {
                    sql += @" INNER JOIN (SELECT [value] FROM OPENJSON(@JsonList)) as jsons ON jsons.[value] = s.Barcodes ";
                }
                else
                {
                    sql += @" INNER JOIN (SELECT [value] FROM OPENJSON(@JsonList)) as jsons ON jsons.[value] = s.ExternalNumber ";
                }



                sql += @" INNER JOIN Manifest m on m.id = s.manifestid";


                var parameters = new { JsonList = jsonList };
                var result = await connection.QueryAsync(sql, parameters, commandTimeout: 300);
                return result.ToList();
            }
        }

    }
}

