/*
	This file is part of Walk About /L Unleashed
		© 2023 Lisias T : http://lisias.net <support@lisias.net>
		© 2016-2017 Antipodes (Clive Pottinger)

	Walk About /L Unleashed is licensed as follows:
		* GPL 3.0 : https://www.gnu.org/licenses/gpl-3.0.txt

	Walk About /L Unleashed is distributed in the hope that
	it will be useful, but WITHOUT ANY WARRANTY; without even the implied
	warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.

	You should have received a copy of the GNU General Public License 3.0
	along with Walk About /L Unleashed. If not, see <https://www.gnu.org/licenses/>.

*/

using KspWalkAbout.Values;
using System;

namespace KspWalkAbout.Entities
{
    public class GreatCircle
    {   /// <summary>
        /// Represents a great circle route between two points on a spherical celestial body.
        /// </summary> <remarks> Thanks to Chris Veness for his reference site:
        /// http://www.movable-type.co.uk/scripts/latlong.html </remarks>
        public GreatCircle(WorldCoordinates origin, WorldCoordinates destination)
        {
            Origin = origin;
            Destination = destination;

            if ((origin.World == null) || (destination.World == null))
            {
                if (origin.WorldRadius <= 0)
                {
                    throw new ApplicationException("Greate circle calculations require that Radius must be greater than 0");
                }
                else if (origin.WorldRadius != destination.WorldRadius)
                {
                    throw new ApplicationException("Start and end points of a great circle must have the same world radius");
                }
            }
            else if (origin.World != destination.World)
            {
                throw new ApplicationException("Start and end points of a great circle must be on the same celestial body");
            }

            CalculateGreatCircleValues();
        }

        /// <summary>
        /// The coordinates of the starting point of the great circle line segment.
        /// </summary>
        public WorldCoordinates Origin { get; private set; }

        /// <summary>
        /// The coordinates of the ending point of the great circle line segment.
        /// </summary>
        public WorldCoordinates Destination { get; private set; }

        /// <summary>
        /// The mean (sea-level) radius of the body on which the great circle lies.
        /// </summary>
        public double WorldRadius { get { return Origin.WorldRadius; } }

        /// <summary>
        /// Difference in altitude between the origin and the destination.
        /// </summary>
        public double DeltaASL { get; private set; }

        /// <summary>
        /// Distance from origin to destination while travelling at sea level.
        /// </summary>
        public double DistanceAtSeaLevel { get; private set; }

        /// <summary>
        /// Distance from origin to destination while travelling at the origin's altitude.
        /// </summary>
        public double DistanceAtOrigAlt { get; private set; }

        /// <summary>
        /// Distance from origin to destination while travelling at the destination's altitude.
        /// </summary>
        public double DistanceAtDestAlt { get; private set; }

        /// <summary>
        /// Distance from origin to destination with smoothly changing altitude.
        /// </summary>
        public double DistanceWithAltChange { get; private set; }

        /// <summary>
        /// The bearing from the origin to the destination.
        /// </summary>
        public double ForwardAzimuth { get; private set; }

        private void CalculateGreatCircleValues()
        {
            double startLat = Origin.Latitude * Constants.DegreesToRadiansFactor;      // φ1 
            double startLong = Origin.Longitude * Constants.DegreesToRadiansFactor;    // λ1
            double endLat = Destination.Latitude * Constants.DegreesToRadiansFactor;   // φ2
            double endLong = Destination.Longitude * Constants.DegreesToRadiansFactor; // λ2

            // Haversine great-circle: 
            double a =                                               // a = sin²(Δφ/2) + cos φ1 ⋅ cos φ2 ⋅ sin²(Δλ/2)
                Math.Pow(Math.Sin((endLat - startLat) / 2), 2) +     //     sin²(Δφ/2)
                Math.Cos(startLat) * Math.Cos(endLat) *              //     cos φ1 ⋅ cos φ2
                Math.Pow(Math.Sin((endLong - startLong) / 2), 2);    //     sin²(Δλ/2)
            double c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));  // c = 2 ⋅ atan2( √a, √(1−a) )
            DistanceAtSeaLevel = WorldRadius * c;                    // d = R ⋅ c

            // Accounting for a difference in altitude:
            DeltaASL = Destination.Altitude - Origin.Altitude;
            DistanceAtOrigAlt = Math.Abs((WorldRadius + Origin.Altitude) * c);
            DistanceAtDestAlt = Math.Abs((WorldRadius + Destination.Altitude) * c);
            // unproven assumption distance is average of inferior and superior great circle routes:
            DistanceWithAltChange = (DistanceAtOrigAlt + DistanceAtDestAlt) / 2;

            // Initial bearing: θ = atan2( sin Δλ ⋅ cos φ2 , cos φ1 ⋅ sin φ2 − sin φ1 ⋅ cos φ2 ⋅ cos Δλ )
            double y = Math.Sin(endLong - startLong) * Math.Cos(endLat);                   // sin Δλ ⋅ cos φ2 
            double x = Math.Cos(startLat) * Math.Sin(endLat) -                             // cos φ1 ⋅ sin φ2
                    Math.Sin(startLat) * Math.Cos(endLat) * Math.Cos(endLong - startLong); // sin φ1 ⋅ cos φ2 ⋅ cos Δλ
            double t = Math.Atan2(y, x) * Constants.RadiansToDegreesFactor;
            ForwardAzimuth = (t + 360) % 360;
        }
    }
}