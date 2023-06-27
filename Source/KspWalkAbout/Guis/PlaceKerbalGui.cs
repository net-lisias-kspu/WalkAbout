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
using KspWalkAbout.Values;
using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using static KspWalkAbout.Entities.WalkAboutPersistent;

namespace KspWalkAbout.Guis
{
    /// <summary>Represents the GUI used to place kerbals at locations.</summary>
    internal class PlaceKerbalGui
    {
        private static readonly PlaceKerbalGui _instance = new PlaceKerbalGui();

        private Rect _coordinates;
        private Vector2 _kerbalSelectorScrollPosition;
        private Vector2 _facilitySelectorScrollPosition;
        private Vector2 _locationSelectorScrollPosition;
        private Vector2 _itemsSelectorScrollPosition;
        private Vector2 _itemsSelectedScrollPosition;
        private ProtoCrewMember _selectedKerbal;
        private string _selectedFacility;
        private Location _selectedLocation;
        private GuiElementStyles _elementStyles;
        private bool _showTopFewOnly;
        private string _windowTitle;
        private Vector2 _minGuiSize;
        private bool _isKisPresent;
        private Assembly _KisMod;
        private Dictionary<string, List<InventoryItem>> _itemsSelected;
        private bool _itemSelectorIsOpen;

        static PlaceKerbalGui() { }

        /// <summary>Initializes a new instance of the MainGui class.</summary>
        internal PlaceKerbalGui()
        {
            _kerbalSelectorScrollPosition = Vector2.zero;
            _facilitySelectorScrollPosition = Vector2.zero;
            _locationSelectorScrollPosition = Vector2.zero;
            _itemsSelectorScrollPosition = Vector2.zero;
            _itemsSelectedScrollPosition = Vector2.zero;

            _isKisPresent = WalkAboutKspAccess.TryGetKisMod(ref _KisMod);
            _itemsSelected = new Dictionary<string, List<InventoryItem>>();

            _windowTitle = $"{Constants.ModName} v{Constants.Version}";
            _minGuiSize = new Vector2(200, 50);
            _itemSelectorIsOpen = false;

            IsActive = false;
        }

        internal static PlaceKerbalGui Instance { get { return _instance; } }

        /// <summary>Gets or sets the screen coordinates and size of the GUI.</summary>
        internal Rect GuiCoordinates
        {
            get { return _coordinates; }
            set
            {
                if (value == new Rect()) Log.detail("given empty screen position - defaulting to 1/4 screen");
                _coordinates = (value == new Rect())
                    ? new Rect(Screen.width / 4, Screen.height / 4, Screen.width / 3, Screen.height / 4)
                    : value;
            }
        }

        /// <summary>Gets or sets a value indicating whether the main GUI is displayed and usable.</summary>
        internal bool IsActive { get; set; }

        /// <summary>
        /// Gets or sets the max number of locations to display in the GUI as the most likely locations the user wants to select from.
        /// </summary>
        public int TopFew { get; internal set; }

        /// <summary>Gets or sets the maximum number of items that can be assigned to a kerbal's inventory.</summary>
        public int MaxItems { get; internal set; }

        /// <summary>
        /// Gets or sets the maximum volume that all items that can be assigned to a kerbal's inventory may occupy.
        /// </summary>
        public float MaxVolume { get; internal set; }

        /// <summary>Gets or sets the user's current request to place a kerbal at a location.</summary>
        public PlacementRequest RequestedPlacement { get; set; }

        /// <summary>Called regularly to draw the GUI on screen.</summary>
        /// <returns>A value indicating whether or not the GUI was displayed.</returns>
        internal bool Display()
        {
            if (!IsActive) return false;
            if (_elementStyles == null) _elementStyles = new GuiElementStyles();

            GuiCoordinates = GUI.Window(0, GuiCoordinates, DrawSelectorHandler, _windowTitle);
            return true;
        }

