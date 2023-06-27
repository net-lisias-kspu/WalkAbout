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
using KspWalkAbout.WalkAboutFiles;
using System;
using System.Collections.Generic;
using System.Linq;
using static KspAccess.CommonKspAccess;
using static KspWalkAbout.Entities.WalkAboutPersistent;
using static KspWalkAbout.Values.Constants;

namespace KspWalkAbout.Entities
{
    /// <summary>
    /// Represents a collection of all locations that this mod can use to place kerbals.
    /// </summary>
    internal class KnownPlaces : List<LocationFile>
    {
        private List<Location> _allLocations;

        /// <summary>
        /// Creates a new instance of the KnownPlaces class.
        /// </summary>
        internal KnownPlaces()
        {
            RefreshLocations();
        }

        /// <summary>
        /// Gets the set of locations currently available for kerbal placement.
        /// </summary>
        public List<Location> AvailableLocations { get; private set; }

        /// <summary>
        /// Gets a key-value-pair collection of facilities currently accessible in the game and the current level of each.
        /// </summary>
        public Dictionary<string, FacilityLevels> AvailableFacilitiesLevel { get; private set; }

        /// <summary>
        /// Gets the indicator of whether or not any locations have been altered.
        /// </summary>
        internal bool IsChanged { get; private set; }

        /// <summary>
        /// Writes the information about all the locations to their respective disk files.
        /// </summary>
        internal void Save()
        {
            foreach (LocationFile locationFile in this)
            {
                if (locationFile.IsChanged)
                {
                    locationFile.Save();
                    Log.info("saved locations to {0}", locationFile.FilePath);
                }
                else
                {
                    Log.detail("no save required for {0}", locationFile.FilePath);
                }
            }
            Log.detail("{0} location files checked for saving", Count);
            IsChanged = false;
        }

        /// <summary>
        /// Reevaluates the locations that can be displayed based on current facility upgrade levels.
        /// </summary>
        internal void RefreshLocations()
        {
            Log.detail("Refresh()");
            _allLocations = new List<Location>();
            AvailableLocations = new List<Location>();
            AvailableFacilitiesLevel = new Dictionary<string, FacilityLevels>();

            if (Count == 0)
            {
                LoadLocationFiles();
            }

            foreach (LocationFile locationFile in this)
            {
                foreach (Location location in locationFile.Locations)
                {
                    _allLocations.Add(location);

                    if (location.File == null)
                    {
                        location.File = locationFile;
                    }
                    FacilityLevels currentLevel = GetFacilityLevel(location.FacilityName);

                    //$"Location [{location.LocationName}] at [{location.FacilityName}] levels: location=[{location.AvailableAtLevels}] facility=[{currentLevel}] result=[{(location.AvailableAtLevels & currentLevel)}] available=[{((location.AvailableAtLevels & currentLevel) != FacilityLevels.None)}]".Debug();
                    if ((location.AvailableAtLevels & currentLevel) != FacilityLevels.None)
                    {
                        AvailableLocations.Add(location);
                        if (!AvailableFacilitiesLevel.ContainsKey(location.FacilityName))
                        {
                            AvailableFacilitiesLevel.Add(location.FacilityName, currentLevel);
                        }
                    }
                }
            }

            AvailableLocations.Sort(CompareLocations);
            Log.detail("available = {0} of {1} locations", AvailableLocations.Count, _allLocations.Count);
        }

        /// <summary>
        /// Moves a chosen location to a new position within the ordered list of all locations.
        /// </summary>
        /// <param name="name">The id of the chosen location.</param>
        /// <remarks>
        /// Each time a location is chosen it is moved up in the rank of all known locations:
        /// <list type="text">
        /// <item>
        /// If this is the first time the location is chosen, it is moved to the bottom of the list
        /// of previously chosen locations.
        /// </item>
        /// <item>
        /// Each subesequent use of the location will move it up the list by approximately 1/2 its
        /// "distance" to the top of the list.
        /// </item>
        /// </list>
        /// </remarks>
        internal void UpdateQueuing(string name)
        {
            Location originalLocation = AvailableLocations[FindIndex(name, AvailableLocations)];
            int originalQueueing = originalLocation.Queueing;
            int maxQueueing = AvailableLocations[0].Queueing;

            int finalQueueing = (originalQueueing == 0) ? 1 : Math.Min(maxQueueing, maxQueueing - (maxQueueing - originalQueueing) / 2 + 1);
            if (finalQueueing == originalQueueing)
            {
                Log.detail("Requeueing {0} from {1}: no change", originalLocation.LocationName, originalQueueing);
                return;
            }

            Log.detail("Requeueing {0} from {1} to {2}", originalLocation.LocationName, originalQueueing, finalQueueing);
            originalLocation.Queueing = finalQueueing;
            originalLocation.File.IsChanged = true;
            _allLocations.Sort(CompareLocations);

            int nextQueueing = 0;
            for (int index = _allLocations.Count - 1; index >= 0; index--)
            {
                int currentQueueing = _allLocations[index].Queueing;
                if (currentQueueing != 0)
                {
                    _allLocations[index].Queueing = ++nextQueueing;
                    _allLocations[index].File.IsChanged = (currentQueueing != nextQueueing);
                }
            }

            IsChanged = true;
            Save();

            AvailableLocations.Sort(CompareLocations);
        }

