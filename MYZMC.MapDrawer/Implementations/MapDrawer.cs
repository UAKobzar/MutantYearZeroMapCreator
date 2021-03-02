using MYZMC.Entities.OpenData;
using MYZMC.MapDrawer.Abstractions;
using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;
using System.Linq;
using System.Numerics;
using System.IO;

namespace MYZMC.MapDrawer.Implementations
{
    public class MapDrawer : IMapDrawer
    {
        private const double meterToFeet = 3.2808399;
        private const long pixelsInFeet = 10;

        private const string boundaryMode = "boundary";
        private const string fillMode = "fill";

        private const string textureFolder = @"D:\OwnProjects\MutantYearZeroMapCreator\MYZMC.MapDrawer\brushes";

        private readonly List<(List<string> tags, Bitmap file)> _textures;


        public MapDrawer()
        {
            _textures = GetTextures();
        }

        private List<(List<string> tags, Bitmap file)> GetTextures()
        {
            var result = new List<(List<string> tags, Bitmap file)>();
            DirectoryInfo d = new DirectoryInfo(textureFolder);//Assuming Test is your Folder
            FileInfo[] Files = d.GetFiles(); //Getting Text files

            foreach (FileInfo file in Files)
            {
                var tags = file.Name.Split('.', '_');
                var bitmap = new Bitmap(file.FullName);

                result.Add((tags.ToList(), bitmap));
            }

            return result;
        }

        public void DrawMap(Osm data, Stream outputStream)
        {
            var size = GetImageSize(data);
            var bitmap = new Bitmap(size.Width, size.Height);
            var graphics = Graphics.FromImage(bitmap);

            DrawDataOnImage(data, ref graphics);

            bitmap.Save(outputStream, System.Drawing.Imaging.ImageFormat.Png);
        }

        private (double minLat, double minLon, double maxLat, double maxLon) GetBoundaries(Osm data)
        {
            var minLat = data.Node.Min(d => d.Lat);
            var minLon = data.Node.Min(d => d.Lon);
            var maxLat = data.Node.Max(d => d.Lat);
            var maxLon = data.Node.Max(d => d.Lon);

            return (minLat, minLon, maxLat, maxLon);
        }
        private Size GetImageSize(Osm data)
        {
            var boundaries = GetBoundaries(data);
            return GetImageSize(boundaries.minLat, boundaries.minLon, boundaries.maxLat, boundaries.maxLon);
        }
        private Size GetImageSize(double minLat, double minLon, double maxLat, double maxLon)
        {
            var distanceMetersWidth = CalculateDistance(maxLat, minLon, maxLat, maxLon);
            var distanceMetersHeight = CalculateDistance(maxLat, maxLon, minLat, maxLon);

            int width = DistanceToPixels(distanceMetersWidth) + 50;
            int height = DistanceToPixels(distanceMetersHeight) + 50;

            return new Size(width, height);
        }

        private int DistanceToPixels(double distance)
        {
            return (int)(distance * meterToFeet * pixelsInFeet);
        }

        private double CalculateDistance(double lat1, double lon1, double lat2, double lon2)
        {
            var d1 = lat1 * (Math.PI / 180.0);
            var num1 = lon1 * (Math.PI / 180.0);
            var d2 = lat2 * (Math.PI / 180.0);
            var num2 = lon2 * (Math.PI / 180.0) - num1;
            var d3 = Math.Pow(Math.Sin((d2 - d1) / 2.0), 2.0) +
                     Math.Cos(d1) * Math.Cos(d2) * Math.Pow(Math.Sin(num2 / 2.0), 2.0);
            return 6376500.0 * (2.0 * Math.Atan2(Math.Sqrt(d3), Math.Sqrt(1.0 - d3)));
        }