        /// <summary>Draws the GUI on screen and handles user selections.</summary>
        /// <param name="id">Required parameter: purpose unknown.</param>
        private void DrawSelectorHandler(int id)
        {
            bool haveKerbals = false;
            bool haveLocations = false;

            GUILayout.BeginHorizontal();
            {
                GUILayout.BeginVertical();
                {
                    GUILayout.BeginHorizontal();
                    {
                        haveKerbals = DrawKerbalSelector();
                        DrawFacilitySelector();
                        haveLocations = DrawLocationSelector();
                    }
                    GUILayout.EndHorizontal();
                    DrawAddItemsButton();
                    DrawActionButton(haveKerbals, haveLocations);
                    DrawCancelButton();
                }
                GUILayout.EndVertical();
                DrawItemSelection();
            }
            GUILayout.EndHorizontal();

            DrawDebugButton(_coordinates);
            GuiResizer.DrawResizingButton(_coordinates, _minGuiSize);
            GUI.DragWindow(new Rect(0, 0, GuiCoordinates.width, GuiCoordinates.height));
        }

        /// <summary>Draws the portion of the GUI that displays available kerbals and handles user selection.</summary>
        /// <returns>A value indicating whether any buttons for selecting kerbals were drawn.</returns>
        private bool DrawKerbalSelector()
        {
            bool drewKerbalButtons = false;
            _kerbalSelectorScrollPosition = GUILayout.BeginScrollView(_kerbalSelectorScrollPosition);
            {
                GUILayout.BeginVertical();
                {
                    foreach (ProtoCrewMember crew in HighLogic.CurrentGame.CrewRoster.Crew)
                    {
                        if (crew.rosterStatus == ProtoCrewMember.RosterStatus.Available)
                        {
                            GUIStyle buttonStyle = (crew.name == ((_selectedKerbal?.name) ?? string.Empty))
                                ? _elementStyles.SelectedButton
                                : _elementStyles.ValidButton;
                            if (GUILayout.Button(crew.name, buttonStyle))
                            {
                                _selectedKerbal = crew; Log.detail("selected {0}", _selectedKerbal.name);
                                if (!_itemsSelected.ContainsKey(_selectedKerbal.name))
                                {
                                    _itemsSelected.Add(_selectedKerbal.name, new List<InventoryItem>());
                                }
                            }
                            drewKerbalButtons = true;
                        }
                    }
                }
                GUILayout.EndVertical();
            }
            GUILayout.EndScrollView();
            return drewKerbalButtons;
        }

        /// <summary>Draws the portion of the GUI that displays available facilities and handles user selection.</summary>
        private void DrawFacilitySelector()
        {
            _facilitySelectorScrollPosition = GUILayout.BeginScrollView(_facilitySelectorScrollPosition);
            {
                GUILayout.BeginVertical();
                {
                    foreach (KeyValuePair<string, FacilityLevels> facility in GetLocationMap().AvailableFacilitiesLevel)
                    {
                        string buttonText = facility.Key.Substring(facility.Key.IndexOf('/') + 1) +
                                         (Log.isDebug ? $" ({facility.Value})" : string.Empty);
                        GUIStyle buttonStyle = (facility.Key == (_selectedFacility ?? string.Empty))
                            ? _elementStyles.SelectedButton
                            : _elementStyles.ValidButton;
                        if (GUILayout.Button(buttonText, buttonStyle))
                        {
                            _selectedFacility = (facility.Key == _selectedFacility) ? null : facility.Key;
                            Log.detail("selected {0}", _selectedFacility);
                        }
                    }
                }
                GUILayout.EndVertical();
            }
            GUILayout.EndScrollView();
        }

