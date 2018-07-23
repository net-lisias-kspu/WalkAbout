/*  Copyright 2016 Clive Pottinger
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
using KspWalkAbout.WalkAboutFiles;
using UnityEngine;

namespace KspWalkAbout.Entities
{
    /// <summary>Represents a location (on Kerbin) where a kerbal may be placed.</summary>
    public class Location
    {
        /// <summary>Text id of the location.</summary>
        [Persistent]
        public string LocationName;

        /// <summary>The KSC facility that this location is associated with.</summary>
        [Persistent]
        public string FacilityName;

        /// <summary>Indicates the facility's upgrade levels at which this location appears.</summary>
        [Persistent]
        public FacilityLevels AvailableAtLevels;

        /// <summary>
        /// An indicator of where this level should appear in a sorted list of levels 
        /// (higher numbers appear earlier on the list).
        /// </summary>
        [Persistent]
        public int Queueing;

        /// <summary>The latitude of the location.</summary>
        [Persistent]
        public double Latitude;

        /// <summary>The longitude of the location.</summary>
        [Persistent]
        public double Longitude;

        /// <summary>The altitude (ASL) of the location.</summary>
        [Persistent]
        public double Altitude;

        /// <summary>The direction that points vertically from the location (local "up).</summary>
        [Persistent]
        public Vector3 Normal;

        /// <summary>The orientation for any kerbal placed at this location.</summary>
        [Persistent]
        public Quaternion Rotation;

        /// <summary>The full path of the disk file which holds this location.</summary>
        public LocationFile File { get; internal set; }
    }
}