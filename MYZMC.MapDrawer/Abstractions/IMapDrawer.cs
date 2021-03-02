using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using MYZMC.Entities.OpenData;

namespace MYZMC.MapDrawer.Abstractions
{
    public interface IMapDrawer
    {
        void DrawMap(Osm data, Stream outputStream);
    }
}
