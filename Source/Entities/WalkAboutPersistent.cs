using System.Collections.Generic;

namespace KspWalkAbout.Entities
{
    /// <summary>Contains items that are to remain available for use across multiple invocations of the mod.</summary>
    internal static class WalkAboutPersistent
    {
        public static Dictionary<string, List<string>> InventoryItems = new Dictionary<string, List<string>>();
    }
}