        private void DrawDataOnImage(Osm data, ref Graphics image)
        {
            DrawGround(image);
            var boundaries = GetBoundaries(data);

            foreach (var way in data.Way)
            {
                var width = 100;
                var wayNodes = data.Node.Where(n => way.Nd.Any(r => r.Ref == n.Id)).ToList();

                var boundaryBrush = GetBrush(way, boundaryMode);

                boundaryBrush.ScaleTransform(width / boundaryBrush.Image.Height, width / boundaryBrush.Image.Height);

                var fillBrush = GetBrush(way, fillMode);

                if (boundaryBrush == null && fillBrush == null)
                    continue;

                List<Point> points = new List<Point>();

                for (int i = 0; i < way.Nd.Count - 1; i++)
                {
                    var start = wayNodes.FirstOrDefault(n => n.Id == way.Nd[i].Ref);
                    var end = wayNodes.FirstOrDefault(n => n.Id == way.Nd[i + 1].Ref);
                    if (start == null || end == null)
                        continue;

                    var startPoint = new Point(DistanceToPixels(CalculateDistance(start.Lat, boundaries.minLon, start.Lat, start.Lon)), DistanceToPixels(CalculateDistance(boundaries.maxLat, start.Lon, start.Lat, start.Lon)));
                    var endPoint = new Point(DistanceToPixels(CalculateDistance(end.Lat, boundaries.minLon, end.Lat, end.Lon)), DistanceToPixels(CalculateDistance(boundaries.maxLat, end.Lon, end.Lat, end.Lon)));

                    points.Add(startPoint);

                    var angle = GetAngle(startPoint, endPoint);

                    if (boundaryBrush != null)
                    {
                        boundaryBrush.RotateTransform(angle);

                        image.FillPolygon(boundaryBrush, GetBoundaryPoints(startPoint, endPoint , width));

                        boundaryBrush.RotateTransform(-angle);
                    }

                }

                if(fillBrush != null && points.Count > 2)
                {
                    image.FillPolygon(fillBrush, points.ToArray());
                }
            }
        }

        private float GetAngle(Point startPoint, Point endPoint)
        {
            var diffPoint = new Point(startPoint.X - endPoint.X, startPoint.Y - endPoint.Y);
            var tan = (double)diffPoint.Y / diffPoint.X;

            var angle = Math.Atan(tan) / (Math.PI / 180);

            return (float)angle;
        }

        private Point[] GetBoundaryPoints(Point startPoint, Point endPoint, int width)
        {
            var angle = GetAngle(startPoint, endPoint);

            var points = GetWidthPoints(startPoint, width, angle).ToList();
            points.AddRange(GetWidthPoints(endPoint, width, angle));

            return points.ToArray();
        }

        private Point[] GetWidthPoints(Point middlePoint, int width, float angle)
        {
            var perpAngle = angle + 90;

            var radAngle = perpAngle * (Math.PI / 180);

            var k = Math.Tan(radAngle);
            var b = middlePoint.Y - k * middlePoint.X;

            var x1 = width / (2 * k);
            var x2 = 2 * middlePoint.X - x1;

            var y1 = k * x1 + b;
            var y2 = k * x2 + b;

            return new Point[] { new Point((int)x1, (int)y1), new Point((int)x2, (int)y2) };
        }

        private void DrawGround(Graphics image)
        {
            var trafaret = new Bitmap(@"D:\OwnProjects\MutantYearZeroMapCreator\MYZMC.MapDrawer\brushes\ground._texture.fill.jpg");
            var brush = new TextureBrush(trafaret);
            brush.ScaleTransform(0.1F, 0.1F);

            image.FillRegion(brush, image.Clip);
        }

        private TextureBrush GetBrush(Way way, string mode)
        {
            var tags = way.Tag.SelectMany(t => new[] { t.K, t.V }).ToList();

            var perTagsCount = _textures.Where(t => t.tags.Contains(mode)).Select(t => new { count = t.tags.Count(tg => tags.Contains(tg)), t.file }).ToList();

            var trafaret = perTagsCount.OrderByDescending(t => t.count).First().file;
            var brush = new TextureBrush(trafaret);

            return brush;
        }
    }
}
