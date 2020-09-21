using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

namespace TileMechanics.Behavior
{
    using Coordinates = Vector2Int;
    abstract public class BuildingBehavior : TileBehavior
    {
        public LandBehavior LandUnderBuilding;
        public List<Coordinates> adjacentRoads = new List<Coordinates>();

        public override void Initialzie(TileTemplate self, Coordinates position, Coordinates[] adjacent)
        {
            base.Initialzie(self, position, adjacent);
            //Do this after initialization


        }

        //Roadfinding must be done after all the roads have loaded in
        public override void SecondaryInitialize()
        {
            if (LandUnderBuilding == null)
            {
                Debug.Log("No land under " + this.name + ", generating default terrain at + " + this.position);


                LandUnderBuilding = TileManager.Instance.defaultTemplate.InitializeTile(this.position, this.neighbors) as LandBehavior;
                Vector3 position = TileManager.Instance.CenteredCellToWorld(this.position);
                if (!this.name.ToLower().Contains("road"))
                {
                    position += Vector3.down * 50f;
                }
                LandUnderBuilding.gameObject.transform.position = position;
                //The bottom land should always be what the map is tracking
                //Buildings are built on top of that land
                //I somewhat regret this early design decision
                TileManager.Instance.Tiles[this.position] = LandUnderBuilding;
                //this.transform.parent = LandUnderBuilding.transform;
                LandUnderBuilding.BuiltBuilding = this;
            }
            findAdjacentRoads();
        }

        public override void TertiaryInitialize()
        {

        }

        public void findAdjacentRoads()
        {
            adjacentRoads.Clear();
            neighbors = TileManager.Instance.GetNeighbors(this.position);
            for (int i = 0; i < 6; ++i)
            {
                Coordinates neighbor = neighbors[i];
                TileBehavior tileBehavior;
                if (TileManager.Instance.Tiles.TryGetValue(neighbor, out tileBehavior))
                {
                    //Debug.Log("Road " + this.name + " is checking neighbor  " + tileBehavior + " for road connection");
                    if (tileBehavior.GetType().IsSubclassOf(typeof(BuildingBehavior)) && tileBehavior.name.ToLower().Contains("road"))
                    {
                        adjacentRoads.Add(neighbor);
                        Debug.Log(this.name + " at " + this.position + " found adjacent road at " + neighbor);
                    }
                    else if (tileBehavior.GetType().IsSubclassOf(typeof(LandBehavior)))
                    {
                        LandBehavior land = tileBehavior as LandBehavior;
                        if (land.BuiltBuilding != null && land.BuiltBuilding.name.ToLower().Contains("road"))
                        {
                            adjacentRoads.Add(neighbor);
                            Debug.Log(this.name + " at " + this.position + " found adjacent road at " + neighbor);
                        }
                    }
                }
            }
        }

        public virtual void RequestItemsFromCastle(Castle castle) { }

        public virtual void PrepareForDestruction()
        {
            if (UI != null)
            {
                Destroy(UI.gameObject);
                Destroy(UIText.gameObject);
            }
        }
    }
}
