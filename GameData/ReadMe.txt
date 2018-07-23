
WalkAbout (version 0.1.5)

The WalkAbout mod allows you to take an available kerbal from the Astronaut Complex and have him/her placed outside any door at the KSC.

You can activate WalkAbout from the Space Centre scene by pressing ctrl-W. The WalkAbout selections screen will appear.

	1 - List of available kerbals.
	- any kerbal currently not assigned to a craft or on EVA will appear in this list.  Simply click on a kerbal to select that one.
	2 - Facilities
	- a list of the KSC facilities with locations for placing kerbals.  Click on one of these to restrict the list of Locations (see below) to only those locations associated with the facility.
	3 - Locations
	- list of all locations where a kerbal may be placed. Or, if a facility has been selected, a list of all locations associated with that facility.  See notes 1, 2, 3 and 4.
	4 - Action Button
	- when the button text is green, click it to send your kerbal on WalkAbout.
	5 - Top 5 Button
	- restricts the locations shown to only the top 5 most commonly selected locations.  See notes 3, 4 and 5.
	6 - Toggle Inventory Button (only appears if the Kerbal Inventory System mod is installed)
	- turns the inventory display on or off
	7 - Kerbal Inventory
	- lists all items in the selected kerbal's inventory.
	8 - Available Items
	- list all items that can be added to the kerbal's inventory.

	Note 1:
	Locations are recorded in the .loc files.  Each location corresponds to a position just outside of one of the doors found on the outside of the buildings of the KSC.  
	I have tried to catalogue every door available at each of the 3 levels of upgrade for all of the buildings, but as you may imagine, it would be easy for me to miss one (or two (or three (...)))).  If you notice any doors or exits that I have missed, please let me know on the forum page.  (I do know about the doors on the tracking dishes - once I figure out how to dynamically determine their locations I will add a fix for them.)

	Note 2:
	Since the individual buildings in KSP are not named, I tried to come up with a generic naming convention for the buildings and the locations.  Most location names are formatted as follows:
		ff_bldg[_subsection]_[d]ls  where
		ff is an abbreviation of the facility name,
		bldg is a name for the building
		subsection may be a wing or a separate building name (eg ..._QuonsetF_...)
		d is the direction the side of the building faces (N, S, E, W)
		l is the floor #
		s is the sequence letter for the door (A, B, C...).
	So R+D_WindTunnel_BldgC_N1B is 2nd door on the first floor of the north side of Building C of the Wind Tunnel complex of the R&D facility.

	Note 3:
	When the selection screen first appears, all locations for all facilities are shown.  Use the Facilities selector and/or the Top 5 button to limit the number of locations shown.

	Note 4:
	Locations are shown in alphabetical order.  However, each time a location is used, it is moved up the list thereby keeping the most often used locations at the top of list.  

	Note 5:
	The Top 5 button is not limited to 5 entries.  The number of locations selected by this button can be set by altering the value for TopFew in the settings.cfg file.

Adding your own Locations:
	It is possible to add your own locations to those that come packaged with WalkAbout. To do so follow the steps below:

	1 - Edit the settings.cfg file in the WalkAbout installation directory.  Change the value of Mode from "normal" to "utility".
	2 - Run KSP and enter the Flight scene for a kerbal on EVA. See note 3.
	3 - Press ctrl-X and the Add Utility GUI should appear.
	4 - Move the kerbal to the new location that you wish to add and face the kerbal in the desired direction. See note 1.
	5 - Select a facility.  Each location is associated with a facility and the current upgrade level of that facility.
	6 - Press the Add Location button.  The new location will be added to a file named user.loc in the locFiles directory. See note 2.

	Note 1:
	Below the Cancel button, you will see up to 3 locations displayed.  For all facilities at level 1, level 2, or level 3, the locations that are closest to the active kerbal's current position are shown.  This can come in handy to determine if you have already entered a location, or if a location is valid for two different upgrade levels.

	Note 2: 
	In the user.loc file, it possible to set a location so that it is valid for more than one upgrade level of a facility (e.g. the location of the main entrace to the Astronaut Complex is the same for whether the Astronaut Complex is at level 1 or has been upgraded to level 2).  To make a location valid for multiple level, change the AvailableAtLevels value to one of
		Level_1
		Level_2
		Level_3
		Levels_1_2
		Levels_1_3
		Levels_1_2_3
		Levels_2_3
	Make sure you note difference between Level_... and Levels_... - using the wrong pluralization can result in the loss of data in the .loc file.

	Note 3:
	This utility, and, in fact, the WalkAbout mod, were intended for use in placing kerbals in and around the area of the KSC.  Though the utility can be used to create locations far from the KSC, doing so is not supported by the author (but I'm willing to close a blind eye). If you want to create locations off the surface of Kerbin - you are utterly on your own.  Doing so is unsupported, probably won't work, and will void any and all warranties held for your kerbal's EVA suit, this mod, and probably even your coffee maker - so... you've been warned.

v1.5
  New Features:
	Added support for Kerbal Inventory System. Items can be added to the kerbal's inventory before the kerbal is placed.

v1.4
  Works with KSP version 1.2
  New Features:
	Added Extras folder.

  Fixed Issues:
	WA005 Reloading the KSC scene sometimes results in log-spam: [ERR 16:26:34.891] Serialization depth limit exceeded at 'Contracts.Agents::Agent'. There may be an object composition cycle in one or more of your serialized classes.
	WA008 When the location is under a structure, the kerbal is generated at high altitude and drops from, or clings to, the structure.
  
v1.3
  Works with KSP version 1.1 
  New Features:
	No longer overwrites an existing settings.cfg file when updated.

  Fixed Issues:
	WA011 User reported nullreference log spam.
	
v1.2
  New Features:
	Resizable windows.
	CKAN compatibility.

  Fixed Issues:
	Fixed locations VAB_Main_E1A and VAB_Main_W1A (were reversed).

v1.1
  Fixed Issues:
	WA009 Eliminated NullReferenceException caused by running in normal mode.   
	WA010 Locations were not moving further up the list when selected. 

v1.0
  Initial Release

v0.0.2
  Fixed Issues:
	WA001 Include a ReadMe file.
	WA004 Require license text.
	WA007 ReadMe needs description and instructions.

v0.1.0
  New Features:
	Buttons indicate status by colour (green=operation can be done, yellow=additional action required).
	Can limit the locations displayed to a single Facility.
	Can limit the number of locations displayed using the Top # Only button.
	AddUtility: Added display of closest known locations.
	AddUtility: Added location Ids.
  Fixed Issues:
	WA002 Added kerbals do not appear on KSC scene until dialogue is re-opened.
	WA006 Requeueing of locations is not working correctly.
	
	
Outstanding Issues:
	WA003 Kerbals are generated face-down and pop up when EVA view is activated.
	WA008 When the location is under a structure, the kerbal is generated at high altitude and drops from, or clings to, the structure.

WishList
 - While on EVA, get another kerbal on WalkAbout.
 - Select location by pointing to the map.
 - Find a way to place kerbals on moving tracking dishes.
 - Work with relocated KSC.