        /// <summary>
        /// Includes a new location in the collection of all known locations.
        /// </summary>
        /// <param name="request">The user's requested information to be applied to the new location.</param>
        internal void AddLocation(LocationRequest request)
        {
            Location location = CreateRequestedLocation(request);
            Log.detail("Requested location {0} created", location.LocationName);

            LocationFile userLocationFile = GetUserLocationFile();
            Log.detail("userLocationFile = {0}", userLocationFile.Filename);
            userLocationFile.Locations.Add(location);

            userLocationFile.IsChanged = true; Log.detail("location file {0} needs saving", userLocationFile.FilePath);
            IsChanged = true;

            RefreshLocations();

            Log.detail("{0} added to known locations.", request.Name);
            ScreenMessages.PostScreenMessage(new ScreenMessage($"{request.Name} added to known locations.", 4.0f, ScreenMessageStyle.UPPER_LEFT));
        }

        private LocationFile GetUserLocationFile()
        {
            string searchFor = $"\\{Constants.UserLocationFilename}".ToLower();

            LocationFile userLocationFile = null;
            foreach (LocationFile file in this)
            {
                if (file.FilePath.ToLower().EndsWith("user.loc"))
                {
                    userLocationFile = file;
                }
            }
            //LocationFile userLocationFile = this.Where(f => f.FilePath.ToLower().EndsWith(searchFor)).FirstOrDefault();

            if (userLocationFile == null)
            {
                Log.detail("{0} not found - creating...", Constants.UserLocationFilename);
                string locPath = $"{WalkAbout.GetModDirectory()}/{Constants.UserLocationSubdirectory}/{Constants.UserLocationFilename}";
                userLocationFile = new LocationFile();
                bool loaded = userLocationFile.Load(
                    locPath,
                    ConfigNode.CreateConfigFromObject(new LocationFile()));
                userLocationFile.Locations = new List<Location>();
                Log.detail("location file {0} loaded = {1}", locPath, loaded);
                Add(userLocationFile);
            }
            else
            {
                Log.detail("Found {0} file", userLocationFile?.FilePath);
            }

            return userLocationFile;
        }

        /// <summary>
        /// Indicates whether or not this collection has a location with the given id.
        /// </summary>
        /// <param name="name">The name of the location to be found.</param>
        /// <returns>A value indicating whether the given id is a known location.</returns>
        internal bool HasLocation(string name)
        {
            foreach (LocationFile locationFile in this)
            {
                if (FindIndex(name, locationFile.Locations) != -1)
                {
                    return true;
                }
            }

            return false;
            // return this.Any(f => FindIndex(name, f.Locations) != -1);
        }

        /// <summary>
        /// Determines the locations (from a given list of locations) that are closest to the given target.
        /// </summary>
        /// <param name="targetLatitude">The target's latitude.</param>
        /// <param name="targetLongitude">The target's longitude.</param>
        /// <param name="targetAltitude">The target's altitude (ASL).</param>
        /// <param name="places">The population of locations to choose from.</param>
        /// <returns>
        /// An array of locations (one for each possible facility upgrade level) that are closest to
        /// the target coordinates.
        /// </returns>
        internal Locale[] FindClosest(double targetLatitude, double targetLongitude, double targetAltitude)
        {
            Locale[] closestLocale = new Locale[]
            {
                new Locale { },
                new Locale { Distance = double.MaxValue },
                new Locale { Distance = double.MaxValue },
                new Locale { Distance = double.MaxValue }
            };

            WorldCoordinates startpoint = new WorldCoordinates()
            {
                Latitude = targetLatitude,
                Longitude = targetLongitude,
                Altitude = targetAltitude,
                World = Homeworld,
            };

            foreach (Location location in _allLocations)
            {
                GreatCircle gc = new GreatCircle(startpoint, location.Coordinates);

                for (int level = 1; level < 4; level++)
                {
                    FacilityLevels facilityLevel = (FacilityLevels)(int)Math.Pow(2, level - 1);
                    bool locationIsAvailable = ((location.AvailableAtLevels & facilityLevel) != FacilityLevels.None);
                    if (locationIsAvailable && (gc.DistanceWithAltChange < closestLocale[level].Distance))
                    {
                        closestLocale[level] =
                            new Locale
                            {
                                Name = location.LocationName,
                                Horizontal = gc.DistanceAtDestAlt,
                                Vertical = gc.DeltaASL,
                                Distance = gc.DistanceWithAltChange,
                            };
                    }
                }
            }

            return closestLocale;
        }

