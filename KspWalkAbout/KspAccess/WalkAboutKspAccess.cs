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

using KspWalkAbout.Entities;
using KspWalkAbout.Extensions;
using System.Collections.Generic;
using System.Reflection;
using static KspAccess.CommonKspAccess;

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
            $"{request.Kerbal.name} will be placed outside {request.Location.LocationName}".Debug();
            $"placement lat:{request.Location.Coordinates.Latitude} long:{request.Location.Coordinates.Longitude} alt:{request.Location.Coordinates.Altitude}".Debug();
            var orbit = CreateOrbitForKerbal(request);
            var vesselNode = CreateVesselNode(request, orbit);

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
                    $"obtained KIS mod assembly [{_KisMod}]".Debug();
                }
                else
                {
                    "KIS mod not detected".Debug();
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
            var isModPresent = IsKisModDetected();
            mod = _KisMod;

            return isModPresent;
        }

        private static Orbit CreateOrbitForKerbal(PlacementRequest request)
        {
            var pos =
                Homeworld.GetWorldSurfacePosition(
                    request.Location.Coordinates.Latitude,
                    request.Location.Coordinates.Longitude,
                    request.Location.Coordinates.Altitude);
            var orbit = new Orbit(0, 0, 0, 0, 0, 0, 0, Homeworld);
            orbit.UpdateFromStateVectors(pos, Homeworld.getRFrmVel(pos), Homeworld, Planetarium.GetUniversalTime());
            $"created orbit for {Homeworld.name}".Debug();
            return orbit;
        }

        private static ConfigNode CreateVesselNode(PlacementRequest request, Orbit orbit)
        {
            // create an id for the flight object that will represent the kerbal's EVA
            var genderQualifier = request.Kerbal.gender == ProtoCrewMember.Gender.Female ? "female" : string.Empty;
            var flightId = ShipConstruction.GetUniqueFlightID(HighLogic.CurrentGame.flightState);
            $"created flightId {flightId}".Debug();

            // create a ship consisting of just the kerbal - this is how EVAs are represented in KSP
            var partNodes = new ConfigNode[1];
            partNodes[0] =
                ProtoVessel.CreatePartNode($"kerbalEVA{genderQualifier}", flightId, request.Kerbal);
            "created partNodes".Debug();
            var vesselNode = ProtoVessel.CreateVesselNode(request.Kerbal.name, VesselType.EVA, orbit, 0, partNodes);
            "created vesselNode".Debug();
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
            "adjusted vesselNode location".Debug();
        }

        private static void AddVesselToGame(PlacementRequest request, ConfigNode vesselNode)
        {
            $"{request.Kerbal.name} is being placed at {request.Location.LocationName}".Log();
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
                foreach (var item in request.Items)
                {
                    AllocateInventoryItem(request.Kerbal.name, item);
                }
            }
        }

        private static void AllocateInventoryItem(string name, InventoryItem item)
        {
            $"Recording that {item.Title} is to be added to {name}'s inventory".Debug();
            WalkAboutPersistent.AllocatedItems[name].Add(item.Name);
            if (Funding.Instance != null)
            {
                $"Subtracting {item.Cost * -1} funds for inventory items".Debug();
                Funding.Instance.AddFunds((double)item.Cost, TransactionReasons.Vessels);
            }
        }

        private static void AllocateEvaResources(ConfigNode vesselNode)
        {
            var partNodes = vesselNode.GetNodes("PART");
            if ((partNodes != null) && (partNodes[0] != null))
            {
                var resourceNodes = partNodes[0].GetNodes("RESOURCE");

                foreach (var resourceNode in resourceNodes)
                {
                    var resourceName = resourceNode.GetValue("name");
                    if (WalkAboutPersistent.EvaResources.Contains(resourceName))
                    {
                        resourceNode.SetValue("amount", resourceNode.GetValue("maxAmount"), true);
                        $"Setting resource [{resourceName}] amount to maximum".Debug();
                    }
                }
            }
        }
    }
}