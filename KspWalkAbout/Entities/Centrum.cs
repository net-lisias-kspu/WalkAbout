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
