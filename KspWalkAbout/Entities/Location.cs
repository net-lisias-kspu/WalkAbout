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
using KspWalkAbout.Values;
using KspWalkAbout.WalkAboutFiles;
using UnityEngine;
using static KspWalkAbout.Entities.WalkAboutPersistent;

namespace KspWalkAbout.Entities
{
    /// <summary>
    /// Represents a location where a kerbal may be placed.
    /// </summary>
    public class Location
    {
        private WorldCoordinates _coordinates;

        /// <summary>
        /// Text id of the location.
        /// </summary>
        [Persistent]
        public string LocationName;

        /// <summary>
        /// The KSC facility that this location is associated with.
        /// </summary>
        [Persistent]
        public string FacilityName;

        /// <summary>
        /// Indicates the facility's upgrade levels at which this location appears.
        /// </summary>
        [Persistent]
        public FacilityLevels AvailableAtLevels;

        /// <summary>
        /// An indicator of where this level should appear in a sorted list of levels (higher numbers
        /// appear earlier on the list).
        /// </summary>
        [Persistent]
        public int Queueing;

        /// <summary>
        /// The great circle initial bearing from the standard centrum (the KSC flagpole) to this location.
        /// </summary>
        [Persistent]
        public double ForwardAzimuth;

        /// <summary>
        /// The great circle distance from the standard centrum (the KSC flagpole) to this location.
        /// </summary>
        [Persistent]
        public double Distance;

        /// <summary>
        /// The change in altitude from the standard centrum (the KSC flagpole) to this location.
        /// </summary>
        [Persistent]
        public double DeltaAltitude;

        /// <summary>
        /// The direction that points vertically from the location (local "up").
        /// </summary>
        [Persistent]
        public Vector3 Normal;

        /// <summary>
        /// The orientation for any kerbal placed at this location.
        /// </summary>
        [Persistent]
        public Quaternion Rotation;

        /// <summary>
        /// The full path of the disk file which holds this location.
        /// </summary>
        public LocationFile File { get; internal set; }

        public WorldCoordinates Coordinates
        {
            get
            {
                if (_coordinates == null)
                {
                    var centrum = GetCentrum();
                    _coordinates = centrum.Coordinates.Add(
                        ForwardAzimuth + centrum.AngularOffset,
                        Distance * centrum.HorizontalScale,
                        DeltaAltitude * centrum.VerticalScale);
                    $"set location {LocationName} to lat:[{ForwardAzimuth} + {centrum.AngularOffset}]={_coordinates.Latitude} long:[{Distance} * {centrum.HorizontalScale}]={_coordinates.Longitude} delta alt:[{DeltaAltitude} * {centrum.VerticalScale}]={_coordinates.Altitude}".Debug();
                }

                return _coordinates;
            }
        }
    }
}