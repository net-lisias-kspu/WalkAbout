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

using KspWalkAbout.Extensions;
using System;
using static KspWalkAbout.Values.Constants;


namespace KspWalkAbout.Entities
{
    public class Centrum
    {
        public Centrum()
        {
            Coordinates = WorldCoordinates.GetFacilityCoordinates<FlagPoleFacility>();
            $" KSC flag = lat:{Coordinates.Latitude} long:{Coordinates.Longitude} alt:{Coordinates.Altitude} radius:{Coordinates.WorldRadius}".Debug();

            var VABPosition = WorldCoordinates.GetFacilityCoordinates<VehicleAssemblyBuilding>();
            $"VAB = lat:{VABPosition.Latitude} long:{VABPosition.Longitude} alt:{VABPosition.Altitude}".Debug();

            var route = new GreatCircle(Coordinates, VABPosition);
            $"Route from Flag to VAB = bearing:{route.ForwardAzimuth} dist:{route.DistanceAtOrigAlt} alt:{route.DeltaASL}".Debug();

            AngularOffset =
                Math.Round(route.ForwardAzimuth - BaseBearingFlagToVAB, RoundingAccuracy, MidpointRounding.AwayFromZero);
            HorizontalScale =
                Math.Round(route.DistanceAtOrigAlt / BaseDistanceFlagToVAB, RoundingAccuracy, MidpointRounding.AwayFromZero);
            VerticalScale =
                Math.Round(route.DeltaASL / BaseDeltaAltFlagToVAB, RoundingAccuracy, MidpointRounding.AwayFromZero);
            $"Offset:{AngularOffset} degrees, Scaling:{HorizontalScale}(h) x {VerticalScale}(v)".Debug();
        }

        public double AngularOffset { get; private set; }
        public WorldCoordinates Coordinates { get; private set; }
        public double HorizontalScale { get; private set; }
        public double VerticalScale { get; private set; }
    }
}
