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

using KspAccess;
using KspWalkAbout.Entities;
using KspWalkAbout.Guis;
using KspWalkAbout.Values;
using KspWalkAbout.WalkAboutFiles;
using UnityEngine;
using Upgradeables;
using static KspAccess.CommonKspAccess;
using static KspWalkAbout.Entities.WalkAboutPersistent;

namespace KspWalkAbout
{
    /// <summary>
    /// Module to allow user to place kerbals at specific locations.
    /// </summary>
    [KSPAddon(KSPAddon.Startup.SpaceCentre, false)]
    public class WalkAbout : MonoBehaviour
    {
        private WalkAboutSettings _config;
        private KnownPlaces _map;
        private InventoryItems _items;
        private static PlaceKerbalGui _mainGui;

        /// <summary>
        /// Called when the game is loaded. Used to set up all persistent objects and properties.
        /// </summary>
        public void Start()
        {
            Log.info("Started [Version={0}, Debug={1}]", Constants.Version, Log.isDebug);

            _config = GetModConfig(); Log.detail("obtained config");
            if (_config == null)
            {
                return;
            }

            _map = GetLocationMap(); Log.detail("obtained map object");
            _map.RefreshLocations(); // needed to avoid holding on to other games' data 
            _items = GetAllItems(); Log.detail("obtained items object");

            _mainGui = PlaceKerbalGui.Instance;
            _mainGui.GuiCoordinates = _config.GetScreenPosition();
            _mainGui.TopFew = _config.TopFew;
            _mainGui.MaxItems = _config.MaxInventoryItems;
            _mainGui.MaxVolume = _config.MaxInventoryVolume;
            Log.detail("created MainGui object");

            GetCentrum(); // Initialize the centrum to avoid errors if AddUtility is opened before WalkAbout.

            GameEvents.OnTechnologyResearched.Remove(ItemRefresh);
            GameEvents.OnKSCFacilityUpgraded.Remove(MapRefresh);
            GameEvents.OnTechnologyResearched.Add(ItemRefresh);
            GameEvents.OnKSCFacilityUpgraded.Add(MapRefresh);
        }

        /// <summary>
        /// Called each time the game state is updated.
        /// </summary>
        public void Update()
        {
            if (_mainGui == null)
            {
                return;
            }

            if (CheckForModActivation())
            {
                _mainGui.GuiCoordinates = GuiResizer.HandleResizing(_mainGui.GuiCoordinates);
                SaveFiles();
            }
        }

        /// <summary>
        /// Called each time the game's GUIs are to be refreshed.
        /// </summary>
        public void OnGUI()
        {
            if (_mainGui?.Display() ?? false)
            {
                _config.SetScreenPosition(_mainGui.GuiCoordinates);
            }

            if (_mainGui?.RequestedPlacement == null) return;

            Log.detail("placing kerbal");
            WalkAboutKspAccess.PlaceKerbal(_mainGui.RequestedPlacement);
            Log.info("Saving game");
            GamePersistence.SaveGame("persistent", HighLogic.SaveFolder, SaveMode.OVERWRITE);

            PerformPostPlacementAction();

            _items.UpdateQueueing(_mainGui.RequestedPlacement.Items);
            _map.UpdateQueuing(_mainGui.RequestedPlacement.Location.LocationName);
            _mainGui.RequestedPlacement = null;
        }

        /// <summary>
        /// Obtains the directory where the WalkAbout mod is currently installed.
        /// </summary>
        /// <returns>Returns a directoy path.</returns>
        internal static string GetModDirectory()
        {
            return CommonKspAccess.GetModDirectory(Constants.ModName);
        }

        /// <summary>
        /// Finds a vessel in the game.
        /// </summary>
        /// <param name="name">The name of vessel.</param>
        private static Vessel FindVesselByName(string name)
        {
            Vessel found = null;
            string searchName = $"{name} (unloaded)";

            foreach (Vessel vessel in FlightGlobals.Vessels)
            {
                if ((vessel.name == name) || (vessel.name == searchName))
                {
                    found = vessel;
                    break;
                }
            }
            //found = FlightGlobals.Vessels.Find(v => (v.name == name) || (v.name == searchName));

            return found;
        }

        /// <summary>
        /// Reloads the collection of items that can used by kerbals.
        /// </summary>
        private void ItemRefresh(GameEvents.HostTargetAction<RDTech, RDTech.OperationResult> data)
        {
            if (data.target == RDTech.OperationResult.Successful)
            {
                Log.detail("Refreshing items due to new technologies");
                _items.RefreshItems();
            }
        }

        /// <summary>
        /// Reloads the collection of places where kerbals can be placed.
        /// </summary>
        private void MapRefresh(UpgradeableFacility data0, int data1)
        {
            Log.detail("Refreshing map due to new facility upgrade");
            _map.RefreshLocations();
        }

        /// <summary>
        /// Determines if the user has requested the WalkAbout mod's GUI.
        /// </summary>
        private bool CheckForModActivation()
        {
            _mainGui.IsActive |= IsKeyCombinationPressed(_config.ActivationHotKey, _config.ActivationHotKeyModifiers);
            return _mainGui.IsActive;
        }

        /// <summary>
        /// Saves all settings files with pending changes.
        /// </summary>
        private void SaveFiles()
        {
            if (_config.IsChanged && !GuiResizer.IsResizing && !Input.GetMouseButton(0))
            {
                _config.Save();
                Log.info("saved settings to {0}", _config.FilePath);
            }

            if (_map.IsChanged)
            {
                _map.Save();
            }

            if (_items.IsChanged)
            {
                _items.Save();
            }
        }

        private void PerformPostPlacementAction()
        {
            switch (_config.PostPlacementAction)
            {
                case PostPlacementMode.noreload:
                    Log.info("Suppressed reload of SPACECENTER");
                    break;

                case PostPlacementMode.jumpto:
                    Vessel vessel = FindVesselByName(_mainGui.RequestedPlacement.Kerbal.name);
                    if (vessel == null)
                    {
                        Log.info("Unable to jump to vessel - no vessel found");
                    }
                    else
                    {
                        Log.info("Loading Flight scene for {0}", vessel.name);
                        FlightDriver.StartAndFocusVessel("persistent", FlightGlobals.Vessels.IndexOf(vessel));
                    }
                    break;

                case PostPlacementMode.reload:
                default:
                    Log.info("Reloading SPACECENTER");
                    HighLogic.LoadScene(GameScenes.SPACECENTER);
                    break;
            }
        }
    }
}