using System;

namespace Combitech.Geographics
{
    public class LatLng
    {
        public double Lat { get; set; }
        public double Lng { get; set; }
        public float Height { get; set; }
        public DateTime Timestamp { get; set; }

        public LatLng() { }

        public LatLng(double lat, double lng)
        {
            Lat = lat;
            Lng = lng;
            Height = 0;
        }

        public LatLng(double lat, double lng, float height)
        {
            Lat = lat;
            Lng = lng;
            Height = height;
        }

        public override string ToString()
        {
            return Lat + "," + Lng + " (" + Height + "m)";
        }
    }

    public class UtmCoord
    {
        public double Easting, Northing;

        public UtmCoord(double easting, double northing)
        {
            Easting = easting;
            Northing = northing;
        }

        public void Set(double easting, double northing)
        {
            Easting = easting;
            Northing = northing;
        }

        public double Distance(UtmCoord other)
        {
            double de = Easting - other.Easting;
            double dn = Northing - other.Northing;
            return Math.Sqrt(de * de + dn * dn);
        }

        public double SquaredDistance(UtmCoord other)
        {
            double de = Easting - other.Easting;
            double dn = Northing - other.Northing;
            return de * de + dn * dn;
        }

        public double ManhattanDistance(UtmCoord other)
        {
            double de = Math.Abs(Easting - other.Easting);
            double dn = Math.Abs(Northing - other.Northing);
            return de + dn;
        }

        public override string ToString()
        {
            return Easting.ToString("#.#") + ", " + Northing.ToString("#.#");
        }
    }

    public class Velocity
    {
        public float Speed { get; set; }
        public float Bearing { get; set; }
        public DateTime Timestamp { get; set; }
    }
}