        /// <summary>Draws the portion of the GUI that displays available locations and handles user selection.</summary>
        /// <returns>A value indicating whether any buttons for selecting locations were drawn.</returns>
        private bool DrawLocationSelector()
        {
            int locationButtonsDrawn = 0;

            GUILayout.BeginVertical();
            {
                _showTopFewOnly = ((TopFew > 0) && (GetLocationMap().AvailableLocations.Count > TopFew))
                    ? (GUILayout.Toggle(_showTopFewOnly, $"Top {TopFew} Only"))
                    : false;

                _locationSelectorScrollPosition = GUILayout.BeginScrollView(_locationSelectorScrollPosition);
                {
                    GUILayout.BeginVertical();
                    {
                        foreach (Location location in GetLocationMap().AvailableLocations)
                        {
                            if (string.IsNullOrEmpty(_selectedFacility) || (location.FacilityName == _selectedFacility))
                            {
                                GUIStyle buttonStyle =
                                    (location.LocationName == ((_selectedLocation?.LocationName) ?? string.Empty))
                                    ? _elementStyles.SelectedButton
                                    : _elementStyles.ValidButton;
                                if (GUILayout.Button(location.LocationName, buttonStyle))
                                {
                                    _selectedLocation = location;
                                    Log.detail("selected {0}", _selectedLocation.LocationName);
                                }
                                if ((++locationButtonsDrawn == TopFew) && _showTopFewOnly) break;
                            }
                        }
                    }
                    GUILayout.EndVertical();
                }
                GUILayout.EndScrollView();
            }
            GUILayout.EndVertical();
            return locationButtonsDrawn > 0;
        }

        /// <summary>Draws the portion of the GUI that displays the user's confimation of action button and handles its selection.</summary>
        /// <param name="haveKerbals">A value indicating whether any buttons for selecting a kerbal have been drawn.</param>
        /// <param name="haveLocations">A value indicating whether any buttons for selecting a location have been drawn.</param>
        private void DrawActionButton(bool haveKerbals, bool haveLocations)
        {
            if (haveKerbals && haveLocations)
            {
                if (_selectedKerbal == null || _selectedLocation == null)
                {
                    GUILayout.Button(string.Format(
                        "Select a {0}{1}",
                        _selectedKerbal == null ? "kerbal" : "location",
                        _selectedKerbal == null && _selectedLocation == null ? " and a location" : string.Empty),
                        _elementStyles.InvalidButton);
                }
                else
                {
                    if (GUILayout.Button($"Place {_selectedKerbal.name} outside {_selectedLocation.LocationName}", _elementStyles.SelectedButton))
                    {
                        RequestedPlacement =
                            new PlacementRequest
                            {
                                Kerbal = _selectedKerbal,
                                Location = _selectedLocation,
                                Items = _itemsSelected[_selectedKerbal.name],
                            };
                        Log.detail("PlacementRequest created - deactivating GUI");
                        IsActive = false;
                        _selectedKerbal = null;
                    }
                }
            }
            else
            {
                GUILayout.Button(haveKerbals ? "No locations found" : "No available kerbals", _elementStyles.InvalidButton);
            }
        }

        /// <summary>Draws the button to add or change the items in the selected kerbal's inventory.</summary>   
        private void DrawAddItemsButton()
        {
            if (_isKisPresent && (_selectedKerbal != null))
            {
                string prefix = (_itemSelectorIsOpen)
                    ? "Close"
                    : (_itemsSelected[_selectedKerbal.name].Count == 0) ? "Add items to" : "Change";

                if (GUILayout.Button($"{prefix} {_selectedKerbal.name}'s inventory", _elementStyles.SelectedButton))
                {
                    _itemSelectorIsOpen = !_itemSelectorIsOpen;
                }
            }
        }

        /// <summary>Draws the button to cancel the user's action and handles its selection.</summary>
        private void DrawCancelButton()
        {
            if (GUILayout.Button("Cancel"))
            {
                _selectedKerbal = null;
                _selectedLocation = null;
                IsActive = false;
            }
        }

