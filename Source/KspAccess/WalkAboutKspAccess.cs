using KspWalkAbout.Entities;
using KspWalkAbout.Extensions;
using System;
using static KspAccess.CommonKspAccess;

namespace KspAccess
{
    /// <summary>Represents internals used by the WalkAbout mod to manipulate KSP's game state.</summary>
    internal static class WalkAboutKspAccess
    {
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

            // adjust the location and orientation of the ship/kerbal
            vesselNode.SetValue("sit", "LANDED");
            vesselNode.SetValue("landed", "True");
            vesselNode.SetValue("splashed", "False");
            vesselNode.SetValue("lat", request.Location.Latitude.ToString());
            vesselNode.SetValue("lon", request.Location.Longitude.ToString());
            vesselNode.SetValue("alt", request.Location.Altitude.ToString());
            vesselNode.SetValue("hgt", "0.28");
            vesselNode.SetValue("nrm", $"{request.Location.Normal.x},{request.Location.Normal.y},{request.Location.Normal.z}");
            vesselNode.SetValue("rot", $"{request.Location.Rotation.x},{request.Location.Rotation.y},{request.Location.Rotation.z},{request.Location.Rotation.w}");
            "adjusted vesselNode".Debug();

            // add the new ship/kerbal to the game
            HighLogic.CurrentGame.AddVessel(vesselNode);
            request.Kerbal.rosterStatus = ProtoCrewMember.RosterStatus.Assigned;
            "Added kerbalEvaNode to CurrentGame".Debug();

            $"{request.Kerbal.name} is being placed at {request.Location.LocationName}".Log();
            ScreenMessages.PostScreenMessage(new ScreenMessage($"{request.Kerbal.name} is being placed at {request.Location.LocationName}", 4.0f, ScreenMessageStyle.UPPER_LEFT));
        }
    }
}
