using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TileMechanics.Behavior
{
    using Coordinates = Vector2Int;
    public class Road : BuildingBehavior
    {
        //The strength of a road determines how many items it can draw from each tile each cycle
        public int roadStrength = 30;
        bool recalculatePathing = false;

        override public void Initialzie(TileTemplate self, Coordinates position, Coordinates[] adjacent)
        {
            base.Initialzie(self, position, adjacent);
            //Make sure all neighboring buildings know there's a new node
            /*  Takin the lazy way out and just forcing a recalculation
             * foreach(Coordinates n in neighbors)
            {
                if (TileManager.Instance.Tiles.ContainsKey(n))
                {
                    TileBehavior neighbor = TileManager.Instance.Tiles[n];
                    neighbor.neighbors = TileManager.Instance.GetNeighbors(neighbor.position);

                    if (neighbor.GetType().IsSubclassOf(typeof(BuildingBehavior)))
                    {
                        Debug.Log("Recalculating road connections for new road");
                        BuildingBehavior building = neighbor as BuildingBehavior;
                        building.findAdjacentRoads();
                    }
                    else if (neighbor.GetType().IsSubclassOf(typeof(LandBehavior)))
                    {
                        LandBehavior land = neighbor as LandBehavior;
                        Debug.Log("Unpacking new building");
                        if (land.BuiltBuilding != null) {
                            BuildingBehavior building = land.BuiltBuilding;
                            Debug.Log("Recalculating road connections for road neighbor: " + building.position);
                            building.findAdjacentRoads();
                        }
                        else
                            Debug.Log("Neighbor is neighther land or building; likely null");
                    }
                }
            }*/
            //Oh god this is gonna make turns last forever
            //Castle.Instance.RecalculateAllConnections();
            //Pathing after connections
            //recalculatePathing = true;
        }

        public override void OnTurnEnd()
        {
            base.OnTurnEnd();
            //This needs to happen during the end step
            if (recalculatePathing) RecalculatePathing();
        }

        public void RecalculatePathing()
        {
            if (Castle.Instance != null)
            {
                Castle pathing = Castle.Instance;
                if (!pathing.allConnectedRoads.Contains(this) && pathing.isConnected(this))
                {
                    Debug.Log("Adding new road to list");
                    pathing.allConnectedRoads.Add(this);
                    //Reseting the connected tiles forces castle to recalculate them with allConnectedRoads
                    pathing.allConnectedTiles = null;
                }
            }
            else
                Debug.LogError("everything blew up");

            recalculatePathing = false;
        }

        /// Recursively pulls in tile values
        /// eh not gonna use this it's stupidly overcomplicates things
        /// depreciated and broken anyway

        /*public int[] DrawFromTiles(List<Coordinates> alreadyDrawn = null, int[] summedValues = null)
        {
            if (alreadyDrawn == null)
                alreadyDrawn = new List<Coordinates>();
            if (summedValues == null)
                summedValues = TileItem.ItemList();
            foreach(Coordinates n in neighbors)
            {
                TileBehavior neighbor = TileManager.Instance.Tiles[n];
                for (int i = 0; i < summedValues.Length; ++i)
                {
                    //changeItemAmmount will reduce roadStrength by a value equal to how far below 0 the itemID 'i' goes, so
                    summedValues[i] += roadStrength + neighbor.changeItemCount(-roadStrength, i);
                }
            }

            if (this.validPrevious != null)
                return DrawFromTiles(alreadyDrawn, summedValues);
            else
                return summedValues;
        }*/

        public override void PrepareForDestruction()
        {
            //Recalculate path back and make sure any path containing this node is recalculated
            //Right now by default roads are recalculated every cycle so for now this doesn't matter
            //A more robust system would only calculate them as needed
        }

        private void OnDrawGizmos()
        {
            if (adjacentRoads != null)
            {
                foreach (Coordinates neighbor in adjacentRoads)
                {
                    Gizmos.DrawWireSphere(TileManager.Instance.CenteredCellToWorld(neighbor), .5f);
                }
                Gizmos.DrawWireSphere(TileManager.Instance.CenteredCellToWorld(this.position), 1f);
            }
        }
    }
}
