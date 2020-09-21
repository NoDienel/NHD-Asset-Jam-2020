using System.Collections.Generic;
using System.Collections;
using UnityEngine;
using System.Text;
using UnityEngine.UI;
using System;

/// <summary>
///
/// 
/// <para>It is a requirement that the name of the .cs file inheriting from this file, the name of the TileManager scriptable object,
/// and the name of the .obj mesh in lowercase, are all the same for filesearch purposes
/// In general this isn't great to do but it's quick and dirty and only done when the TileTemplates first load in
/// I have no plans to change this</para>
/// </summary>

namespace TileMechanics.Behavior
{
    using Coordinates = Vector2Int;
    using Modifiers = Dictionary<string, int>;

    /// <summary>
    ///  This class exists as the base behavior of each unique tile type, to be used in inheritance. From this 'land' tile behaviors and 'building' tile behaviors both inherit, each with their own inheritance
    /// </summary>
    abstract public class TileBehavior : MonoBehaviour
    {

        public TileTemplate originTemplate;
        public GameObject UIPoint;
        public Canvas UI;
        public Text UIText;
        public Coordinates position;
        public Coordinates[] neighbors;

        /// <summary>
        /// The number of items this tile currently holds
        /// </summary>
        protected int[] thisTilesItems = TileItem.ItemList();
        //Yeah gross but I don't have time to mess around with variables. although I have considered
        protected int[] itemsToCastle = TileItem.ItemList();

        /// <summary>
        /// The change in items at the end of every turn
        /// </summary>
        protected int[] baseItemChangePerCycle = TileItem.ItemList();

        /// <summary>
        /// A list of modifiers to the 'end turn' change, with the list being which resource, 
        /// and the dictionary containing a list of different changes sorted by their source as strings
        /// </summary>
        public List<Modifiers> itemChangeModifiers = new List<Modifiers>();

        protected int[] itemsMax = TileItem.ItemList();
        public List<Modifiers> itemMaxModifiers = new List<Modifiers>();

        public DijkstraNode node;


        #region Accesors and Mutators

        public int getItemCount(string ItemType)
        {
            return getItemCount(TileItem.ID(ItemType));
        }
        public int getItemCount(int ItemID)
        {
            return thisTilesItems[ItemID];
        }
        public int[] getAllItemCount()
        {
            return thisTilesItems;
        }
        /// <summary>
        /// Modifies a given itemCount. 
        /// Returns the inverse of however many it was able to put in the array. Therefor you can 'put in' and 'take out' resources between two locations at once
        /// </summary>
        /// <param name="value">the value is added to the thisTilesItems list, after checking to see that it doesnt exceed the max</param>
        /// <param name="ItemType">The name of the type of item as defined in TileItem</param>
        public int changeItemCount(int value, string ItemType)
        {
            return changeItemCount(value, TileItem.ID(ItemType));
        }
        public int changeItemCount(int value, int ItemID)
        {
            int totalvalue = thisTilesItems[ItemID] + value;
            thisTilesItems[ItemID] = totalvalue;


            int max = getMaxItems(ItemID);

            int leftover = 0;
            if (totalvalue > max)
            {
                leftover = totalvalue - max;
                thisTilesItems[ItemID] = max;
            }
            if (totalvalue < 0)
            {
                leftover = totalvalue;
                thisTilesItems[ItemID] = 0;
            }


            itemsToCastle[ItemID] = value - leftover;


            //both symblomatic of the fact that the 'excess' is returned,
            //but also is used to calculate how much is actually taken out of this tile and given to the castle hold
            return leftover;
        }

