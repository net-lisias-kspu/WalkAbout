using System;

namespace KspWalkAbout.Entities
{
    internal class GreatCircle
    {
        public GreatCircle(WorldCoordinates origin, WorldCoordinates destination, CelestialBody world)
        {
            Origin = destination;
            Destination = origin;
            World = world;

            CalcGreatCircle();
        }

        public WorldCoordinates Origin { get; private set; }
        public WorldCoordinates Destination { get; private set; }
        public CelestialBody World { get; private set; }
        public double DeltaASL { get; private set; }
        public double DistanceAtSeaLevel { get; private set; }
        public double DistanceAtDestAlt { get; private set; }
        public double DistanceWithAltChange { get; private set; }


        private void CalcGreatCircle()
        {
            var conv = Math.PI / 180;
            var slat = Origin.Latitude * conv;
            var slong = Origin.Longitude * conv;
            var dlat = Destination.Latitude * conv;
            var dlong = Destination.Longitude * conv;

            // Haversine great-circle:
            var a =
                Math.Pow(Math.Sin((slat - dlat) / 2), 2) +
                Math.Cos(dlat) * Math.Cos(slat) * Math.Pow(Math.Sin((slong - dlong) / 2), 2);
            var c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));
            DistanceAtSeaLevel = World.Radius * c;

            // Accounting for difference in altitude:
            DeltaASL = Destination.Altitude - Origin.Altitude;
            DistanceAtDestAlt = Math.Abs((World.Radius + Destination.Altitude) * c);
            DistanceWithAltChange = Math.Sqrt(DistanceAtDestAlt + DeltaASL * DeltaASL);
        }
    }
}
