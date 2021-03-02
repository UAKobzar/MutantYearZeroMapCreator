using System;
using MYZMC.OpenMapDataFetcher.Implementations;
using MYZMC.MapDrawer.Implementations;
using System.IO;

namespace MutantYearZeroMapCreator
{
    class Program
    {
        static void Main(string[] args)
        {
            var api = new OpenMapsApi(new System.Net.Http.HttpClient());

            var task = api.GetDataByBoundaries(51.492553, -0.115864, 51.495239, -0.112771);

            task.Wait();

            var result = task.Result;

            var drawer = new MapDrawer();

            using(var fs = new FileStream(@"D:\tmp\MYZMAP.png",FileMode.OpenOrCreate))
            {
                drawer.DrawMap(result, fs);
            }

            Console.WriteLine("OK");
            Console.Read();
        }
    }
}
