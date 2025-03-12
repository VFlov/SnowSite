using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SnowSite.Server.Models;
using Npgsql;

namespace SnowSite.Server.Controllers
{

    [Route("api/[controller]")]
    [ApiController]
    public class MapController : Controller
    {
        private const string ConnectionString = "Host=45.130.214.139;Username=postgres;Password=dfcz333;Database=postgres";
        [HttpGet("gethouses")]
        public IActionResult GetHouses(int start = 0, int count = 100)
        {
            var houses = new List<HouseData>();

            using (var connection = new NpgsqlConnection(ConnectionString))
            {
                connection.Open();

                var query = @"
                    SELECT 
                        ih.ihs_ee_class, 
                        hp.hpl_geo_latitude, 
                        hp.hpl_geo_longitude
                    FROM 
                        imported_house ih
                    JOIN 
                        house_placement hp 
                    ON 
                        ih.ihs_house_guid = hp.ihs_house_guid
                    WHERE 
                        ih.ihs_ee_class IS NOT NULL 
                        AND ih.ihs_ee_class <> 'Нет'
                        AND ih.ihs_ee_class <> '-'
                        AND hp.hpl_geo_latitude IS NOT NULL
                        AND hp.hpl_geo_longitude IS NOT NULL
                    ";

                using (var command = new NpgsqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@Offset", start);
                    command.Parameters.AddWithValue("@Limit", count);

                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            houses.Add(new HouseData
                            {
                                IhsEeClass = reader.GetString(0),
                                HplGeoLatitude = reader.GetDouble(1),
                                HplGeoLongitude = reader.GetDouble(2)
                            });
                        }
                    }
                }
            }

            return Ok(houses);
        }
        [HttpGet("housescount")]
        public IActionResult HousesCount()
        {
            using (var connection = new NpgsqlConnection(ConnectionString))
            {
                connection.Open();

                var query = @"
                SELECT 
                    COUNT(*) AS house_count
                FROM 
                    imported_house ih
                JOIN 
                    house_placement hp 
                ON 
                    ih.ihs_house_guid = hp.ihs_house_guid
                WHERE 
                    ih.ihs_ee_class IS NOT NULL 
                    AND ih.ihs_ee_class <> 'Нет'
                    AND ih.ihs_ee_class <> '-'
                    AND hp.hpl_geo_latitude IS NOT NULL
                    AND hp.hpl_geo_longitude IS NOT NULL
                ";

                using (var command = new NpgsqlCommand(query, connection))
                {
                    using (var reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            var houseCount = reader.GetInt64(0);
                            return Ok(houseCount);
                        }
                        else
                        {
                            return Ok(0);
                        }
                    }
                }
            }

        }
    }


}

