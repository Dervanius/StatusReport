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

        public async Task<IEnumerable<dynamic>> GetReportByBarcodesAsync(string jsonList)
        {
            using (var connection = _serverConnection.GetDbConnection())
            {
                string sql = @"
        WITH LatestEvents AS (
            SELECT
                s.Barcodes,
                e.StatusCodeId,
                l.Description,
                e.Id AS EventId,
                e.EventDate
            FROM
                ShipmentEvent e
            INNER JOIN
                Shipment s ON e.ShipmentId = s.Id
            INNER JOIN
                CodeLookUp l ON e.StatusCodeId = l.Id
            INNER JOIN
                (SELECT JSON_VALUE([value], '$.Barcodes') AS Barcodes
                 FROM OPENJSON(@JsonList)) AS jsons
                ON jsons.Barcodes = s.Barcodes
        ),
        RankedEvents AS (
            SELECT
                Barcodes,
                StatusCodeId,
                Description,
                EventId,
                EventDate,
                ROW_NUMBER() OVER (PARTITION BY Barcodes ORDER BY EventId DESC) AS rn
            FROM
                LatestEvents
        )
        SELECT
            Barcodes,
            EventId,
            EventDate,
            StatusCodeId,
            Description
        FROM
            RankedEvents
        WHERE
            rn = 1
        ORDER BY
            Barcodes;";

                var parameters = new { JsonList = jsonList };
                return await connection.QueryAsync(sql, parameters);
            }
        }

        public async Task<IEnumerable<dynamic>> GetReportByExternalNumbersAsync(string jsonList)
        {
            using (var connection = _serverConnection.GetDbConnection())
            {
                string sql = @"
        WITH RankedEvents AS (
            SELECT
                s.ExternalNumber,
                s.Id,
                m.awb,
                s.Barcodes,
                clu.Description as Status,
                se.EventDate,
                ROW_NUMBER() OVER (PARTITION BY s.ShipmentId ORDER BY se.EventDate desc) AS RowNum,
                clu.DisplayOrder,
                s.Weight
            FROM
                ShipmentEvent se
            INNER JOIN
                Shipment s ON s.Id = se.ShipmentId
            INNER JOIN
                Manifest m ON m.Id = s.ManifestId
            INNER JOIN
                CodeLookUp clu ON clu.Id = se.StatusCodeId
            INNER JOIN
                (SELECT [value] FROM OPENJSON(@JsonList)) as jsons ON jsons.[value] = s.ExternalNumber
            WHERE
                se.StatusCodeId = 1244 -- Ocarinjeno status
        )
        SELECT
            Barcodes,
            ExternalNumber,
            Status,
            EventDate,
            Weight,
            awb
        FROM
            RankedEvents
        WHERE
            RowNum = 1
        ORDER BY
            Barcodes;";

                var parameters = new { JsonList = jsonList };
                return await connection.QueryAsync(sql, parameters);
            }
        }




    }




}
