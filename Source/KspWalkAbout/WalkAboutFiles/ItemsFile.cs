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
using KspWalkAbout.KspFiles;
using System.Collections.Generic;

namespace KspWalkAbout.WalkAboutFiles
{
    /// <summary>
    /// Represents a settings file containing information on all parts that can be used with the mod.
    /// </summary>
    public class ItemsFile : SettingsFile
    {
        /// <summary>Collection of all items that a kerbal may possibly have in inventory.</summary>
        [Persistent]
        public List<InventoryItem> Items;
    }
}