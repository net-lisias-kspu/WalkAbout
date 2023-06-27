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

using KspWalkAbout.Entities;
using System.Collections.Generic;
using System.Reflection;
using static KspAccess.CommonKspAccess;
using KspWalkAbout;

namespace KspAccess
{
    /// <summary>Represents internals used by the WalkAbout mod to manipulate KSP's game state.</summary>
    internal static class WalkAboutKspAccess
    {
        private static Assembly _KisMod = null;
        private static bool _isKisModChecked = false;
        private static bool _isKisModPresent = false;

        /// <summary>
        /// Adds a kerbal to the game the requested location.
        /// </summary>
        internal static void PlaceKerbal(PlacementRequest request)
        {
            Log.detail("{0} will be placed outside {1}", request.Kerbal.name, request.Location.LocationName);
            Log.detail("placement lat:{0} long:{1} alt:{2}", request.Location.Coordinates.Latitude, request.Location.Coordinates.Longitude, request.Location.Coordinates.Altitude);
            Orbit orbit = CreateOrbitForKerbal(request);
            ConfigNode vesselNode = CreateVesselNode(request, orbit);

            SetVesselLocation(request, vesselNode);
            AddVesselToGame(request, vesselNode);

            AllocateInventoryItems(request);

            AllocateEvaResources(vesselNode);
        }

        /// <summary>
        /// Determines if the Kerbal Inventory System mod has been installed.
        /// </summary>
        internal static bool IsKisModDetected()
        {
            if (!_isKisModChecked)
            {
                _isKisModPresent = IsModInstalled("KIS");
                if (_isKisModPresent)
                {
                    _KisMod = GetMod("KIS");
                    Log.info("obtained KIS mod assembly [{0}]", _KisMod);
                }
                else
                {
                    Log.info("KIS mod not detected");
                }
                _isKisModChecked = true;
            }

            return _isKisModPresent;
        }

        /// <summary>
        /// Obtains the Assembly for the Kerbal Inventory System if it has been installed.
        /// </summary>
        internal static bool TryGetKisMod(ref Assembly mod)
        {
            bool isModPresent = IsKisModDetected();
            mod = _KisMod;

            return isModPresent;
        }

        private static Orbit CreateOrbitForKerbal(PlacementRequest request)
        {
            Vector3d pos =
                Homeworld.GetWorldSurfacePosition(
                    request.Location.Coordinates.Latitude,
                    request.Location.Coordinates.Longitude,
                    request.Location.Coordinates.Altitude);
            Orbit orbit = new Orbit(0, 0, 0, 0, 0, 0, 0, Homeworld);
            orbit.UpdateFromStateVectors(pos, Homeworld.getRFrmVel(pos), Homeworld, Planetarium.GetUniversalTime());
            Log.detail("created orbit for {0}", Homeworld.name);
            return orbit;
        }

        private static ConfigNode CreateVesselNode(PlacementRequest request, Orbit orbit)
        {
            // create an id for the flight object that will represent the kerbal's EVA
            string genderQualifier = request.Kerbal.gender == ProtoCrewMember.Gender.Female ? "female" : string.Empty;
            uint flightId = ShipConstruction.GetUniqueFlightID(HighLogic.CurrentGame.flightState);
            Log.detail("created flightId {0}", flightId);

            // create a ship consisting of just the kerbal - this is how EVAs are represented in KSP
            ConfigNode[] partNodes = new ConfigNode[1];
            partNodes[0] =
                ProtoVessel.CreatePartNode($"kerbalEVA{genderQualifier}", flightId, request.Kerbal);
            Log.detail("created partNodes");
            ConfigNode vesselNode = ProtoVessel.CreateVesselNode(request.Kerbal.name, VesselType.EVA, orbit, 0, partNodes);
            Log.detail("created vesselNode");
            return vesselNode;
        }

        private static void SetVesselLocation(PlacementRequest request, ConfigNode vesselNode)
        {
            vesselNode.SetValue("sit", Vessel.Situations.LANDED.ToString());
            vesselNode.SetValue("landed", true.ToString());
            vesselNode.SetValue("splashed", false.ToString());
            vesselNode.SetValue("lat", request.Location.Coordinates.Latitude.ToString());
            vesselNode.SetValue("lon", request.Location.Coordinates.Longitude.ToString());
            vesselNode.SetValue("alt", request.Location.Coordinates.Altitude.ToString());
            vesselNode.SetValue("hgt", "0.28");
            vesselNode.SetValue("nrm", $"{request.Location.Normal.x},{request.Location.Normal.y},{request.Location.Normal.z}");
            vesselNode.SetValue("rot", $"{request.Location.Rotation.x},{request.Location.Rotation.y},{request.Location.Rotation.z},{request.Location.Rotation.w}");
            Log.detail("adjusted vesselNode location");
        }

        private static void AddVesselToGame(PlacementRequest request, ConfigNode vesselNode)
        {
            Log.detail("{0} is being placed at {1}", request.Kerbal.name, request.Location.LocationName);
            ScreenMessages.PostScreenMessage(new ScreenMessage($"{request.Kerbal.name} is being placed at {request.Location.LocationName}", 4.0f, ScreenMessageStyle.UPPER_LEFT));
            HighLogic.CurrentGame.AddVessel(vesselNode);
            request.Kerbal.rosterStatus = ProtoCrewMember.RosterStatus.Assigned;
            HighLogic.CurrentGame.CrewRoster[request.Kerbal.name].rosterStatus = ProtoCrewMember.RosterStatus.Assigned;
        }

        private static void AllocateInventoryItems(PlacementRequest request)
        {
            WalkAboutPersistent.AllocatedItems.Remove(request.Kerbal.name);
            if (request.Items.Count > 0)
            {
                WalkAboutPersistent.AllocatedItems.Add(request.Kerbal.name, new List<string>());
                foreach (InventoryItem item in request.Items)
                {
                    AllocateInventoryItem(request.Kerbal.name, item);
                }
            }
        }

        private static void AllocateInventoryItem(string name, InventoryItem item)
        {
            Log.detail("Recording that {0} is to be added to {1}'s inventory", item.Title, name);
            WalkAboutPersistent.AllocatedItems[name].Add(item.Name);
            if (Funding.Instance != null)
            {
                Log.detail("Subtracting {0} funds for inventory items", item.Cost * -1);
                Funding.Instance.AddFunds((double)item.Cost, TransactionReasons.Vessels);
            }
        }

        private static void AllocateEvaResources(ConfigNode vesselNode)
        {
            ConfigNode[] partNodes = vesselNode.GetNodes("PART");
            if ((partNodes != null) && (partNodes[0] != null))
            {
                ConfigNode[] resourceNodes = partNodes[0].GetNodes("RESOURCE");

                foreach (ConfigNode resourceNode in resourceNodes)
                {
                    string resourceName = resourceNode.GetValue("name");
                    if (WalkAboutPersistent.EvaResources.Contains(resourceName))
                    {
                        resourceNode.SetValue("amount", resourceNode.GetValue("maxAmount"), true);
                        Log.detail("Setting resource [{0}] amount to maximum", resourceName);
                    }
                }
            }
        }
    }
}