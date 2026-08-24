using NetTopologySuite.Algorithm;
using NetTopologySuite.Features;
using NetTopologySuite.Geometries;
using NetTopologySuite.IO;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using System.Text;
using System.Windows.Media;
using System.Windows.Shapes;
using Brush = System.Drawing.Brush;
using Color = System.Drawing.Color;
using Geometry = NetTopologySuite.Geometries.Geometry;
using Pen = System.Drawing.Pen;
using Polygon = NetTopologySuite.Geometries.Polygon;

namespace GIS_Viewer_CSharp {

    internal class MapLoader {

        private readonly List<Action<Graphics>> actionList;
        private readonly GraphicsPath graphicsPath;

        public Bitmap BitMap {
            get; private set;
        }

        public ShapefileDataReader ShapeReader {
            get; private set;
        }

        public string FileName {
            get; private set;
        }

        public List<IFeature> FeatureList {
            get; private set;
        }

        public Envelope Envelope {
            get; private set;
        }

       
        public MapLoader(string filePath, int width, int height) {
            this.BitMap = new Bitmap(width, height);
            this.ShapeReader = new ShapefileDataReader(filePath, GeometryFactory.Default, Encoding.UTF8);
            this.FileName = filePath;
            this.FeatureList = [];
            this.actionList = [];
            this.Envelope = new Envelope();
            this.graphicsPath = new GraphicsPath();
            LoadFile();
        }

        private void LoadFile() {
            while (ShapeReader.Read()) {
                Geometry geometry = ShapeReader.Geometry;
                AttributesTable attributes = new AttributesTable();

                for (int i = 0; i < ShapeReader.DbaseHeader.NumFields; i++) {
                    string name = ShapeReader.DbaseHeader.Fields[i].Name;
                    object value = ShapeReader.GetValue(i + 1);

                    attributes.Add(name, value);
                }
                FeatureList.Add(new Feature(geometry, attributes));
            }

            foreach (var feature in FeatureList) {
                this.Envelope.ExpandToInclude(feature.Geometry.EnvelopeInternal);
            }
        }

        public MapLoader SetBackground(Color color, Func<IFeature, bool> filterFunc) {
            SolidBrush brush = new(color);
            actionList.Add(graphics => {
                foreach (var filter in FeatureList.Where(filterFunc)) {
                    DrawGeometry(graphics, null, brush, filter.Geometry);
                }
            });
            return this;
        }

        public MapLoader AddLine(Color color, int strokeSize, Func<IFeature, bool> filterFunc) {
            Pen pen = new(color, strokeSize);
            actionList.Add(graphics => {
                foreach (var filter in FeatureList.Where(filterFunc)) {
                    DrawGeometry(graphics, pen, null, filter.Geometry);
                }
            });
            return this;
        }

        public MapLoader AddImage(string path, double longitude, double latitude) {
            Bitmap image = new(path);
            Coordinate coordinate = new(longitude, latitude);
            actionList.Add(graphics => {
                Envelope.ExpandToInclude(coordinate);
                PointF point = ToPoint(coordinate);
                float x = point.X - image.Width / 2.0f;
                float y = point.Y - image.Height / 2.0f;
                graphics.DrawImage(image, x, y, image.Width, image.Height);
            });

            return this;
        }

        private void DrawGeometry(Graphics graphics, Pen pen, Brush brush, Geometry geometry) {
            if (geometry is Polygon polygon)
                DrawPolygon(graphics, pen, brush, polygon);
            else if (geometry is MultiPolygon multiPolygon) {
                foreach (Polygon p in multiPolygon.Geometries)
                    DrawPolygon(graphics, pen, brush, p);
            } 
            else if (geometry is LineString lineString) {
                var pointF = ToPoints(lineString.Coordinates);
                if (pen == null)
                    return;
                graphics.DrawLines(pen, pointF);
            } 
            else if (geometry is MultiLineString multiLine) {
                foreach (LineString line in multiLine.Geometries)
                    DrawGeometry(graphics, pen, brush, line);
            }
        }

        private void DrawPolygon(Graphics graphics, Pen pen, Brush brush, Polygon polygon) {
            PointF[] pointF = ToPoints(polygon.ExteriorRing.Coordinates);
            graphicsPath.AddPolygon(pointF);

            foreach (var hole in polygon.InteriorRings) {
                graphicsPath.AddPolygon(ToPoints(hole.Coordinates));
            }

            if (brush != null)
                graphics.FillPath(brush, graphicsPath);
            if (pen != null)
                graphics.DrawPolygon(pen, pointF);
        }

        public MapLoader SetDrawArea(Func<IFeature, bool> filter) {
            List<Envelope>? envelopes = FeatureList.Where(filter)
                .Select(feature => feature.Geometry.EnvelopeInternal)
                .ToList();

            if (envelopes.Count == 0) {
                return this;
            }

            var result = new Envelope(envelopes.First());

            foreach (var env in envelopes)
                result.ExpandToInclude(env);
            this.Envelope = result;

            return this;
        }


        public Bitmap Build() {
            using (var graphic = Graphics.FromImage(BitMap)) {
                graphic.SmoothingMode = SmoothingMode.AntiAlias;
                foreach (var action in actionList)
                    action(graphic);
            }
            return BitMap;
        }

        private PointF[] ToPoints(Coordinate[] coordinates) {
            return coordinates.Select(coordinate => {
                float x = (float)((coordinate.X - Envelope.MinX) * BitMap.Width / Envelope.Width);
                float y = (float)((Envelope.MaxY - coordinate.Y) * BitMap.Height / Envelope.Height);
                return new PointF(x, y);
            }).ToArray();
        }

        private PointF ToPoint(Coordinate coordinate) {
            float x = (float)((coordinate.X - Envelope.MinX) * BitMap.Width / Envelope.Width);
            float y = (float)((Envelope.MaxY - coordinate.Y) * BitMap.Height / Envelope.Height);
            return new PointF(x, y);
        }

    }
}
