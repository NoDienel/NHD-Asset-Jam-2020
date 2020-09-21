using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace TileMechanics.Behavior {
    using Coordinates = Vector2Int;
    /// <summary>
    /// Keeping this here in the hopes that I'll eventually store a path between the castle and buildings. Which is much more efficient as far as not having to seek out every patch of dirt but also allows for some fun pathing stuff. 
    /// <para>There's a good chance I won't have the time, though.</para>
    /// </summary>
    public struct Connection
    {
        TileBehavior endBehavior;
        TileBehavior[] path;
        public Connection(TileBehavior end, TileBehavior[] path)
        {
            endBehavior = end;
            this.path = path;
        }
    }


    public class Castle : BuildingBehavior
    {
        //Yes I made a singleton. Yes Im running out of time
        public static Castle Instance;

        /// <summary>
        /// Deliberately set to null to initialize in a special way
        /// LandBehavior = unique tile, Road = the road that connected the tile to the castle
        /// </summary>
        public Dictionary<LandBehavior, Road> allConnectedTiles = null;
        public List<Road> allConnectedRoads = new List<Road>();


        public override void Initialzie(TileTemplate self, Coordinates position, Coordinates[] adjacent)
        {
            if(Instance == null)
            {
                Instance = this;
            }
            else
            {
                //Not good, will leave behind children I think
                //But this situation should be very rare/impossible
                Destroy(Instance.gameObject);
                Instance = this;
            }
            base.Initialzie(self, position, adjacent);
            for (int i = 0; i < itemsMax.Length; ++i)
            {
                itemsMax[i] = 250;
            }
        }

        //Pathfinding must be done after all the roads have been connected
        public override void TertiaryInitialize()
        {
            base.TertiaryInitialize();
            Debug.Log("Calculating Connections to Castle");
            RecalculateAllConnections();
        }
        public override void OnTurnEnd()
        {
            base.OnTurnEnd();
        }
        public override void AfterTurnEnd()
        {

            Debug.Log("After turn Castle behavior is running");
            //If this don't work I dunno what will
            RecalculateAllConnections();

            if (allConnectedTiles == null)
            {
                Debug.Log("Recalculating all tiles connected to castle");
                CollectItemsAndGetConnectedTiles();
                GiveItems();
            }
            else
            {
                Debug.Log("Don't recalculate, just collect");
                CollectItems();
                GiveItems();
            }

            base.AfterTurnEnd();
        }

        public override int getItemChangePerCycle(int itemID)
        {
            int totalChange = 0;
            if (allConnectedTiles != null && allConnectedTiles.Count > 0)
            {
                foreach (LandBehavior land in allConnectedTiles.Keys)
                {
                    int change = 0;
                    change += land.getItemChangePerCycle(itemID);
                    if (land.BuiltBuilding != null)
                        change += land.BuiltBuilding.getItemChangePerCycle(itemID);

                    totalChange += change;
                }
            }
            return totalChange;
        }

        /// <summary>
        /// Gets a table of all the tiles connected to all the roads connected to your castle, then collects roadPower number of items from them
        /// Will only be called whenever the roadsystem updates
        /// </summary>
        private void CollectItemsAndGetConnectedTiles()
        {
            if (allConnectedTiles == null)
            {
                allConnectedTiles = new Dictionary<LandBehavior, Road>();
            }
            Dictionary<Coordinates, int[]> values = new Dictionary<Coordinates, int[]>();

            Debug.Log("Collection Phase begun: ");
            Debug.Log("Number of roads to check: " + allConnectedRoads.Count);
            foreach (Road r in this.allConnectedRoads)
            {
                r.neighbors = TileManager.Instance.GetNeighbors(r.position);
                r.findAdjacentRoads();
                //Debug.Log("Road " + r.name + r.GetInstanceID() + " is being queried");
                foreach (Coordinates n in r.neighbors)
                {
                    if (n != this.position)
                    {
                        //Debug.Log("Neighbor at " + n + "is being queried");
                        LandBehavior neighbor = TileManager.Instance.Tiles[n] as LandBehavior;
                        if (neighbor != null && !values.ContainsKey(n))
                        {
                            //Debug.Log("Neighbor " + neighbor.name + " is not null and is not part of the list");
                            int[] vals = TileItem.ItemList();
                            for (int i = 0; i < vals.Length; ++i)
                            {
                                //neighbor.ChangeItemCount returns the 'remaineder' so if it draws below zero roadStrength will be reduced by however much it went over
                                vals[i] += r.roadStrength + neighbor.changeItemCount(-r.roadStrength, i);
                                if (neighbor.BuiltBuilding != null)
                                    vals[i] += r.roadStrength + neighbor.BuiltBuilding.changeItemCount(-r.roadStrength, i);
                                if (!allConnectedTiles.ContainsKey(neighbor))
                                    allConnectedTiles.Add(neighbor, r);
                            }
                            values.Add(n, vals);

                        }
                    }
                }
            }
            Debug.Log("Total number of tiles queried: " + allConnectedTiles.Count);
            //Add accumulated values to the castle's holds
            foreach (int[] vals in values.Values)
            {
                for (int i = 0; i < vals.Length; ++i)
                {
                    this.changeItemCount(vals[i], i);
                }
            }

            Debug.Log("Collection Phase Over");
        }

        private void CollectItems()
        {
            List<int[]> values = new List<int[]>();
            foreach (LandBehavior land in allConnectedTiles.Keys)
            {
                Road from = allConnectedTiles[land];
                int[] vals = TileItem.ItemList();
                for (int i = 0; i < vals.Length; ++i)
                {
                    vals[i] += from.roadStrength + land.changeItemCount(-from.roadStrength, i);
                    if (land.BuiltBuilding != null)
                        vals[i] += from.roadStrength + land.BuiltBuilding.changeItemCount(-from.roadStrength, i);
                }
                values.Add(vals);
            }
        }

        private void GiveItems()
        {
            foreach (LandBehavior land in allConnectedTiles.Keys)
            {
                BuildingBehavior building = land.BuiltBuilding;
                building.RequestItemsFromCastle(this);
            }
        }

        /// <summary>
        /// Expensive. Should only be done once at the beginning, otherwise be smarter about which connections to regenerate by using path
        /// Should also only be done at the end of a cycle because allconnectedtiles may be needed
        /// </summary>
        public void RecalculateAllConnections()
        {
            allConnectedRoads.Clear();
            if (allConnectedTiles != null)
                allConnectedTiles.Clear();
            allConnectedTiles = null;
            Debug.Log("Recalculation begun --------------\n tiles: " + TileManager.Instance.Tiles.Values.Count);
            foreach (TileBehavior tile in TileManager.Instance.Tiles.Values)
            {
                //Debug.Log("Recalculating neighbors for " + tile.name + tile.position);
                tile.neighbors = TileManager.Instance.GetNeighbors(tile.position);
                //Debug.Log("Checking tile " + tile.name + tile.position + " for road.");
                //Debug.Log("Type = " + tile.GetType());


                if (tile.GetType().IsSubclassOf(typeof(LandBehavior)))
                {
                    //Debug.Log(tile.name + tile.position + " is subclass of LandBehavior");
                    LandBehavior land = tile as LandBehavior;
                    if (land.BuiltBuilding != null && land.BuiltBuilding.name.ToLower().Contains("road"))
                    {
                        //Debug.Log("Land tile " + tile.name + " has a road");
                        Road r = land.BuiltBuilding as Road;
                        r.neighbors = TileManager.Instance.GetNeighbors(r.position);
                        r.findAdjacentRoads();
                        Debug.Log("Testing Castle-connection for " + r.name + r.position);
                        if (isConnected(r))
                        {
                            //Debug.Log(tile.name + tile.position + " has a Road");
                            if (!allConnectedRoads.Contains(r))
                                allConnectedRoads.Add(r);
                        }
                    }
                }
                else if (tile.GetType().IsSubclassOf(typeof(BuildingBehavior)))
                {
                    //Debug.Log(tile.name + tile.position + " is subclass of BuildingBehavior");
                    BuildingBehavior building = tile as BuildingBehavior;
                    if (building.name.ToLower().Contains("road"))
                    {
                        Road r = building as Road;
                        if (isConnected(r))
                        {
                            //Debug.Log(tile.name + tile.position + " is a Road");
                            if (!allConnectedRoads.Contains(r))
                                allConnectedRoads.Add(r);
                        }
                    }
                }
                else
                {
                    //Debug.LogError("Recalculation has found a null tile. This should be impossible");
                }
            }
            Debug.Log("-------Connected Roads detected: " + allConnectedRoads.Count);
        }

        public  bool isConnected(Road road)
        {
            road.neighbors = TileManager.Instance.GetNeighbors(road.position);
            road.findAdjacentRoads();

            Debug.Log("Adjacent roads to the castle: " + this.adjacentRoads.Count);
            if (this.adjacentRoads.Count > 0)
            {
                Coordinates start = this.adjacentRoads[0];
                Coordinates end = road.position;
                TileBehavior[] pathTo = FindPathTo(start, end);
                if (pathTo != null)
                {
                    Debug.Log("Connection to castle found for road at " + road.position);
                    return true;
                }
            }
            else
                Debug.Log("No roads found next to castle yet");
            return false;
        }

        /*
         * Retired for not really being what I want
        protected void CalculateConnection(TileBehavior end)
        {
            //If you pass another building into this function;
            if (end != this
                && end.GetType().IsSubclassOf(typeof(BuildingBehavior))
                )
            {
                Debug.Log("Testing " + end.name + " for connections to " + this.name);
                BuildingBehavior building = end as BuildingBehavior;
                if (this.adjacentRoads.Count > 0 && building.adjacentRoads.Count > 0)
                {
                    Coordinates firstConnectedRoad = building.adjacentRoads[0];
                    TileBehavior[] pathTo = FindPathTo(firstConnectedRoad);
                    if (pathTo != null)
                    {
                        Connections.Add(new Connection(building, pathTo));
                        Debug.Log("Connection found between " + this.name + " and " + end.name + "\n" +
                            "Path at " + pathTo);
                    }
                    else
                    {
                        Debug.Log("No connection found between " + this.name + " and " + end.name);
                    }
                }
                else
                {
                    Debug.Log("There are no roads connected to one of the provided tiles\n" +
                        "Therefor no connection possible between " + this.name + " and " + end.name);
                }
            }
        }*/

        protected TileBehavior[] FindPathTo(Coordinates startRoad, Coordinates endRoad)
        {
            Debug.Log("Begun solving for road tile " + endRoad);
            DijkstraSolver pathSolver = new DijkstraSolver();
            var tiles = TileManager.Instance.Tiles;
            //Only buildingbehaviors are guarenteed to have well-set nodes
            TileBehavior start, end;
            LandBehavior tempBelow;
            tempBelow = tiles[startRoad] as LandBehavior;
            start = tempBelow.BuiltBuilding;
            tempBelow = tiles[endRoad] as LandBehavior;
            Debug.Log("Programatic location of end " + tempBelow.position);
            if (tempBelow.BuiltBuilding != null)
                end = tempBelow.BuiltBuilding;
            else
                end = tempBelow;
            Debug.Log("This better be the same " + end.position);
            return pathSolver.Solve(start.node, end.node);
        }

        // Update is called once per frame
        void Update()
        {

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
                }
            }
            return sb.ToString();
        }
    }
}
