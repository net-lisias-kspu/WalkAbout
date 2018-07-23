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
using KspWalkAbout.Extensions;
using KspWalkAbout.Values;
using KspWalkAbout.WalkAboutFiles;
using System;
using System.Collections.Generic;
using static KspWalkAbout.Entities.WalkAboutPersistent;

namespace KspWalkAbout.Entities
{
    /// <summary>Represents a collection of all locations that this mod can use to place kerbals.</summary>
    internal class KnownPlaces : List<LocationFile>
    {
        private int _maxQueueing = int.MinValue;

        public List<Location> AllLocations { get; private set; }
        public List<Location> AvailableLocations { get; private set; }
        public List<string> AvailableFacilities { get; private set; }
        internal bool IsChanged { get; set; }

        internal KnownPlaces()
        {
            RefreshLocations();
        }

        /// <summary>Writes the information about all the locations to their respective disk files.</summary>
        internal void Save()
        {
            foreach (var locationFile in this)
            {
                if (locationFile.IsChanged)
                {
                    locationFile.Save();
                    $"saved locations to {locationFile.FilePath}".Log();
                }
                else
                {
                    $"no save required for {locationFile.FilePath}".Debug();
                }
            }
            $"{Count} location files checked for saving".Debug();
            IsChanged = false;
        }

        /// <summary>Reevaluates the locations that can be displayed based on current facility upgrade levels.</summary>
        internal void RefreshLocations()
        {
            "Refresh()".Debug();
            AllLocations = new List<Location>();
            AvailableLocations = new List<Location>();
            AvailableFacilities = new List<string>();

            if (Count == 0)
            {
                LoadLocationFiles();
            }

            foreach (var locationFile in this)
            {
                foreach (var location in locationFile.Locations)
                {
                    AllLocations.Add(location);

                    if (location.File == null)
                    {
                        location.File = locationFile;
                    }
                    FacilityLevels currentLevel = GetFacilityLevel(location.FacilityName);
                    if ((location.AvailableAtLevels & currentLevel) != FacilityLevels.None)
                    {
                        AvailableLocations.Add(location);
                        if (!AvailableFacilities.Contains(location.FacilityName))
                        {
                            AvailableFacilities.Add(location.FacilityName);
                        }
                    }

                    _maxQueueing = Math.Max(_maxQueueing, location.Queueing);
                }
            }

            AvailableLocations.Sort(CompareLocations);
            $"availabe = {AvailableLocations.Count} of {AllLocations.Count} locations".Debug();
        }

        /// <summary>
        /// Determines the new order of locations within a sorted list of locations after a specific
        /// location has chosen.
        /// </summary>
        /// <param name="name">The id of the chosen location.</param>
        /// <remarks>
        /// Each time a location is chosen it is moved up in the rank of all known locations:
        /// <list type="text">
        /// <item>
        /// If this is the first time the location is chosen, it is moved to the bottom of the list of
        /// previously chosen locations.
        /// </item>
        /// <item>
        /// Each subesequent use of the location will move it up the list by approximately 1/2 its 
        /// "distance" to the top of the list.
        /// </item>
        /// </list>
        /// </remarks>
        internal void UpdateQueuing(string name)
        {
            var origIndex = FindIndex(name, AvailableLocations);
            var origLocation = AvailableLocations[origIndex];
            var origQueueing = AvailableLocations[origIndex].Queueing;
            var newQueueing = (origQueueing == 0) ? 1 : Math.Min(_maxQueueing, _maxQueueing - (_maxQueueing - origQueueing) / 2 + 1);
            $"Requeueing operation to change {origLocation.LocationName} from {origQueueing} to {newQueueing} ".Debug();
            _maxQueueing += (origQueueing == 0) ? 1 : 0;
            AvailableLocations.RemoveAt(origIndex);
            AvailableLocations.Insert(_maxQueueing - newQueueing, origLocation);
            for (var index = 0; index < _maxQueueing; index++)
            {
                var newIndex = _maxQueueing - index;
                $"Requeueing {AvailableLocations[index].LocationName} from position {AvailableLocations[index].Queueing} to {newIndex}".Debug();
                if (AvailableLocations[index].Queueing != newIndex)
                {
                    AvailableLocations[index].Queueing = newIndex;
                    AvailableLocations[index].File.IsChanged = true;
                    IsChanged = true;
                }
            }

            if (IsChanged)
                Save();
        }

        /// <summary>Includes a new location in the collection of all known locations.</summary>
        /// <param name="request">The user's requested information to be applied to the new location.</param>
        internal void AddLocation(LocationRequest request)
        {
            Location location = CreateRequestedLocation(request);
            $"Requested location {location.LocationName} created".Debug();

            LocationFile userLocationFile = null;
            foreach (var file in this)
            {
                if (file.FilePath.ToLower().EndsWith("user.loc"))
                {
                    userLocationFile = file; "Found user.loc file".Debug();
                }
            }

            if (userLocationFile == null)
            {
                "user.loc not found - creating...".Debug();
                var locPath = $"{WalkAbout.GetModDirectory()}/locFiles/user.loc";
                userLocationFile = new LocationFile();
                var loaded = userLocationFile.Load(
                    locPath,
                    ConfigNode.CreateConfigFromObject(
                        new LocationFile { Locations = new List<Location> { location } }));
                $"location file {locPath} loaded = {loaded}".Debug();
                Add(userLocationFile);
            }
            else
            {
                userLocationFile.Locations.Add(location);
            }

            userLocationFile.IsChanged = true; $"location file {userLocationFile.FilePath} needs saving".Debug();
            IsChanged = true;

            RefreshLocations();

            $"{request.Name} added to known locations.".Debug();
            ScreenMessages.PostScreenMessage(new ScreenMessage($"{request.Name} added to known locations.", 4.0f, ScreenMessageStyle.UPPER_LEFT));
        }

