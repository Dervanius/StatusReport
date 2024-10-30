using Dapper;
using Newtonsoft.Json;
using StatusReport.Connections;
using StatusReport.Models.Dto;

namespace StatusReport.Services
{
    public class BarcodeServices
    {
        private readonly ServerConnection _serverConnection;

        public BarcodeServices()
        {
            _serverConnection = new ServerConnection();
        }

        public async Task<List<CourierBarcodeDto>> GetCourierBarcode(List<string> barcodes, bool isSearchByBarcode)
        {
            var shipmentCarrierDto = await GetShipmentIdAndLastMileBySearchInput(barcodes, isSearchByBarcode);
            var courierbarcodes = await GetCourierBarcode(shipmentCarrierDto);
            return courierbarcodes;
        }

        private async Task<List<ShipmentCarrierDto>> GetShipmentIdAndLastMileBySearchInput(List<string> barcodes, bool isSearchByBarcode)
        {
            var json = JsonConvert.SerializeObject(barcodes);

            using (var connection = _serverConnection.GetDbConnection())
            {
                string sql = @"
                                SELECT
		                            s.id as ShipmentId,
		                            clu.code as Carrier
                                FROM Shipment s
                                JOIN CodeLookUp clu on clu.Description = s.LastMileCarrier 
                            ";

                if (isSearchByBarcode)
                    sql += " INNER JOIN (SELECT [value] FROM OPENJSON(@json)) as jsons  ON jsons.[value] = s.Barcodes";
                else
                    sql += " INNER JOIN (SELECT [value] FROM OPENJSON(@json)) as jsons  ON jsons.[value] = s.ExternalNumber";

                var parameters = new { json };
                var result = (await connection.QueryAsync<ShipmentCarrierDto>(sql, parameters, commandTimeout: 300)).ToList();
                return result;
            }
        }

        public async Task<List<CourierBarcodeDto>> GetCourierBarcode(List<ShipmentCarrierDto> shipmentCarrierDto)
        {
            var json = JsonConvert.SerializeObject(shipmentCarrierDto);

            using (var connection = _serverConnection.GetDbConnection())
            {
                List<CourierBarcodeDto> barcodeDtos = new List<CourierBarcodeDto>();

                List<CourierBarcodeDto> cityBarcodes = await GetCityBarcodes(shipmentCarrierDto.Where(x=>x.Carrier == 479004).ToList());
                List<CourierBarcodeDto> aksBarcodes = await GetAksBarcodes(shipmentCarrierDto.Where(x => x.Carrier == 479003).ToList());

                barcodeDtos.AddRange(cityBarcodes);
                barcodeDtos.AddRange(aksBarcodes);

                return barcodeDtos.ToList();
            }
        }

        public async Task<List<CourierBarcodeDto>> GetCityBarcodes(List<ShipmentCarrierDto> shipmentCarrierDto)
        {
            if (shipmentCarrierDto.Count == 0)
                return new List<CourierBarcodeDto>();

            var json = JsonConvert.SerializeObject(shipmentCarrierDto.Select(x=>x.ShipmentId));

            using (var connection = _serverConnection.GetDbConnection())
            {
                string sql = @"
                    WITH RankedEvents AS (
                        SELECT  
                            s.lastMileCarrier as 'Kurir'
                            ,s.Barcodes as 'TsfBarcode'
                            ,no.StatusDesc as 'KurirBarcode'
                            ,ROW_NUMBER() OVER (PARTITION BY ShipmentId ORDER BY EventDate desc) AS RowNum  
                        FROM StatusLogCourierAPI_NoEventID no
                        INNER JOIN Shipment s on no.shipmentid = s.id
                        INNER JOIN (SELECT [value] FROM OPENJSON(@json)) as jsons ON jsons.[value] = s.id
                    )

                    select * from RankedEvents where RowNum = 1
                ";

                var parameters = new { json };
                var result = (await connection.QueryAsync<CourierBarcodeDto>(sql, parameters, commandTimeout: 500)).ToList();
                return result;
            }
        }

        public async Task<List<CourierBarcodeDto>> GetAksBarcodes(List<ShipmentCarrierDto> shipmentCarrierDto)
        {
            if (shipmentCarrierDto.Count == 0)
                return new List<CourierBarcodeDto>();

            var json = JsonConvert.SerializeObject(shipmentCarrierDto.Select(x => x.ShipmentId));

            using (var connection = _serverConnection.GetDbConnection())
            {
                string sql = @"
                    SELECT  
                        s.lastMileCarrier as 'Kurir'
                        ,s.Barcodes as 'TsfBarcode'
                        ,cr.Barcode as 'KurirBarcode'
                    FROM CarrierReference cr
                    INNER JOIN Shipment s on cr.shipmentid = s.id
                    INNER JOIN (SELECT [value] FROM OPENJSON(@json)) as jsons ON jsons.[value] = s.id
                ";

                var parameters = new { json };
                var result = (await connection.QueryAsync<CourierBarcodeDto>(sql, parameters, commandTimeout: 500)).ToList();
                return result;
            }
        }

    }
}

