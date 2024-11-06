using Dapper;
using StatusReport.Connections;
using StatusReport.Models.Dto;

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
        public async Task<IEnumerable<SpecificationNeededStatuses>> GetClearedReport(string jsonList, bool isSearchByBarcode)
        {
            using (var connection = _serverConnection.GetDbConnection())
            {

                string sql = @"
                        WITH
                            Ship AS (
	                            select 
		                            s.id
		                            ,s.ExternalNumber
		                            ,s.Barcodes
		                            ,clu.Description as Status
		                            ,s.StatusTime
	                            from shipment s ";

                if (isSearchByBarcode)
                {
                    sql += @" INNER JOIN (SELECT [value] FROM OPENJSON(@JsonList)) as jsons ON jsons.[value] = s.Barcodes";
                }
                else
                {
                    sql += @" INNER JOIN (SELECT [value] FROM OPENJSON(@JsonList)) as jsons ON jsons.[value] = s.ExternalNumber ";
                }

                sql += @"
                        INNER JOIN CodeLookUp clu on clu.id = s.StatusCodeId
                            ),

                            NaCar AS ( SELECT * FROM (
                                SELECT
		                            s.id
                                    ,se.EventDate
                                    ,ROW_NUMBER() OVER (PARTITION BY ShipmentId ORDER BY EventDate) AS RowNum
                                FROM
                                    ShipmentEvent se
                                INNER JOIN Ship s ON  s.id = se.ShipmentId 
                                WHERE se.StatusCodeId = 1243                --Ocarninjeno
                            )a
                            WHERE RowNum = 1
                            ),

                            Ocarinjeno AS ( SELECT * FROM (
                                SELECT
		                            s.id
                                    ,se.EventDate
                                    ,ROW_NUMBER() OVER (PARTITION BY ShipmentId ORDER BY EventDate) AS RowNum
                                FROM
                                    ShipmentEvent se
                                INNER JOIN Ship s ON  s.id = se.ShipmentId 
                                WHERE se.StatusCodeId = 1244                --Ocarninjeno
                            )a
                            WHERE RowNum = 1
                            ),

                            PredatoKuriru AS ( SELECT * FROM (
                                SELECT
                                    s.id
                                    ,se.EventDate
                                    ,ROW_NUMBER() OVER (PARTITION BY ShipmentId ORDER BY EventDate) AS RowNum
                                FROM
                                    ShipmentEvent se
                                INNER JOIN Ship s ON  s.id = se.ShipmentId 
                                WHERE se.StatusCodeId = 1254                --Predato Kuriru
                            )a
                            WHERE RowNum = 1
                            ),

                            UDisCentr AS ( SELECT * FROM (
                                SELECT
                                    s.id
                                    ,se.EventDate
                                    ,ROW_NUMBER() OVER (PARTITION BY ShipmentId ORDER BY EventDate) AS RowNum
                                FROM
                                    ShipmentEvent se
                                INNER JOIN Ship s ON  s.id = se.ShipmentId 
                                WHERE se.StatusCodeId = 15					--U Distributivnom Centru
                            )a
                            WHERE RowNum = 1
                            ),

                            StatusKurir AS ( SELECT * FROM (
                                SELECT
                                    s.id
                                    ,se.EventDate
		                            ,clu.description as status
                                    ,ROW_NUMBER() OVER (PARTITION BY ShipmentId ORDER BY EventDate) AS RowNum
                                FROM
                                    ShipmentEvent se
                                INNER JOIN Ship s ON  s.id = se.ShipmentId 
	                            join CodeLookUp clu on se.StatusCodeId = clu.id
                                WHERE se.StatusCodeId in (1261,1255,1258,16)--Statusi kurira
                            )a
                            WHERE RowNum = 1
                            )

                            SELECT 
                            s.id AS ShipmentId,
                            s.ExternalNumber,
                            s.Barcodes,
                            s.Status AS PoslednjiStatus,
                            s.StatusTime AS DatumPoslednjegStatusa,

                            nc.EventDate AS DatumNaCarinjenju,
                            o.EventDate AS DatumOcarinjeno,
                            pk.EventDate AS DatumPredatoKuriru,
                            dc.EventDate AS DatumUDisCentr,

                            sk.status as StatusKurira,
                            sk.EventDate AS DatumStatusKurir

                            from Ship s
                            left join NaCar nc on s.id = nc.id
                            left join Ocarinjeno o on s.id = o.id
                            left join PredatoKuriru pk on s.id = pk.id
                            left join UDisCentr dc on s.id = dc.id
                            left join StatusKurir sk on s.id = sk.id
                        ";

                var parameters = new { JsonList = jsonList };
                var result = await connection.QueryAsync<SpecificationNeededStatuses>(sql, parameters, commandTimeout: 420);
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
		                            se.ExternalNumber,
		                            clu.Description as status,
		                            se.EventDate,
                                    CAST(se.EventDate AS DATE) as EventDateOnly,
                                    ROW_NUMBER() OVER (PARTITION BY ShipmentId ORDER BY EventDate desc) AS RowNum
                                FROM ShipmentEvent se
	                            INNER JOIN CodeLookUp clu on clu.id = se.StatusCodeId
                                INNER JOIN (SELECT [value] FROM OPENJSON(@JsonList)) as jsons ON jsons.[value] = se.ExternalNumber ";
                sql += @" 
                        ) SELECT
                            ExternalNumber, 
                            Status, 
                            EventDate, 
                            EventDateOnly
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
                                s.Weight,
                                s.GoodsValue 
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

        public async Task<IEnumerable<SpecificationNeededStatuses>> GetLMCStatuses(string jsonList, bool isSearchByBarcode)
        {
            using (var connection = _serverConnection.GetDbConnection())
            {

                string sql = @"
                        WITH
                            Ship AS (
	                            select 
		                            s.id
		                            ,s.ExternalNumber
		                            ,s.Barcodes
		                            ,clu.Description as Status
		                            ,s.StatusTime
	                            from shipment s ";

                if (isSearchByBarcode)
                {
                    sql += @" INNER JOIN (SELECT [value] FROM OPENJSON(@JsonList)) as jsons ON jsons.[value] = s.Barcodes";
                }
                else
                {
                    sql += @" INNER JOIN (SELECT [value] FROM OPENJSON(@JsonList)) as jsons ON jsons.[value] = s.ExternalNumber ";
                }

                sql += @"
                        INNER JOIN CodeLookUp clu on clu.id = s.StatusCodeId
                            ),

                            NaCar AS ( SELECT * FROM (
                                SELECT
		                            s.id
                                    ,se.EventDate
                                    ,ROW_NUMBER() OVER (PARTITION BY ShipmentId ORDER BY EventDate) AS RowNum
                                FROM
                                    ShipmentEvent se
                                INNER JOIN Ship s ON  s.id = se.ShipmentId 
                                WHERE se.StatusCodeId = 1243                --Ocarninjeno
                            )a
                            WHERE RowNum = 1
                            ),

                            Ocarinjeno AS ( SELECT * FROM (
                                SELECT
		                            s.id
                                    ,se.EventDate
                                    ,ROW_NUMBER() OVER (PARTITION BY ShipmentId ORDER BY EventDate) AS RowNum
                                FROM
                                    ShipmentEvent se
                                INNER JOIN Ship s ON  s.id = se.ShipmentId 
                                WHERE se.StatusCodeId = 1244                --Ocarninjeno
                            )a
                            WHERE RowNum = 1
                            ),

                            PredatoKuriru AS ( SELECT * FROM (
                                SELECT
                                    s.id
                                    ,se.EventDate
                                    ,ROW_NUMBER() OVER (PARTITION BY ShipmentId ORDER BY EventDate) AS RowNum
                                FROM
                                    ShipmentEvent se
                                INNER JOIN Ship s ON  s.id = se.ShipmentId 
                                WHERE se.StatusCodeId = 1254                --Predato Kuriru
                            )a
                            WHERE RowNum = 1
                            ),

                            UDisCentr AS ( SELECT * FROM (
                                SELECT
                                    s.id
                                    ,se.EventDate
                                    ,ROW_NUMBER() OVER (PARTITION BY ShipmentId ORDER BY EventDate) AS RowNum
                                FROM
                                    ShipmentEvent se
                                INNER JOIN Ship s ON  s.id = se.ShipmentId 
                                WHERE se.StatusCodeId = 15					--U Distributivnom Centru
                            )a
                            WHERE RowNum = 1
                            ),

                            StatusKurir AS ( SELECT * FROM (
                                SELECT
                                    s.id
                                    ,se.EventDate
		                            ,clu.description as status
                                    ,ROW_NUMBER() OVER (PARTITION BY ShipmentId ORDER BY EventDate) AS RowNum
                                FROM
                                    ShipmentEvent se
                                INNER JOIN Ship s ON  s.id = se.ShipmentId 
	                            join CodeLookUp clu on se.StatusCodeId = clu.id
                                WHERE se.StatusCodeId in (1261,1255,1258,16)--Statusi kurira
                            )a
                            WHERE RowNum = 1
                            )

                            SELECT 
                            s.id AS ShipmentId,
                            s.ExternalNumber,
                            s.Barcodes,
                            s.Status AS PoslednjiStatus,
                            s.StatusTime AS DatumPoslednjegStatusa,

                            nc.EventDate AS DatumNaCarinjenju,
                            o.EventDate AS DatumOcarinjeno,
                            pk.EventDate AS DatumPredatoKuriru,
                            dc.EventDate AS DatumUDisCentr,

                            sk.status as StatusKurira,
                            sk.EventDate AS DatumStatusKurir

                            from Ship s
                            left join NaCar nc on s.id = nc.id
                            left join Ocarinjeno o on s.id = o.id
                            left join PredatoKuriru pk on s.id = pk.id
                            left join UDisCentr dc on s.id = dc.id
                            left join StatusKurir sk on s.id = sk.id
                        ";

                var parameters = new { JsonList = jsonList };
                var result = await connection.QueryAsync<SpecificationNeededStatuses>(sql, parameters, commandTimeout: 420);
                return result.ToList();
            }
        }
    }
}

