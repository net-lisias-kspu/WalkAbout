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

using KspAccess;
using KspWalkAbout.Extensions;
using KspWalkAbout.Values;
using KspWalkAbout.WalkAboutFiles;
using System;
using System.Collections.Generic;
using static KspWalkAbout.Entities.WalkAboutPersistent;

namespace KspWalkAbout.Entities
{
    /// <summary>Represents the collection of items that can possibly be added to an inventory.</summary>
    internal class InventoryItems : Dictionary<string, InventoryItem>
    {
        private ItemsFile _file = new ItemsFile();
        private InventoryItem[] _sortedItems;
        private int _maxQueueing = 0;

        /// <summary>
        /// Creates a new instance of the InventoryItems class.
        /// </summary>
        internal InventoryItems()
        {
            RefreshItems();
        }

        /// <summary>
        /// The maximum volume of all items added to an inventory.
        /// </summary>
        internal float MaxVolume { get; set; }

        /// <summary>
        /// Whether or not the values for any items have been altered.
        /// </summary>
        internal bool IsChanged { get; set; }

        /// <summary>
        /// Writes the information about all items to disk.
        /// </summary>
        internal void Save()
        {
            if (!IsChanged || (Count == 0)) { return; }

            _file.Items.Clear();

            foreach (var item in Values)
            {
                if (item.Queueing > 0)
                {
                    $"Adding item {item.Name} (queueing={item.Queueing}) to InventoryItems to save".Debug();
                    _file.Items.Add(item);
                }
            }

            if (_file.Items.Count > 0)
            {
                _file.Save();
            }

            IsChanged = false;
        }

        /// <summary>
        /// Determines the new order of items within a sorted list of items after a specific
        /// set of items have been chosen.</summary>
        /// <param name="items">List of items that are to move up the display queue.</param>
        /// <remarks>
        /// Each time an item is chosen it is moved up in the rank of all available items:
        /// <list type="text">
        /// <item>
        /// If this is the first time the item is chosen, it is moved to the bottom of the list of
        /// previously chosen items.
        /// </item>
        /// <item>
        /// Each subesequent selection of the item will move it up the list by approximately 1/2 its
        /// "distance" to the top of the list.
        /// </item>
        /// </list>
        /// </remarks>
        internal void UpdateQueueing(List<InventoryItem> items)
        {
            if (DebugExtensions.DebugIsOn)
            {
                foreach (var item in items)
                {
                    $"requeueing item {item.Name} from queue {this[item.Name].Queueing}".Debug();
                }
            }

            foreach (var item in items)
            {
                var origQueueing = this[item.Name].Queueing;
                var newQueueing = (origQueueing == 0) ? 1 : Math.Min(_maxQueueing, _maxQueueing - (_maxQueueing - origQueueing) / 2 + 1);
                $"requeuing operation to change {item.Name} from {origQueueing} to {newQueueing} ".Debug();
                if (origQueueing == 0)
                {
                    $"increasing queueing for items between {0} and {_maxQueueing - 1}".Debug();
                    for (var index = 0; index < _maxQueueing; index++, this[_sortedItems[index].Name].Queueing++)
                    {
                        $"{this[_sortedItems[index].Name]} set to {this[_sortedItems[index].Name].Queueing}".Debug();
                    }
                    _maxQueueing++;
                }
                else
                {
                    $"decreasing queueing for items between {newQueueing} and {origQueueing - 1}".Debug();
                    for (var index = newQueueing; index < origQueueing; index++, this[_sortedItems[index].Name].Queueing--)
                    {
                        $"{this[_sortedItems[index].Name]} set to {this[_sortedItems[index].Name].Queueing}".Debug();
                    }
                }
                this[item.Name].Queueing = newQueueing;
                $"{item.Name} set to {newQueueing}".Debug();

                SortItems();
            }

            if (IsChanged)
                Save();
        }

