using System.Collections.Generic;


namespace TileMechanics.Behavior
{
    //basically a beefier enum for Resources and their IDs
    public static class TileItem
    {
        /// <summary>
        /// A list of the different type of tile items there are
        /// All you have to do to add a new type of item is add another string
        /// </summary>
        private static List<string> types = new List<string>()
            {
                "wood", "grain", "flour", "bread", "stone", "ore", "water", "housing"
            };

        /// <summary>
        /// Returns the integer ID of an 'Item'
        /// </summary>
        /// <param name="typename">the name of the TileItem</param>
        /// <returns></returns>
        public static int ID(string typename)
        {
            return types.IndexOf(typename);
        }

        public static string ItemType(int id)
        {
            return types[id];
        }

        public static int Count
        {
            get
            {
                return types.Count;
            }
        }

        /// <summary>
        /// Used to give every TileBehavior a list of resources.
        /// <para>use the TileItem static class to reference which resource you want</para>
        /// </summary>
        /// <returns></returns>
        public static int[] ItemList()
        {
            return new int[types.Count];
        }

        public static List<string> allTypes()
        {
            return types;
        }
    }
}

