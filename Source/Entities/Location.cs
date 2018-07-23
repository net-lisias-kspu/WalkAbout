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
using UnityEngine;

namespace KspWalkAbout.Entities
{
    public class Location
    {
        [Persistent]
        public string LocationName;

        [Persistent]
        public string FacilityName;

        [Persistent]
        public FacilityLevels AvailableAtLevels;

        [Persistent]
        public int Queueing;

        [Persistent]
        public double Latitude;

        [Persistent]
        public double Longitude;

        [Persistent]
        public double Altitude;

        [Persistent]
        public Vector3 Normal;

        [Persistent]
        public Quaternion Rotation;

        public LocationFile File { get; internal set; }
    }
}