        /// <summary>
        /// Reevaluates the items that can be added to a kerbal's inventory.
        /// </summary>
        internal void RefreshItems()
        {
            if (!WalkAboutKspAccess.IsKisModDetected())
            {
                return;
            }
            $"KIS detected - refreshing".Debug();

            if (Count == 0)
            {
                LoadItemsFromFile();
            }

            $"examining {PartLoader.LoadedPartsList.Count} parts from PartLoader".Debug();
            foreach (var part in PartLoader.LoadedPartsList)
            {
                var volume = CalculatePartVolume(part.partPrefab);
                if (volume == 0.0f)
                {
                    volume = EstimatePartVolume(part.partPrefab);
                }

                if (volume <= GetModConfig().MaxInventoryVolume)
                {
                    var tech = AssetBase.RnDTechTree.FindTech(part.TechRequired);
                    if (tech == null)
                    {
                        $"unable to find tech for {part.name}".Debug();
                        continue;
                    }

                    var available = GetPartAvailability(part);

                    $"info for part {part.name}: cost={part.cost} volume={volume} title={part.title} required tech={(part?.TechRequired) ?? "null"} available={available}".Debug();

                    if (ContainsKey(part.name))
                    {
                        this[part.name].Title = part.title;
                        this[part.name].IsAvailable = available;
                        this[part.name].Volume = volume;
                        this[part.name].Cost = part.cost;
                        _maxQueueing = Math.Max(_maxQueueing, this[part.name].Queueing);
                    }
                    else
                    {
                        Add(part.name, new InventoryItem()
                        {
                            Name = part.name,
                            Title = part.title,
                            IsAvailable = available,
                            Volume = volume,
                            Cost = part.cost,
                            Queueing = 0,
                        });
                    }
                }
            }

            SortItems();
        }

        /// <summary>Obtains a sorted list of the items in this collection.</summary>
        /// <returns>Items sorted in the proper order for displaying.</returns>
        internal IEnumerable<InventoryItem> GetSorted()
        {
            return _sortedItems;
        }

        private static float CalculatePartVolume(Part part)
        {
            var volume = 0.0f;

            var module = GetPartKisModule(part);
            if (module != null)
            {
                var volumeText = module.Fields["volumeOverride"].originalValue.ToString();
                if (!float.TryParse(volumeText, out volume))
                {
                    $"Part [{part.name}]: unable to translate KIS volumeOverride value [{volumeText}] to a valid number".Debug();
                }
            }

            return volume;
        }

        private static PartModule GetPartKisModule(Part part)
        {
            PartModule module = null;

            if ((part.Modules != null) && (part.Modules.Count > 0))
            {
                foreach (var moduleName in WalkAboutPersistent.KisModuleNames)
                {
                    if (part.Modules.Contains(moduleName))
                    {
                        module = part.Modules[moduleName];
                        break;
                    }
                }
            }

            return module;
        }

        private static float EstimatePartVolume(Part part)
        {
            var boundsSize = PartGeometryUtil.MergeBounds(part.GetRendererBounds(), part.transform).size;
            var volume = boundsSize.x * boundsSize.y * boundsSize.z * 1000f;

            return volume;
        }

        /// <summary>
        /// Determines if a part is available for the current game mode and researched techs.
        /// </summary>
        /// <param name="part">The part to be evaluated.</param>
        /// <returns>A value indicating if the part is available.</returns>
        private static bool GetPartAvailability(AvailablePart part)
        {
            var available = true;
            if ((HighLogic.CurrentGame.Mode == Game.Modes.CAREER) ||
                (HighLogic.CurrentGame.Mode == Game.Modes.SCIENCE_SANDBOX))
            {
                available = ResearchAndDevelopment.PartModelPurchased(part);
            }

            return available;
        }

        /// <summary>
        /// Adds any recorded information about items that have already been selected.
        /// </summary>
        private void LoadItemsFromFile()
        {
            $"Refresh: No items detected - loading (_file={_file})".Debug();
            var loaded = _file.Load($"{WalkAbout.GetModDirectory()}/Items.cfg", Constants.DefaultItems);
            $"Refresh: loaded={loaded} status={_file.StatusMessage}".Debug();
            if (loaded)
            {
                if (_file.Items == null)
                {
                    _file.Items = new List<InventoryItem>();
                    "Initialized items to null list due to unresolved bug in Settings".Debug();
                }

                foreach (var item in _file.Items)
                {
                    Add(item.Name, item);
                    $"added previously selected part [{item.Name}]".Debug();
                }
            }
        }

        /// <summary>
        /// Sort the items in this collection.
        /// </summary>
        private void SortItems()
        {
            _sortedItems = new InventoryItem[Count];
            Values.CopyTo(_sortedItems, 0);
            Array.Sort(_sortedItems, CompareItems);
            IsChanged = true;
        }

        /// <summary> Compares two items ordering them by queueing and name.</summary>
        /// <param name="a">The base item.</param>
        /// <param name="b">The item to be combared to the base item.</param>
        /// <returns>A value indicating whether item b comes before or after item a.</returns>
        private int CompareItems(InventoryItem a, InventoryItem b)
        {
            var queueOrder = (b?.Queueing ?? 0).CompareTo(a?.Queueing ?? 0);
            return queueOrder == 0
                ? (a?.Volume ?? 0).CompareTo(b?.Volume ?? 0)
                : queueOrder;
        }
    }
}