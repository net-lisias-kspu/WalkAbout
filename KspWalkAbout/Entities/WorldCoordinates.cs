/*  Copyright 2017 Clive Pottinger
    This file is part of the WalkAbout Mod.

    WalkAbout is free software: you can redistribute it and/or modify
    it under the terms of the GNU General Public License as published by
    the Free Software Foundation, either version 3 of the License, or
    (at your option) any later version.

    WalkAbout is distributed in the hope that it will be useful,
    but WITHOUT ANY WARRANTY; without even the implied warranty of
    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.See the
    GNU General Public License for more details.

    You should have received a copy of the GNU General Public License
    along with WalkAbout.  If not, see<http://www.gnu.org/licenses/>.
*/

using KspWalkAbout.Values;
using System;
using static KspAccess.CommonKspAccess;

namespace KspWalkAbout.Entities
{
    /// <summary>
    /// Represents a location on a spherical celstial body using latitude and longitude.
    /// </summary>
    /// <remarks>Thanks to Chris Veness for his reference site: http://www.movable-type.co.uk/scripts/latlong.html</remarks>
    public class WorldCoordinates
    {
        private double? _radius;

        public WorldCoordinates() { }

        public WorldCoordinates(CelestialBody world, double lat, double lng, double alt) : this(lat, lng, alt, world.Radius)
        {
            World = world;
        }

        public WorldCoordinates(double lat, double lng, double alt, double radius)
        {
            Latitude = lat;
            Longitude = lng;
            Altitude = alt;
            WorldRadius = radius;
        }

        /// <summary>
        /// Gets the altitude of the point above sea level in metres.
        /// </summary>
        public double Altitude { get; internal set; }

        public double Latitude { get; internal set; }

        public double Longitude { get; internal set; }

        public CelestialBody World { get; internal set; }

        public double WorldRadius
        {
            get { return _radius ?? World?.Radius ?? 0; }
            set { _radius = value; }
        }

        public WorldCoordinates Add(double bearing, double distance, double deltaASL = 0d)
        {
            var startLat = Latitude * Constants.DegreesToRadiansFactor;   // φ1
            var startLong = Longitude * Constants.DegreesToRadiansFactor; // λ1
            var radius = WorldRadius + Altitude;                          // R
            var angularDist = distance / radius;                          // δ 

            // φ2 = asin( sin φ1 ⋅ cos δ + cos φ1 ⋅ sin δ ⋅ cos θ )
            var angle = bearing * Constants.DegreesToRadiansFactor;                // θ
            var endLat =
                Math.Asin(
                    Math.Sin(startLat) * Math.Cos(angularDist) +                   // sin φ1 ⋅ cos δ
                    Math.Cos(startLat) * Math.Sin(angularDist) * Math.Cos(angle)); // cos φ1 ⋅ sin δ ⋅ cos θ 

            // λ2 = λ1 + atan2( sin θ ⋅ sin δ ⋅ cos φ1, cos δ − sin φ1 ⋅ sin φ2 )
            var a = Math.Sin(angle) * Math.Sin(angularDist) * Math.Cos(startLat);  // sin θ ⋅ sin δ ⋅ cos φ1
            var b = Math.Cos(angularDist) - Math.Sin(startLat) * Math.Sin(endLat); // cos δ − sin φ1 ⋅ sin φ2
            var endLong = startLong + Math.Atan2(a, b);

            return new WorldCoordinates
            {
                Altitude = Altitude + deltaASL,
                Latitude = endLat * Constants.RadiansToDegreesFactor,
                Longitude = (endLong * Constants.RadiansToDegreesFactor + 540) % 360 - 180,
                World = World,
            };
        }

        public WorldCoordinates Travel(GreatCircle route)
        {
            return Add(route.ForwardAzimuth, route.DistanceAtOrigAlt, route.DeltaASL);
        }

        public static WorldCoordinates GetFacilityCoordinates<T>() where T : SpaceCenterBuilding
        {
            var facilityVectorPosition =
                   ((T)(SpaceCenter.FindObjectOfType(typeof(T))))
                   .transform
                   .position;

            var facilityCoordinates = new WorldCoordinates
            {
                Latitude = Homeworld.GetLatitude(facilityVectorPosition),
                Longitude = Homeworld.GetLongitude(facilityVectorPosition),
                Altitude = Homeworld.GetAltitude(facilityVectorPosition),
                World = Homeworld,
            };

            return facilityCoordinates;
        }
    }
}