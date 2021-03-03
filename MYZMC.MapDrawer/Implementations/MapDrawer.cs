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

        private const string textureFolder = @".\brushes";

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
                var tags = file.Name.Replace(file.Extension, "").Split('_');
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
                var width = 50;
                var wayNodes = data.Node.Where(n => way.Nd.Any(r => r.Ref == n.Id)).ToList();

                var boundaryBrush = GetBrush(way, boundaryMode);

                boundaryBrush?.ScaleTransform((float)width / boundaryBrush.Image.Height, (float)width / boundaryBrush.Image.Height);

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

                        image.FillPolygon(boundaryBrush, GetBoundaryPoints(startPoint, endPoint, width));

                        boundaryBrush.RotateTransform(-angle);
                    }

                }

                if (fillBrush != null && points.Count > 2)
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
            points.AddRange(GetWidthPoints(endPoint, width, angle).Reverse());

            return points.ToArray();
        }

        private Point[] GetWidthPoints(Point middlePoint, int width, float angle)
        {
            width /= 2;
            var perpAngle = angle + 90;

            var radPerpAngle = perpAngle * (Math.PI / 180);

            var k = Math.Tan(radPerpAngle);
            var b = middlePoint.Y - k * middlePoint.X;

            var radAngle = angle * (Math.PI / 180);

            var dx = Math.Sin(radAngle) * width;

            var x1 = middlePoint.X - dx;
            var x2 = middlePoint.X + dx;

            var y1 = k * x1 + b;
            var y2 = k * x2 + b;

            return new Point[] { new Point((int)x1, (int)y1), new Point((int)x2, (int)y2) };
        }

        private void DrawGround(Graphics image)
        {
            var trafaret = new Bitmap(@".\brushes\ground._texture.fill.jpg");
            var brush = new TextureBrush(trafaret);
            brush.ScaleTransform(0.1F, 0.1F);

            image.FillRegion(brush, image.Clip);
        }

        private TextureBrush GetBrush(Way way, string mode)
        {
            var bestTextureIndex = -1;
            var bestTextureValue = 0;

            var textures = _textures.Where(t => t.tags.Contains("texture." + mode)).ToList();

            for (int i = 0; i < textures.Count; i++)
            {
                var texture = textures[i];
                var textureVal = 0;
                foreach (var tag in way.Tag)
                {
                    if (texture.tags.Any(t => t.StartsWith(tag.K)))
                    {
                        textureVal += 1;
                    }
                    if (texture.tags.Any(t => t.StartsWith(tag.K + '.' + tag.V)))
                    {
                        textureVal += 2;
                    }
                }

                if(textureVal > bestTextureValue)
                {
                    bestTextureValue = textureVal;
                    bestTextureIndex = i;
                }
            }

            if (bestTextureIndex != -1)
            {
                var trafaret = textures[bestTextureIndex].file;
                var brush = new TextureBrush(trafaret);

                return brush;
            }
            else
            {
                return null;
            }
        }
    }
}
