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

using System;
using static KspWalkAbout.Values.Constants;


namespace KspWalkAbout.Entities
{
    public class Centrum
    {
        public Centrum()
        {
            Coordinates = WorldCoordinates.GetFacilityCoordinates<FlagPoleFacility>();
            Log.detail("KSC flag = lat:{0} long:{1} alt:{2} radius:{3}", Coordinates.Latitude, Coordinates.Longitude, Coordinates.Altitude, Coordinates.WorldRadius);

            WorldCoordinates VABPosition = WorldCoordinates.GetFacilityCoordinates<VehicleAssemblyBuilding>();
            Log.detail("VAB = lat:{0} long:{1} alt:{2}", VABPosition.Latitude, VABPosition.Longitude, VABPosition.Altitude);

            GreatCircle route = new GreatCircle(Coordinates, VABPosition);
            Log.detail("Route from Flag to VAB = bearing:{0} dist:{1} alt:{2}", route.ForwardAzimuth, route.DistanceAtOrigAlt, route.DeltaASL);

            AngularOffset =
                Math.Round(route.ForwardAzimuth - BaseBearingFlagToVAB, RoundingAccuracy, MidpointRounding.AwayFromZero);
            HorizontalScale =
                Math.Round(route.DistanceAtOrigAlt / BaseDistanceFlagToVAB, RoundingAccuracy, MidpointRounding.AwayFromZero);
            VerticalScale =
                Math.Round(route.DeltaASL / BaseDeltaAltFlagToVAB, RoundingAccuracy, MidpointRounding.AwayFromZero);
            Log.detail("Offset:{0} degrees, Scaling:{1}(h) x {2}(v)", AngularOffset, HorizontalScale, VerticalScale);
        }

        public double AngularOffset { get; private set; }
        public WorldCoordinates Coordinates { get; private set; }
        public double HorizontalScale { get; private set; }
        public double VerticalScale { get; private set; }
    }
}