        public virtual int getItemChangePerCycle(int itemID)
        {
            return baseItemChangePerCycle[itemID] + sumModifiers(itemID, itemChangeModifiers);
        }
        public int getMaxItems(int itemID)
        {
            return itemsMax[itemID] + sumModifiers(itemID, itemMaxModifiers);
        }
        public bool tryAddItemChangeModifier(int itemID, string sourceUniqueName, int value)
        {
            Modifiers mods = itemChangeModifiers[itemID];
            if (!mods.ContainsKey(sourceUniqueName))
            {
                mods.Add(sourceUniqueName, value);
                return true;
            }
            else
                return false;

        }
        public bool tryRemoveItemChangeModifier(int itemID, string sourceUniqueName)
        {
            Modifiers mods = itemChangeModifiers[itemID];
            return mods.Remove(sourceUniqueName);
        }
        public bool tryAddItemMaxModifier(int itemID, string sourceUniqueName, int value)
        {
            Modifiers mods = itemMaxModifiers[itemID];
            if (!mods.ContainsKey(sourceUniqueName))
            {
                mods.Add(sourceUniqueName, value);
                return true;
            }
            else
                return false;

        }
        public bool tryRemoveItemMaxModifier(int itemID, string sourceUniqueName)
        {
            Modifiers mods = itemMaxModifiers[itemID];
            return mods.Remove(sourceUniqueName);
        }
        public int sumModifiers(int itemID, List<Modifiers> modList)
        {
            int totalMod = 0;
            if (modList.Count <= itemID && modList[itemID].Count > 0)
            {
                foreach (int mod in modList[itemID].Values)
                    totalMod += mod;
            }
            return totalMod;
        }
        #endregion

        virtual protected void Awake()
        {
            //Add one list of modifiers for each of the types of items
            for (int i = 0; i < TileItem.Count; ++i)
            {
                itemChangeModifiers.Add(new Modifiers());
                itemMaxModifiers.Add(new Modifiers());
            }
        }
        public virtual void Initialzie(TileTemplate self, Coordinates position, Coordinates[] adjacent)
        {
            originTemplate = self;
            this.position = position;
            neighbors = adjacent;
            node = new DijkstraNode();
            node.behavior = this;
            node.coordinates = position;

            for (int i = 0; i < TileItem.Count; ++i)
            {
                if (itemsMax[i] == 0)
                {
                    if(i != TileItem.ID("housing"))
                        itemsMax[i] = 50;
                }
            }
        }
        public virtual void SecondaryInitialize()
        {
            Debug.Log("After tilemapMap loaded, secondary loading begun for " + this.name);
        }

        public virtual void TertiaryInitialize()
        {
            Debug.Log("After node network setup, tertiary loading has begun for " + this.name);
        }


        public virtual void ResetTracking()
        {

        }
        /// <summary>
        /// Make sure you call the base implementation of this function in each inherited class
        /// base.OnTurnEnd() will activate default end-turn activities
        /// </summary>
        public virtual void OnTurnEnd()
        {
            Debug.Log("Default EndTurn for " + this.name);
            TurnEndDefaultBehavior();
        }
        public virtual void AfterTurnEnd()
        {
            Debug.Log("Default EndTurn for " + this.name);
        }

        private void TurnEndDefaultBehavior()
        {
            //Iterates through all the resource IDs and updates them accordingly
            for (int i = 0; i < TileItem.Count; ++i)
            {
                changeItemCount(baseItemChangePerCycle[i] + sumModifiers(i, itemChangeModifiers), i);
            }
        }

        override public string ToString()
        {
            StringBuilder sb = new StringBuilder("Tile: " + originTemplate.name + "\nAt coordinates: " + position + "\n\n");
            for (int i = 0; i < TileItem.Count; ++i)
            {
                int count = getItemCount(i);
                int change = getItemChangePerCycle(i);
                int max = getMaxItems(i);
                if (count != 0 || change != 0)
                {
                    sb.Append("Resource: " + TileItem.ItemType(i) + "\nCount: " + count + '/' + max + "\nChange: " + change + " per/cycle\n");
                    int sentToCastle = itemsToCastle[i];
                    if (sentToCastle != 0)
                    {
                        sb.Append("To Castle: " + sentToCastle + " per/cycle\n");
                    }
                }
            }
            return sb.ToString();
        }

        private void OnDrawGizmos()
        {
            if (false && this.position.x % 5 == 0 && this.position.y % 5 == 0)
            {
                Gizmos.color = Color.red;
                Gizmos.DrawWireSphere(TileManager.Instance.CenteredCellToWorld(this.position), .5f);
                foreach (Coordinates neighbor in neighbors)
                {
                    Gizmos.color = Color.blue;
                    Gizmos.DrawWireSphere(TileManager.Instance.CenteredCellToWorld(neighbor), .5f);
                }
            }
        }
    }
}
