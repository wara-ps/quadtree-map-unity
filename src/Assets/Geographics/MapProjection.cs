using ProjNet.Converters.WellKnownText;
using ProjNet.CoordinateSystems;
using ProjNet.CoordinateSystems.Transformations;
using System.Collections.Generic;
using System.IO;

namespace Combitech.Geographics
{
    public class MapProjection
    {
        public struct WKTstring
        {
            public int WKID;
            public string WKT;
        }

        private readonly string DbPath = "";

        private readonly IProjectedCoordinateSystem Projection;
        private readonly ICoordinateTransformationFactory TransformFactory = new CoordinateTransformationFactory();
        private ICoordinateTransformation FromWGS84ToProjection, FromProjectionToWGS84;

        public MapProjection(string path, int epsg)
        {
            DbPath = Path.Combine(path, "SRID.csv");
            Projection = (IProjectedCoordinateSystem)GetCSbyID(epsg);
            MakeTransforms();
        }

        public MapProjection(int zone, bool north)
        {
            Projection = ProjectedCoordinateSystem.WGS84_UTM(zone, north);
            MakeTransforms();
        }

        public UtmCoord ToProjection(LatLng latlng)
        {
            return ToProjection(latlng.Lat, latlng.Lng);
        }

        public UtmCoord ToProjection(double lat, double lng)
        {
            double[] d = FromWGS84ToProjection.MathTransform.Transform(new double[] { lng, lat });
            return new UtmCoord(d[0], d[1]);
        }

        public LatLng FromProjection(UtmCoord utm)
        {
            double[] d = FromProjectionToWGS84.MathTransform.Transform(new double[] { utm.Easting, utm.Northing });
            return new LatLng(d[1], d[0]);
        }

        private void MakeTransforms()
        {
            FromWGS84ToProjection = TransformFactory.CreateFromCoordinateSystems(GeographicCoordinateSystem.WGS84, Projection);
            FromProjectionToWGS84 = TransformFactory.CreateFromCoordinateSystems(Projection, GeographicCoordinateSystem.WGS84);
        }

        private IEnumerable<WKTstring> GetSRIDs()
        {
            using (var sr = File.OpenText(DbPath))
            {
                while (!sr.EndOfStream)
                {
                    var line = sr.ReadLine();
                    int split = line.IndexOf(';');
                    if (split > -1)
                    {
                        var wkt = new WKTstring
                        {
                            WKID = int.Parse(line.Substring(0, split)),
                            WKT = line.Substring(split + 1)
                        };
                        yield return wkt;
                    }
                }
                sr.Close();
            }
        }

        private ICoordinateSystem GetCSbyID(int id)
        {
            foreach (WKTstring wkt in GetSRIDs())
            {
                if (wkt.WKID == id)
                {
                    return CoordinateSystemWktReader.Parse(wkt.WKT) as ICoordinateSystem;
                }
            }
            return null;
        }
    }
}

