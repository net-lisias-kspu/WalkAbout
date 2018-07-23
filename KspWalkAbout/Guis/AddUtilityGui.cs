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

using KspAccess;
using KspWalkAbout.Entities;
using KspWalkAbout.Extensions;
using KspWalkAbout.Values;
using UnityEngine;
using static KspWalkAbout.Entities.WalkAboutPersistent;

namespace KspWalkAbout.Guis
{
    /// <summary>Represents the GUI used to add new locations.</summary>
    internal class AddUtilityGui
    {
        private Rect _coordinates;
        private Vector2 _facilitySelectorScrollPosition;
        private string _enteredLocationName;
        private readonly string _unenteredText;
        private string _previousText;
        private bool _textChanged;
        private bool _existingName;
        private KnownPlaces _map;

        private string _selectedFacility;
        private GuiElementStyles _elementStyles;

        /// <summary>Initializes a new instance of the GUI with a collection of locations.</summary>
        internal AddUtilityGui()
        {
            _facilitySelectorScrollPosition = Vector2.zero;
            _unenteredText = "Location Name";

            _enteredLocationName = _unenteredText;
            _selectedFacility = string.Empty;

            _map = GetLocationMap();
            GuiCoordinates = new Rect();
            IsActive = false;
        }

        /// <summary>Gets or sets the screen coordinates and size of the GUI.</summary>
        internal Rect GuiCoordinates
        {
            get { return _coordinates; }
            set
            {
                _coordinates = (value == new Rect())
                    ? new Rect(Screen.width / 4, Screen.height / 4, Screen.width / 4, Screen.height / 2)
                    : value;
            }
        }

        /// <summary>Gets or sets a value indicating whether the main GUI is displayed and usable.</summary>
        internal bool IsActive { get; set; }

        /// <summary>Gets or sets the user's current request for a new location.</summary>
        internal LocationRequest RequestedLocation { get; set; }

        /// <summary>Called regularly to draw the GUI on screen.</summary>
        /// <returns>A value indicating whether or not the GUI was displayed.</returns>
        internal bool Display()
        {
            if (!IsActive || CommonKspAccess.IsPauseMenuOpen) return false;

            GuiCoordinates = GUI.Window(0, GuiCoordinates, DrawSelectorHandler, $"{Constants.ModName} - Add");
            return true;
        }

        /// <summary>Draws the GUI on screen and handles user selections.</summary>
        /// <param name="id">Required parameter: purpose unknown.</param>
        private void DrawSelectorHandler(int id)
        {
            if (_elementStyles == null) _elementStyles = new GuiElementStyles();

            GUILayout.BeginVertical();
            {
                DrawLocationNameInput();
                DrawFacilitySelector();
                DrawActionButton();
                DrawCancelButton();
                DrawClosestLocation();
            }
            GUILayout.EndVertical();

            GuiResizer.DrawResizingButton(GuiCoordinates);
            GUI.DragWindow(new Rect(0, 0, GuiCoordinates.width, GuiCoordinates.height));
        }

        /// <summary>Draws the portion of the GUI that allows the user to name a location.</summary>
        private void DrawLocationNameInput()
        {
            var beganWithUserText = (_enteredLocationName != _unenteredText);

            GUILayout.BeginHorizontal();
            {
                GUILayout.Label("Enter new location Name");
                _enteredLocationName = GUILayout.TextField(_enteredLocationName, beganWithUserText ? _elementStyles.ValidTextInput : _elementStyles.InvalidTextInput).Trim();
            }
            GUILayout.EndHorizontal();

            _textChanged = (_enteredLocationName != _previousText);
            _previousText = _enteredLocationName;

            if (!_textChanged) return;

            if (string.IsNullOrEmpty(_enteredLocationName))
            {
                _enteredLocationName = _unenteredText;
            }
            else if (!beganWithUserText)
            {
                foreach (var letter in _unenteredText)
                {
                    var index = _enteredLocationName.IndexOf(letter);
                    if (index != -1)
                    {
                        _enteredLocationName = _enteredLocationName.Remove(index, 1);
                    }
                }
            }

            _existingName = _map.HasLocation(_enteredLocationName);
        }

        /// <summary>Draws the portion of the GUI that displays available facilities and handles user selection.</summary>
        private void DrawFacilitySelector()
        {
            GUILayout.BeginHorizontal();
            {
                GUILayout.Label("Facilities:");
                _facilitySelectorScrollPosition = GUILayout.BeginScrollView(_facilitySelectorScrollPosition);
                {
                    GUILayout.BeginVertical();
                    {
                        foreach (var facilityName in ScenarioUpgradeableFacilities.protoUpgradeables.Keys)
                        {
                            var buttonStyle = (facilityName == (_selectedFacility ?? string.Empty))
                                ? _elementStyles.SelectedButton
                                : _elementStyles.ValidButton;
                            if (GUILayout.Button(facilityName, buttonStyle))
                            {
                                _selectedFacility = facilityName; $"selected {_selectedFacility}".Debug();
                            }
                        }
                    }
                    GUILayout.EndVertical();
                }
                GUILayout.EndScrollView();
            }
            GUILayout.EndHorizontal();
        }

        /// <summary>Draws the portion of the GUI that displays the user's confimation of action button and handles its selection.</summary>
        private void DrawActionButton()
        {
            string label = null;
            if (_enteredLocationName == _unenteredText)
            {
                label = "Enter a new location name";
            }
            if (string.IsNullOrEmpty(_selectedFacility))
            {
                label = (string.IsNullOrEmpty(label) ? "Select" : $"{label} and select") + " a facility";
            }

            if (GUILayout.Button(
                label ?? $"Add location {_enteredLocationName}",
                ((string.IsNullOrEmpty(label) || _existingName) ? _elementStyles.SelectedButton : _elementStyles.InvalidButton))
                && string.IsNullOrEmpty(label))
            {
                $"Action selected: create location {_enteredLocationName} for {_selectedFacility}".Debug();
                RequestedLocation = new LocationRequest { Name = _enteredLocationName, AssociatedFacility = _selectedFacility };
                _enteredLocationName = _unenteredText;
                _selectedFacility = string.Empty;
                IsActive = false;
                "Location request created - closing GUI".Debug();
            }
        }

        /// <summary>Draws the button to cancel the user's action and handles its selection.</summary>
        private void DrawCancelButton()
        {
            if (GUILayout.Button("Cancel"))
            {
                IsActive = false;
            }
        }

        /// <summary>Draws the portion of the GUI that displays the existing locations closest to the current EVA location.</summary>
        private void DrawClosestLocation()
        {
            var closest = _map.FindClosest(FlightGlobals.ActiveVessel.latitude,
                                           FlightGlobals.ActiveVessel.longitude,
                                           FlightGlobals.ActiveVessel.altitude);
            var text = "Closest known locations:";
            var suffix = " none found";
            for (var level = 1; level < closest.Length; level++)
            {
                if (string.IsNullOrEmpty(closest[level].Name)) continue;

                text += $"\nLevel {level} ({closest[level].Name} dh:{closest[level].Horizontal:0.00}m dv:{closest[level].Vertical:0.00}m";
                suffix = string.Empty;
            }

            text += suffix;
            GUILayout.TextArea(text);
        }
    }
}