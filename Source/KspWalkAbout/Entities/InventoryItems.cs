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

            foreach (InventoryItem item in Values)
            {
                if (item.Queueing > 0)
                {
                    Log.detail("Adding item {0} (queueing={1}) to InventoryItems to save", item.Name, item.Queueing);
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
            if (Log.isDebug)
            {
                foreach (InventoryItem item in items)
                {
                    Log.detail("requeueing item {0} from queue {1}", item.Name, this[item.Name].Queueing);
                }
            }

            foreach (InventoryItem item in items)
            {
                int origQueueing = this[item.Name].Queueing;
                int newQueueing = (origQueueing == 0) ? 1 : Math.Min(_maxQueueing, _maxQueueing - (_maxQueueing - origQueueing) / 2 + 1);
                Log.detail("requeuing operation to change {0} from {1} to {2} ", item.Name, origQueueing, newQueueing);
                if (origQueueing == 0)
                {
                    Log.detail("increasing queueing for items between [0..{1}[", _maxQueueing);
                    for (int index = 0; index < _maxQueueing; index++, this[_sortedItems[index].Name].Queueing++)
                    {
                        Log.detail("{0} set to {1}", this[_sortedItems[index].Name], this[_sortedItems[index].Name].Queueing);
                    }
                    _maxQueueing++;
                }
                else
                {
                    Log.detail("decreasing queueing for items between [{0}..{1}[", newQueueing, origQueueing);
                    for (int index = newQueueing; index < origQueueing; index++, this[_sortedItems[index].Name].Queueing--)
                    {
                        Log.detail("{0} set to {1}", this[_sortedItems[index].Name], this[_sortedItems[index].Name].Queueing);
                    }
                }
                this[item.Name].Queueing = newQueueing;
                Log.detail("{0} set to {1}", item.Name, newQueueing);

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
            Log.detail("KIS detected - refreshing");

            if (Count == 0)
            {
                LoadItemsFromFile();
            }

            Log.detail("examining {0} parts from PartLoader", PartLoader.LoadedPartsList.Count);
            foreach (AvailablePart part in PartLoader.LoadedPartsList)
            {
                float volume = CalculatePartVolume(part.partPrefab);
                if (volume == 0.0f)
                {
                    volume = EstimatePartVolume(part.partPrefab);
                }

                if (volume <= GetModConfig().MaxInventoryVolume)
                {
                    ProtoTechNode tech = AssetBase.RnDTechTree.FindTech(part.TechRequired);
                    if (tech == null)
                    {
                        Log.detail("unable to find tech for {0}", part.name);
                        continue;
                    }

                    bool available = GetPartAvailability(part);

                    Log.detail(
                            "info for part {0}: cost={1} volume={2} title={3} required tech={4} available={4}", 
                            part.name, part.cost, volume, part.title, (part?.TechRequired) ?? "null", available
                        );

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
            float volume = 0.0f;

            PartModule module = GetPartKisModule(part);
            if (module != null)
            {
                string volumeText = module.Fields["volumeOverride"].originalValue.ToString();
                if (!float.TryParse(volumeText, out volume))
                {
                    Log.detail("Part [{0}]: unable to translate KIS volumeOverride value [{1}] to a valid number", part.name, volumeText);
                }
            }

            return volume;
        }

        private static PartModule GetPartKisModule(Part part)
        {
            PartModule module = null;

            if ((part.Modules != null) && (part.Modules.Count > 0))
            {
                foreach (string KisModuleName in WalkAboutPersistent.KisModuleNames)
                {
                    foreach (PartModule partModule in part.Modules)
                    {
                        if ((!string.IsNullOrEmpty(partModule.moduleName)) && (partModule.moduleName.Contains(KisModuleName)))
                        {
                            module = partModule;
                            break;
                        }
                    }
                }
            }

            return module;
        }

        private static float EstimatePartVolume(Part part)
        {
            UnityEngine.Vector3 boundsSize = PartGeometryUtil.MergeBounds(part.GetRendererBounds(), part.transform).size;
            float volume = boundsSize.x * boundsSize.y * boundsSize.z * 1000f;

            return volume;
        }

        /// <summary>
        /// Determines if a part is available for the current game mode and researched techs.
        /// </summary>
        /// <param name="part">The part to be evaluated.</param>
        /// <returns>A value indicating if the part is available.</returns>
        private static bool GetPartAvailability(AvailablePart part)
        {
            bool available = true;
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
            Log.detail("Refresh: No items detected - loading (_file={0})", _file);
            bool loaded = _file.Load($"{WalkAbout.GetModDirectory()}/Items.cfg", Constants.DefaultItems);
            Log.detail("Refresh: loaded={0} status={1}", loaded, _file.StatusMessage);
            if (loaded)
            {
                if (_file.Items == null)
                {
                    _file.Items = new List<InventoryItem>();
                    Log.detail("Initialized items to null list due to unresolved bug in Settings");
                }

                foreach (InventoryItem item in _file.Items)
                {
                    Add(item.Name, item);
                    Log.detail("added previously selected part [{0}]", item.Name);
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
            int queueOrder = (b?.Queueing ?? 0).CompareTo(a?.Queueing ?? 0);
            return queueOrder == 0
                ? (a?.Volume ?? 0).CompareTo(b?.Volume ?? 0)
                : queueOrder;
        }
    }
}