        /// <summary>Indicates whether or not this collection has a location with the given id.</summary>
        /// <param name="name">The name of the location to be found.</param>
        /// <returns>A value indicating whether the given id is a known location.</returns>
        internal bool HasLocation(string name)
        {
            foreach (var file in this)
            {
                if (FindIndex(name, file.Locations) != -1)
                {
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// Determines the locations (from a given list of locations) that are closest to the given target.
        /// </summary>
        /// <param name="targetLatitude">The target's latitude.</param>
        /// <param name="targetLongitude">The target's longitude.</param>
        /// <param name="targetAltitude">The target's altitude (ASL).</param>
        /// <param name="places">The population of locations to choose from.</param>
        /// <returns>An array of locations (one for each possible facility upgrade level) that are closest to the target coordinates.</returns>
        internal Locale[] FindClosest(double targetLatitude, double targetLongitude, double targetAltitude)
        {
            var closest = new Locale[]
            {
                new Locale { },
                new Locale { Distance = double.MaxValue },
                new Locale { Distance = double.MaxValue },
                new Locale { Distance = double.MaxValue }
            };
            var conv = Math.PI / 180;

            foreach (var location in AllLocations)
            {
                var dlon = (location.Longitude - targetLongitude) * conv;
                var dlat = (location.Latitude - targetLatitude) * conv;
                var a =
                    Math.Pow(Math.Sin(dlat / 2), 2) +
                    Math.Cos(targetLatitude * conv) * Math.Cos(location.Latitude * conv) * Math.Pow(Math.Sin(dlon * conv / 2), 2);
                var c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));
                var h = Math.Abs((600000 + targetAltitude) * c);
                var v = targetAltitude - location.Altitude;
                var d = Math.Sqrt(h * h + v * v);

                for (int level = 1; level < 4; level++)
                {
                    var facilityLevel = (FacilityLevels)(int)Math.Pow(2, level - 1);
                    if (((location.AvailableAtLevels & facilityLevel) != FacilityLevels.None) && (d < closest[level].Distance))
                    {
                        closest[level] = new Locale { Name = location.LocationName, Horizontal = h, Vertical = v, Distance = d };
                    }
                }
            }

            return closest;
        }

        /// <summary>Obtains the current upgrade level of a facility.</summary>
        /// <param name="facilityName">The name of the facility.</param>
        /// <returns>A value indicating the current upgrade level.</returns>
        private static FacilityLevels GetFacilityLevel(string facilityName)
        {
            return (FacilityLevels)(int)
                Math.Pow(2, (ScenarioUpgradeableFacilities.protoUpgradeables[facilityName].GetLevel() *
                             ScenarioUpgradeableFacilities.GetFacilityLevelCount(facilityName)));
        }

        /// <summary>Creates a location based on the supplied user information and the currently active vessel.</summary>
        /// <param name="request">The user's requested location information.</param>
        /// <returns>A new location.</returns>
        private static Location CreateRequestedLocation(LocationRequest request)
        {
            return new Location
            {
                LocationName = request.Name,
                FacilityName = request.AssociatedFacility,
                AvailableAtLevels = GetFacilityLevel(request.AssociatedFacility),
                Queueing = 0,
                Latitude = FlightGlobals.ActiveVessel.latitude,
                Longitude = FlightGlobals.ActiveVessel.longitude,
                Altitude = FlightGlobals.ActiveVessel.altitude,
                Normal = FlightGlobals.ActiveVessel.terrainNormal,
                Rotation = FlightGlobals.ActiveVessel.transform.rotation,
            };
        }

        /// <summary>Loads all location files in the WalkAbout mod's location file directory.</summary>
        private void LoadLocationFiles()
        {
            var di = new System.IO.DirectoryInfo($"{WalkAbout.GetModDirectory()}/locFiles");
            foreach (var file in di.GetFiles("*.loc"))
            {
                var locationFile = new LocationFile();
                var loaded = locationFile.Load(file.FullName); $"loading locations file {file.FullName} = {loaded}".Debug();
                if (loaded)
                {
                    Add(locationFile);
                }
                locationFile.StatusMessage.Log();
            }
            $"{Count} location files loaded".Debug();
        }

        /// <summary>
        /// Finds the index of a location (matching the given location name) within the supplied
        /// list of locations.
        /// </summary>
        /// <param name="name">The name of the location to be found.</param>
        /// <param name="targetList">The population of locations to be searched.</param>
        /// <returns>The index of the matching location (-1 if not found).</returns>
        private int FindIndex(string name, List<Location> targetList)
        {
            var searchName = name.ToUpper();

            for (var index = 0; index < targetList.Count; index++)
            {
                if (targetList[index].LocationName.ToUpper() == searchName)
                {
                    return index;
                }
            }

            return -1;
        }

        /// <summary> Compares two locations ordering them by queueing and name.</summary>
        /// <param name="a">The base location.</param>
        /// <param name="b">The location to be combared to the base location.</param>
        /// <returns>A value indicating whether location b comes before or after location a.</returns>
        private int CompareLocations(Location a, Location b)
        {
            var queueOrder = (b?.Queueing ?? 0).CompareTo(a?.Queueing ?? 0);
            return queueOrder == 0
                ? string.Compare(a?.FacilityName + a?.LocationName, b?.FacilityName + b?.LocationName, StringComparison.Ordinal)
                : queueOrder;
        }

        /// <summary>Represents basic information about nearby locations.</summary>
        internal struct Locale
        {
            public string Name;
            public double Horizontal;
            public double Vertical;
            public double Distance;
        }
    }
}