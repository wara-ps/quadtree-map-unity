using System;

namespace Combitech.Geographics
{
    public static class Utilities
    {
        private const double Radius = 6371000;

        public static double BearingTo(LatLng from, LatLng to)
        {
            double phi1 = from.Lat * Math.PI / 180.0;
            double phi2 = to.Lat * Math.PI / 180.0;
            double dlambda = (to.Lng - from.Lng) * Math.PI / 180.0;

            double y = Math.Sin(dlambda) * Math.Cos(phi2);
            double x = Math.Cos(phi1) * Math.Sin(phi2) - Math.Sin(phi1) * Math.Cos(phi2) * Math.Cos(dlambda);
            return Math.Atan2(y, x) * 180.0 / Math.PI;
        }

        public static LatLng PointAtDistance(LatLng origin, double distance, double bearing)
        {
            double theta = bearing * Math.PI / 180.0;
            double phi1 = origin.Lat * Math.PI / 180.0;
            double phi2 = Math.Asin(Math.Sin(phi1) * Math.Cos(distance / Radius) + Math.Cos(phi1) * Math.Sin(distance / Radius) * Math.Cos(theta));

            double lambda1 = origin.Lng * Math.PI / 180.0;
            double y = Math.Sin(theta) * Math.Sin(distance / Radius) * Math.Cos(phi1);
            double x = Math.Cos(distance / Radius) - Math.Sin(phi1) * Math.Sin(phi2);
            double lambda2 = lambda1 + Math.Atan2(y, x);
            return new LatLng(phi2 * 180.0 / Math.PI, lambda2 * 180.0 / Math.PI);
        }
    }
}
