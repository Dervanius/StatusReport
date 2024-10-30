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
                                s.LastMileCarrier,
                                s.Barcodes,
		                        clu.Description as status,
		                        se.EventDate,
                                CAST(se.EventDate AS DATE) as EventDateOnly,
                                s.CreatedOn,
                                CAST(s.CreatedOn AS DATE) as CreatedOnDateOnly,
                                ROW_NUMBER() OVER (PARTITION BY ShipmentId ORDER BY EventDate desc) AS RowNum,
		                        clu.DisplayOrder,
		                        s.Weight
                            FROM ShipmentEvent se
	                        INNER JOIN Shipment s on s.id = se.ShipmentId
	                        INNER JOIN CodeLookUp clu on clu.id = se.StatusCodeId 
                            INNER JOIN Manifest m on m.id = s.manifestid";

                if (isSearchByBarcode)
                {
                    sql += @" INNER JOIN (SELECT [value] FROM OPENJSON(@JsonList)) as jsons ON jsons.[value] = s.Barcodes";
                }
                else 
                {
                    sql += @" INNER JOIN (SELECT [value] FROM OPENJSON(@JsonList)) as jsons ON jsons.[value] = s.ExternalNumber ";
                }

                sql += @" WHERE se.StatusCodeId = 1244 
                        ) SELECT
                            AWB, 
                            Nalog, 
                            LastMileCarrier,
                            Barcodes, 
                            ExternalNumber, 
                            Status, 
                            EventDate, 
                            EventDateOnly,
                            CreatedOn,
                            CreatedOnDateOnly,
                            Weight 
                        FROM RankedEvents
                        WHERE RowNum = 1";

                var parameters = new { JsonList = jsonList };
                var result = await connection.QueryAsync(sql, parameters, commandTimeout:300);
                return result.ToList();
            }

        }

        //Get Temp
        public async Task<IEnumerable<dynamic>> GetTemporaryReport(string jsonList, bool isSearchByBarcode)
        {
            using (var connection = _serverConnection.GetDbConnection())
            {

                string sql = @"WITH RankedEvents AS (
                                SELECT
		                            s.ExternalNumber,
		                            s.Id,
		                            m.awb,
		                            m.Nalog,
                                    s.LastMileCarrier,
                                    s.Barcodes,
		                            clu.Description as status,
		                            se.EventDate,
                                    CAST(se.EventDate AS DATE) as EventDateOnly,
                                    s.CreatedOn,
                                    CAST(s.CreatedOn AS DATE) as CreatedOnDateOnly,
                                    ROW_NUMBER() OVER (PARTITION BY ShipmentId ORDER BY EventDate desc) AS RowNum,
		                            clu.DisplayOrder,
		                            s.Weight
                                FROM ShipmentEvent se
	                            INNER JOIN Shipment s on s.id = se.ShipmentId
	                            INNER JOIN CodeLookUp clu on clu.id = se.StatusCodeId
                                INNER JOIN Manifest m on m.id = s.manifestid";

                if (isSearchByBarcode)
                {
                    sql += @" INNER JOIN (SELECT [value] FROM OPENJSON(@JsonList)) as jsons ON jsons.[value] = s.Barcodes ";
                }
                else 
                {
                    sql += @" INNER JOIN (SELECT [value] FROM OPENJSON(@JsonList)) as jsons ON jsons.[value] = s.ExternalNumber ";
                }


	            sql += @" 
                        ) SELECT
                            AWB, 
                            Nalog, 
                            LastMileCarrier,
                            Barcodes, 
                            ExternalNumber, 
                            Status, 
                            EventDate, 
                            EventDateOnly,
                            CreatedOn,
                            CreatedOnDateOnly,
                            Weight 
                        FROM RankedEvents
                        WHERE RowNum = 1 ";

                var parameters = new { JsonList = jsonList };
                var result = await connection.QueryAsync(sql, parameters, commandTimeout: 300);
                return result.ToList();
            }
        }

        //GetLastReport
        public async Task<IEnumerable<dynamic>> GetLastReport(string jsonList, bool isSearchByBarcode)
        {
            using (var connection = _serverConnection.GetDbConnection())
            {

                string sql = @"SELECT 
                                m.AWB,
                                m.Nalog,
                                s.LastMileCarrier,
                                s.Barcodes,
                                s.ExternalNumber,
                                c.Description as Status, 
                                s.StatusTime as EventDate,
                                CAST(s.StatusTime AS DATE) as EventDateOnly,
                                s.CreatedOn as CreatedOn,
                                CAST(s.CreatedOn AS DATE) as CreatedOnDateOnly,
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

                sql += @" LEFT JOIN Manifest m on m.id = s.manifestid";

                var parameters = new { JsonList = jsonList };
                var result = await connection.QueryAsync(sql, parameters, commandTimeout: 300);
                return result.ToList();
            }
        }

    }
}

