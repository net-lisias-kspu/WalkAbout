/*
	This file is part of Walk About /L Unleashed
		© 2023 Lisias T : http://lisias.net <support@lisias.net>
		© 2016-2017 Antipodes (Clive Pottinger)

	Walk About /L Unleashed is licensed as follows:
		* GPL 3.0 : https://www.gnu.org/licenses/gpl-3.0.txt

	Walk About /L Unleashed is distributed in the hope that
	it will be useful, but WITHOUT ANY WARRANTY; without even the implied
	warranty of	MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.

	You should have received a copy of the GNU General Public License 3.0
	along with Walk About /L Unleashed. If not, see <https://www.gnu.org/licenses/>.

*/

using KspWalkAbout.Entities;
using KspWalkAbout.Extensions;
using KspWalkAbout.Guis;
using KspWalkAbout.WalkAboutFiles;
using UnityEngine;
using static KspAccess.CommonKspAccess;
using static KspWalkAbout.Entities.WalkAboutPersistent;

namespace KspWalkAbout
{
    /// <summary>
    /// Module to allow the user to add new locations for kerbals to be placed.
    /// </summary>
    [KSPAddon(KSPAddon.Startup.Flight, false)]
    public class WalkAboutAddUtility : MonoBehaviour
    {
        private WalkAboutSettings _config;
        private KnownPlaces _map;
        private AddUtilityGui _addUtilityGui;

        /// <summary>
        /// Called when the game is loaded. Used to set up all persistent objects and properties.
        /// </summary>
        public void Start()
        {
            if (!(FlightGlobals.ActiveVessel?.isEVA ?? false))
            {
                "Add Location utility deactivated: not an EVA".Debug();
                return;
            }
            var crew = FlightGlobals.ActiveVessel.GetVesselCrew();
            if ((crew?.Count ?? 0) != 1)
            {
                "Add Location utility deactivated: invalid crew count".Debug();
                return;
            }

            _config = GetModConfig(); "Add Location utility obtained config".Debug();
            if (_config == null) { return; }

            if (_config.Mode != "utility")
            {
                "Add Location utility deactivated: not in utility mode".Debug();
                return;
            }

            $"Add Location utility activated on EVA for {FlightGlobals.ActiveVessel.GetVesselCrew()[0].name}".Debug();

            _map = GetLocationMap(); "Add Location utility obtained map object".Debug();
            _addUtilityGui = AddUtilityGui.Instance;
        }

        /// <summary>
        /// Called each time the game state is updated.
        /// </summary>
        public void Update()
        {
            if (_addUtilityGui == null)
            {
                return;
            }

            if (CheckForModUtilityActivation())
            {
                _addUtilityGui.GuiCoordinates = GuiResizer.HandleResizing(_addUtilityGui.GuiCoordinates);
                SaveFiles();
            }

            if (!Input.GetMouseButton(0))
            {
                SaveFiles();
            }
        }

        /// <summary>
        /// Called each time the game's GUIs are to be refreshed.
        /// </summary>
        public void OnGUI()
        {
            _addUtilityGui?.Display();

            if (_addUtilityGui?.RequestedLocation == null) return;

            $"Request for new location {_addUtilityGui.RequestedLocation.Name} detected".Debug();
            _map.AddLocation(_addUtilityGui.RequestedLocation);
            _addUtilityGui.RequestedLocation = null;
        }

        /// <summary>
        /// Determines if the user has requested the WalkAbout mod's utility GUI.
        /// </summary>
        private bool CheckForModUtilityActivation()
        {
            var wasActive = _addUtilityGui.IsActive;
            _addUtilityGui.IsActive |= IsKeyCombinationPressed(_config.AUActivationHotKey, _config.AUActivationHotKeyModifiers);

            if (wasActive != _addUtilityGui.IsActive)
            {
                _map.RefreshLocations();
            }

            return _addUtilityGui.IsActive;
        }

        /// <summary>
        /// Saves all settings files with pending changes.
        /// </summary>
        private void SaveFiles()
        {
            if (_map.IsChanged)
            {
                "Saving map changes".Debug();
                _map.Save();
            }
        }
    }
}