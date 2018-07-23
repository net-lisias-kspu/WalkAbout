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
namespace KspWalkAbout.Entities
{

    /// <summary>Represents an item that can be added to a kerbal's inventory.</summary>
    public class InventoryItem
    {
        /// <summary>The textual identifier of the item.</summary>
        [Persistent]
        public string Name;

        /// <summary>
        /// An indicator of where this item should appear in a sorted list of items 
        /// (higher numbers appear earlier on the list).
        /// </summary>
        [Persistent]
        internal int Queueing;

        /// <summary>Indicates whether the item can currently be added to an inventory.</summary>
        public bool IsAvailable;

        /// <summary>The user-friendly textual identifier.</summary>
        public string Title { get; internal set; }

        /// <summary>The cost of the item in KSP funds.</summary>
        public float Cost { get; internal set; }

        /// <summary>The volume of the item in litres.</summary>
        public float Volume { get; internal set; }
    }
}