        /// <summary>
        /// Obtains the current upgrade level of a facility.
        /// </summary>
        /// <param name="facilityName">The name of the facility.</param>
        /// <returns>A value indicating the current upgrade level.</returns>
        private static FacilityLevels GetFacilityLevel(string facilityName)
        {
            int levelCount = ScenarioUpgradeableFacilities.GetFacilityLevelCount(facilityName);
            PSystemSetup.SpaceCenterFacility facility = PSystemSetup.Instance.GetSpaceCenterFacility(facilityName.Split('/').Last());
            float rawLevel = facility.GetFacilityLevel();

            FacilityLevels level = (levelCount == 1) ? FacilityLevels.Level_3 : LevelConversion[rawLevel];
            //$"Facility {facilityName} levelCount={levelCount} raw level={rawLevel} level={level}".Debug();
            return level;
        }

        /// <summary>
        /// Creates a location based on the supplied user information and the currently active vessel.
        /// </summary>
        /// <param name="request">The user's requested location information.</param>
        /// <returns>A new location.</returns>
        private static Location CreateRequestedLocation(LocationRequest request)
        {
            WorldCoordinates requestCoordinates =
                new WorldCoordinates(
                    Homeworld,
                    FlightGlobals.ActiveVessel.latitude,
                    FlightGlobals.ActiveVessel.longitude,
                    FlightGlobals.ActiveVessel.altitude);
            GreatCircle displacement = new GreatCircle(GetCentrum().Coordinates, requestCoordinates);

            return new Location
            {
                LocationName = request.Name,
                FacilityName = request.AssociatedFacility,
                AvailableAtLevels = GetFacilityLevel(request.AssociatedFacility),
                Queueing = 0,
                ForwardAzimuth = displacement.ForwardAzimuth - GetCentrum().AngularOffset,
                Distance = displacement.DistanceAtOrigAlt / GetCentrum().HorizontalScale,
                DeltaAltitude = displacement.DeltaASL / GetCentrum().VerticalScale,
                Normal = FlightGlobals.ActiveVessel.terrainNormal,
                Rotation = FlightGlobals.ActiveVessel.transform.rotation,
            };
        }

        /// <summary>
        /// Loads all location files in the locFiles folder at the WalkAbout mod's installed path.
        /// </summary>
        private void LoadLocationFiles()
        {
            System.IO.DirectoryInfo di = new System.IO.DirectoryInfo($"{WalkAbout.GetModDirectory()}/{Constants.UserLocationSubdirectory}");
            foreach (System.IO.FileInfo file in di.GetFiles($"*.{LocationFileExtension}"))
            {
                LocationFile locationFile = new LocationFile();
                bool loaded = locationFile.Load(file.FullName); Log.detail("loading locations file {0} = {1}", file.FullName, loaded);
                if (loaded)
                {
                    Add(locationFile);
                }
                Log.info(locationFile.StatusMessage);
            }
            Log.detail("{0} location files loaded", Count);
        }

        /// <summary>
        /// Finds the index of a location (matching the given location name) within the supplied list
        /// of locations.
        /// </summary>
        /// <param name="name">The name of the location to be found.</param>
        /// <param name="targetList">The population of locations to be searched.</param>
        /// <returns>The index of the matching location (-1 if not found).</returns>
        private int FindIndex(string name, List<Location> targetList)
        {
            string searchName = name.ToUpper();

            for (int index = 0;index < targetList.Count;index++)
            {
                if (targetList[index].LocationName.ToUpper() == searchName)
                {
                    return index;
                }
            }

            return -1;
        }

        /// <summary>
        /// Compares two locations, ordering them by queueing and name.
        /// </summary>
        /// <param name="a">The base location.</param>
        /// <param name="b">The location to be compared to the base location.</param>
        /// <returns>A value indicating whether location b comes before or after location a.</returns>
        private int CompareLocations(Location a, Location b)
		{
            int queueOrder = (b?.Queueing ?? 0).CompareTo(a?.Queueing ?? 0);

            return queueOrder == 0
                ? string.Compare(a?.FacilityName + a?.LocationName, b?.FacilityName + b?.LocationName, StringComparison.Ordinal)
                : queueOrder;
        }

        /// <summary>
        /// Represents basic information about nearby locations.
        /// </summary>
        internal struct Locale
        {
            public string Name;
            public double Horizontal;
            public double Vertical;
            public double Distance;
        }
    }
}