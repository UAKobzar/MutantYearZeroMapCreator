using MYZMC.Entities.OpenData;
using MYZMC.OpenMapDataFetcher.Abstractions;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace MYZMC.OpenMapDataFetcher.Implementations
{
    public class OpenMapsApi : IOpenMapsApi
    {
        private readonly HttpClient _httpClient;
        private const string baseUrl = "https://overpass.openstreetmap.ru/api/interpreter";

        public OpenMapsApi(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }
        public async Task<Osm> GetDataByBoundaries(double minLat, double minLon, double maxLat, double maxLon)
        {
            var request = $"(node({minLat}, {minLon}, {maxLat},{maxLon});<;);out;";

            var data = new Dictionary<string, string>();

            data["data"] = request;
            var content = new FormUrlEncodedContent(data);

            var response = await _httpClient.PostAsync(baseUrl, content);

            var xml = await response.Content.ReadAsStringAsync();

            var result = Helpers.XMLHelper.ParseXML<Osm>(xml);

            return result;
        }
    }
}
