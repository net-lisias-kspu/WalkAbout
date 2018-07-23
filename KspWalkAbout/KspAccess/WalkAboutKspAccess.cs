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

            // create an orbit around Kerbin located at the desired location
            var pos = CommonKspAccess.Kerbin.GetWorldSurfacePosition(request.Location.Latitude, request.Location.Longitude, request.Location.Altitude);
            var orbit = new Orbit(0, 0, 0, 0, 0, 0, 0, CommonKspAccess.Kerbin);
            orbit.UpdateFromStateVectors(pos, CommonKspAccess.Kerbin.getRFrmVel(pos), CommonKspAccess.Kerbin, Planetarium.GetUniversalTime());
            $"created orbit for {CommonKspAccess.Kerbin.name}".Debug();

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

            // set the location and orientation of the ship/kerbal
            vesselNode.SetValue("sit", Vessel.Situations.LANDED.ToString());
            vesselNode.SetValue("landed", true.ToString());
            vesselNode.SetValue("splashed", false.ToString());
            vesselNode.SetValue("lat", request.Location.Latitude.ToString());
            vesselNode.SetValue("lon", request.Location.Longitude.ToString());
            vesselNode.SetValue("alt", request.Location.Altitude.ToString());
            vesselNode.SetValue("hgt", "0.28");
            vesselNode.SetValue("nrm", $"{request.Location.Normal.x},{request.Location.Normal.y},{request.Location.Normal.z}");
            vesselNode.SetValue("rot", $"{request.Location.Rotation.x},{request.Location.Rotation.y},{request.Location.Rotation.z},{request.Location.Rotation.w}");
            "adjusted vesselNode".Debug();

            // add the new ship/kerbal to the game
            $"{request.Kerbal.name} is being placed at {request.Location.LocationName}".Log();
            ScreenMessages.PostScreenMessage(new ScreenMessage($"{request.Kerbal.name} is being placed at {request.Location.LocationName}", 4.0f, ScreenMessageStyle.UPPER_LEFT));
            HighLogic.CurrentGame.AddVessel(vesselNode);
            request.Kerbal.rosterStatus = ProtoCrewMember.RosterStatus.Assigned;
            HighLogic.CurrentGame.CrewRoster[request.Kerbal.name].rosterStatus = ProtoCrewMember.RosterStatus.Assigned;

            // Record the items for the kerbals's inventory. Reduce funds accordingly
            // Items will be added to the kerbal's inventory when the EVA scene is activated.
            WalkAboutPersistent.AllocatedItems.Remove(request.Kerbal.name);
            if (request.Items.Count > 0)
            {
                var itemNames = new List<string>();
                var cost = 0f;
                foreach (var item in request.Items)
                {
                    $"Recording that {item.Title} is to be added to {request.Kerbal.name}'s inventory".Debug();
                    itemNames.Add(item.Name);
                    cost -= item.Cost;
                }
                WalkAboutPersistent.AllocatedItems.Add(request.Kerbal.name, itemNames);
                if (Funding.Instance != null)
                {
                    $"Subtracting {cost * -1} funds for inventory items".Debug();
                    Funding.Instance.AddFunds((double)cost, TransactionReasons.Vessels);
                }
            }
        }

        /// <summary>
        /// Determines if the Kerbal Inventory System has been installed.
        /// </summary>
        internal static bool DetectKisMod()
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
        internal static bool DetectKisMod(ref Assembly mod)
        {
            var isModPresent = DetectKisMod();
            mod = _KisMod;

            return isModPresent;
        }
    }
}