        /// <summary>Draws the buttons to select specific items to add or remove from the selected kerbal's inventory.</summary>
        private void DrawItemSelection()
        {
            if (_itemSelectorIsOpen && (_selectedKerbal != null))
            {
                GUILayout.BeginVertical();
                {
                    int numItems = _itemsSelected[_selectedKerbal.name].Count;
                    int itemsRemaining = MaxItems - numItems;
                    float volumeRemaining = MaxVolume;

                    double fundsRemaining = double.MaxValue;
                    if (Funding.Instance != null)
                    {
                        fundsRemaining = Funding.Instance.Funds;
                        foreach (string kerbal in _itemsSelected.Keys)
                        {
                            foreach (InventoryItem item in _itemsSelected[kerbal])
                            {
                                fundsRemaining -= item.Cost;
                            }
                        }
                    }

                    if (numItems > 0)
                    {
                        string pluralization = (numItems == 1) ? string.Empty : "s";
                        float cost = 0f;
                        foreach (InventoryItem item in _itemsSelected[_selectedKerbal.name])
                        {
                            cost += item.Cost;
                        }
                        fundsRemaining -= cost;
                        string costText = (Funding.Instance == null) ? string.Empty : $", {Math.Round(cost, 2, MidpointRounding.ToEven)} funds";
                        GUILayout.Label($"Selected: {numItems} item{pluralization}{costText}");
					}

                    foreach (InventoryItem item in _itemsSelected[_selectedKerbal.name].ToArray())
                    {
                        volumeRemaining -= item.Volume;

                        if (GUILayout.Button($"{item.Title}", _elementStyles.SelectedButton))
                        {
                            _itemsSelected[_selectedKerbal.name].Remove(item);
                            volumeRemaining += item.Volume;
                        }
                    }

                    {
                        string pluralization = (itemsRemaining == 1) ? string.Empty : "s";
                        GUILayout.Label($"Remaining: {itemsRemaining} item{pluralization}, {Math.Round(volumeRemaining, 2, MidpointRounding.ToEven)} L");
                    }

                    _itemsSelectorScrollPosition = GUILayout.BeginScrollView(_itemsSelectorScrollPosition);
                    {
                        GUILayout.BeginVertical();
                        {
                            foreach (InventoryItem item in GetAllItems().GetSorted())
                            {
                                if (item.IsAvailable)
                                {
                                    bool canPurchase = (
                                        (itemsRemaining > 0) &&
                                        (item.Volume <= volumeRemaining) &&
                                        (item.Cost <= fundsRemaining));

                                    GUIStyle buttonStyle = (canPurchase)
                                        ? _elementStyles.ValidButton
                                        : _elementStyles.InvalidButton;
                                    if (GUILayout.Button($"{item.Title}", buttonStyle))
                                    {
                                        if (canPurchase)
                                        {
                                            _itemsSelected[_selectedKerbal.name].Add(item);
                                        }
                                    }
                                }
                            }
                        }
                        GUILayout.EndVertical();
                    }
                    GUILayout.EndScrollView();
                }
                GUILayout.EndVertical();
            }
        }

        /// <summary>Draws the button to toggle debug logging.</summary>
        /// <param name="guiCoordinates">The screen coordinates of the main GUI dialogue window.</param>
        private void DrawDebugButton(Rect guiCoordinates)
        {
			GUIStyle style = (Log.isDebug) ? _elementStyles.HighlightedButton : _elementStyles.SelectedButton;
            if (GUI.Button(
                new Rect(0, guiCoordinates.height - 10, 10, 10), "*", style))
            {
                Log.detail("Debugging messages are being turned OFF");
                Log.isDebug = !Log.isDebug;
                Log.detail("Debugging messages have been turned ON");
                string state = (Log.isDebug) ? "ON" : "OFF";
                ScreenMessages.PostScreenMessage(new ScreenMessage($"WalkAbout debugging is {state}", 4.0f, ScreenMessageStyle.UPPER_LEFT));
            }
        }
    }
}