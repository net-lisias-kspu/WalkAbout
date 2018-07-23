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
using KspWalkAbout.Entities;
using KspWalkAbout.Extensions;
using KspWalkAbout.Values;
using System.Collections.Generic;
using UnityEngine;

namespace KspWalkAbout.Guis
{
    /// <summary>Represents the GUI used to place kerbals at locations.</summary>
    internal class MainGui
    {
        private Rect _coordinates;
        private Vector2 _kerbalSelectorScrollPosition;
        private Vector2 _facilitySelectorScrollPosition;
        private Vector2 _locationSelectorScrollPosition;
        private ProtoCrewMember _selectedKerbal;
        private string _selectedFacility;
        private Location _selectedLocation;
        private GuiElementStyles _elementStyles;
        private bool _showTopFewOnly;
        private string _windowTitle;
        private Vector2 _minGuiSize;

        /// <summary>Initializes a new instance of the MainGui class.</summary>
        internal MainGui()
        {
            _kerbalSelectorScrollPosition = Vector2.zero;
            _facilitySelectorScrollPosition = Vector2.zero;
            _locationSelectorScrollPosition = Vector2.zero;

            _showTopFewOnly = false;
            _windowTitle = $"{Constants.ModName} v{Constants.Version}";
            _minGuiSize = new Vector2(200, 50);

            IsActive = false;
            Locations = new List<Location>();
        }

        /// <summary>Gets or sets the screen coordinates and size of the GUI.</summary>
        internal Rect GuiCoordinates
        {
            get { return _coordinates; }
            set
            {
                if (value == new Rect()) "given empty screen position - defaulting to 1/4 screen".Debug();
                _coordinates = (value == new Rect())
                    ? new Rect(Screen.width / 4, Screen.height / 4, Screen.width / 3, Screen.height / 4)
                    : value;
            }
        }

        /// <summary>Gets or sets a value indicating whether the main GUI is displayed and usable.</summary>
        internal bool IsActive { get; set; }

        /// <summary>Gets or sets the collection of facility names from which the the user may select.</summary>
        internal List<string> Facilities { get; set; }

        /// <summary>Gets or sets the collection of locations were kerbals can be placed.</summary>
        internal List<Location> Locations { get; set; }

        /// <summary>The max number of locations to display in the GUI as the most likely locations the user wants to select from.</summary>
        public int TopFew { get; internal set; }

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
            var haveKerbals = false;
            var haveLocations = false;

            GUILayout.BeginVertical();
            {
                GUILayout.BeginHorizontal();
                {
                    haveKerbals = DrawKerbalSelector();
                    DrawFacilitySelector();
                    haveLocations = DrawLocationSelector();
                }
                GUILayout.EndHorizontal();
                DrawActionButton(haveKerbals, haveLocations);
                DrawCancelButton();
            }
            GUILayout.EndVertical();

            GuiResizer.DrawResizingButton(_coordinates, _minGuiSize);
            GUI.DragWindow(new Rect(0, 0, GuiCoordinates.width, GuiCoordinates.height));
        }

        /// <summary>Draws the portion of the GUI that displays available kerbals and handles user selection.</summary>
        /// <returns>A value indicating whether any buttons for selecting kerbals were drawn.</returns>
        private bool DrawKerbalSelector()
        {
            var drewKerbalButtons = false;
            _kerbalSelectorScrollPosition = GUILayout.BeginScrollView(_kerbalSelectorScrollPosition);
            {
                GUILayout.BeginVertical();
                {
                    foreach (var crew in HighLogic.CurrentGame.CrewRoster.Crew)
                    {
                        if (crew.rosterStatus == ProtoCrewMember.RosterStatus.Available)
                        {
                            var buttonStyle = (crew.name == ((_selectedKerbal?.name) ?? string.Empty))
                                ? _elementStyles.ActionableButton
                                : _elementStyles.ValidButton;
                            if (GUILayout.Button(crew.name, buttonStyle))
                            {
                                _selectedKerbal = crew; $"selected {_selectedKerbal.name}".Debug();
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
                    foreach (var facilityName in Facilities)
                    {
                        var buttonText = facilityName.Substring(facilityName.IndexOf('/') + 1);
                        var buttonStyle = (facilityName == (_selectedFacility ?? string.Empty))
                            ? _elementStyles.ActionableButton
                            : _elementStyles.ValidButton;
                        if (GUILayout.Button(buttonText, buttonStyle))
                        {
                            _selectedFacility = (facilityName == _selectedFacility) ? null : facilityName;
                            $"selected {_selectedFacility}".Debug();
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
            var locationButtonsDrawn = 0;

            GUILayout.BeginVertical();
            {
                _showTopFewOnly = ((TopFew > 0) && (Locations.Count > TopFew))
                    ? (GUILayout.Toggle(_showTopFewOnly, $"Top {TopFew} Only"))
                    : false;

                _locationSelectorScrollPosition = GUILayout.BeginScrollView(_locationSelectorScrollPosition);
                {
                    GUILayout.BeginVertical();
                    {
                        foreach (var location in Locations)
                        {
                            if (string.IsNullOrEmpty(_selectedFacility) || (location.FacilityName == _selectedFacility))
                            {
                                var buttonStyle = (location.LocationName == ((_selectedLocation?.LocationName) ?? string.Empty))
                                    ? _elementStyles.ActionableButton
                                    : _elementStyles.ValidButton;
                                if (GUILayout.Button(location.LocationName, buttonStyle))
                                {
                                    _selectedLocation = location; $"selected {_selectedLocation.LocationName}".Debug();
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
                    if (GUILayout.Button($"Place {_selectedKerbal.name} outside {_selectedLocation.LocationName}", _elementStyles.ActionableButton))
                    {
                        RequestedPlacement = new PlacementRequest { Kerbal = _selectedKerbal, Location = _selectedLocation };
                        "PlacementRequest created - deactivating GUI".Debug();
                        IsActive = false;
                    }
                }
            }
            else
            {
                GUILayout.Button(haveKerbals ? "No locations found" : "No available kerbals", _elementStyles.InvalidButton);
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
    }
}