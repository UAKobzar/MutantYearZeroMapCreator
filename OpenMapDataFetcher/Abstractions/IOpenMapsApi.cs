using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using MYZMC.Entities.OpenData;

namespace MYZMC.OpenMapDataFetcher.Abstractions
{
    public interface IOpenMapsApi
    {
        Task<Osm> GetDataByBoundaries(double minLat, double minLon, double maxLat, double maxLon);
    }
}
