using System.Collections.Generic;
using UnityEngine.Tilemaps;
using TileMechanics.Behavior;
using UnityEngine;

namespace TileMechanics
{
    using coordinates = UnityEngine.Vector2Int;
    public class TileManager : MonoBehaviour
    {
        //Singleton cuz I just need it in that many scripts
        public static TileManager Instance;

        //Set in inspector
        public TileInventoryManager TileInventory;

        /// <summary>
        /// It should be noted that this is just a list of the default map's 'land' tiles. 
        /// Building tiles are handled by the land tile themselves
        /// </summary>
        public Dictionary<coordinates, TileBehavior> Tiles = new Dictionary<coordinates, TileBehavior>();
        public Dictionary<coordinates, TileBehavior> TilesState = new Dictionary<coordinates, TileBehavior>();
        public Dictionary<string, TileTemplate> Templates = new Dictionary<string, TileTemplate>();
        public Tilemap map;
        public TileTemplate defaultTemplate;
        public Castle playerCastle;

        coordinates[] testneighbors;
        coordinates testself;

        // This should initialize everything
        // Many scripts require certain information to already be loaded and this script is what primarily loads tiledata
        void Awake()
        {
            if (Instance != null)
                Destroy(this.gameObject);
            else
                Instance = this;

            map = this.GetComponent<Tilemap>();

            Object[] templates = Resources.FindObjectsOfTypeAll(typeof(TileTemplate));
            foreach(TileTemplate template in templates)
            {
                Templates.Add(template.name, template);
            }

            //Sketchy. Really should retrieve the 'Grass' TileTemplate
            //But there's no easy way to do that right now
            defaultTemplate = ScriptableObject.CreateInstance<TileTemplate>();

            BoundsInt bounds = map.cellBounds;
            Debug.Log("Bounds " + bounds);
            TileBase[] baseTiles = map.GetTilesBlock(bounds);

            for (int x = 0; x < bounds.size.x; ++x)
            {
                for (int y = 0; y < bounds.size.y; ++y)
                {
                    //Debug.Log("Checking Hex "+ x + ',' + y);
                    //Recieves all items in a linear array so y*boundsx increases x by one 'layer'
                    int index = x + y * bounds.size.x;

                    TileTemplate tile = baseTiles[index] as TileTemplate;
                    if (tile != null)
                    {
                        coordinates c = new coordinates(x, y);
                        TileBehavior clone = tile.InitializeTile(c, GetNeighbors(c));

                        if(!Tiles.ContainsKey(c))
                            Tiles.Add(c, clone);

                        if (clone.GetType().Equals(typeof(Castle)))
                        {
                            if (playerCastle == null)
                                playerCastle = clone as Castle;
                            else
                                Debug.LogError("Too many castles! Only one castle should exist. It's the player's piece!");
                        }

                    }
                }
            }

            Debug.Log("Tile Map Loading Finished-------------- Tiles Loaded: " + Tiles.Count +
                "\nIndividual tiles will now do secondary initialization, mostly relating to node-building");
            List<TileBehavior> behaviors = new List<TileBehavior>(Tiles.Values);
            for (int i = 0; i < Tiles.Count; ++i)
            {
                behaviors[i].SecondaryInitialize();
            }

            Debug.Log("Secondary Initialization finished --------\n"+
                "Certain tiles will do tertiary initialization relating to pathing \n" +
                "Which was impossible to carry out until the entire node network had been completed");

            behaviors = new List<TileBehavior>(Tiles.Values);
            for (int i = 0; i < Tiles.Count; ++i)
            { 
                behaviors[i].TertiaryInitialize();
            }

            if (TileInventory != null)
            {
                TileInventory.SecondaryInitialize();
            }
            else
                Debug.LogError("Error: Tile Inventory not found, please set in inspector");

            Debug.Log("All Loading has finished ------------");
        }

        public void SetState()
        {
            TilesState = new Dictionary<coordinates, TileBehavior>(Tiles);
        }

        public void ResetState()
        {
            Tiles = new Dictionary<coordinates, TileBehavior>(TilesState);
        }

        /// <summary>
        ///Make an array of neighbors.
        ///Index 0 is up right, following a clockwise rotation.
        ///<para>These values are found according to unity's interal 'offset coordinate' hexagonal system. It's messy but it works and is only done once when the tile is first loaded in</para>
        /// </summary>
        /// <param name="pos">the vector2int coordinate position of the tile you want to find the neighbors of.</param>
        /// <returns></returns>
        public coordinates[] GetNeighbors(coordinates pos)
        {
            coordinates[] neighbors = new coordinates[6];
            //Axial array (Not good but quick)
            //mod is a modifier that changes the result based on if you're on an even row or an odd one
            int mod = pos.y % 2 == 0 ? 0 : -1;
            neighbors[0] = new coordinates(
                pos.x + mod + 1,
                pos.y - 1);
            neighbors[1] = new coordinates(
                pos.x + 1,
                pos.y);
            neighbors[2] = new coordinates(
                pos.x + mod + 1,
                pos.y + 1);
            neighbors[3] = new coordinates(
                pos.x + mod,
                pos.y + 1);
            neighbors[4] = new coordinates(
                pos.x - 1,
                pos.y);
            neighbors[5] = new coordinates(
                pos.x + mod,
                pos.y - 1);
            if (pos.y % 2 == 0)
            {
                testneighbors = neighbors;
                testself = pos;
            }

            return neighbors;
        }

        //Centers the cellToWorld by subtracting the bounds, which the world coordinates are ofset by
        public Vector3 CenteredCellToWorld(coordinates pos)
        {
            return map.CellToWorld((Vector3Int)(pos + new coordinates(map.cellBounds.xMin, map.cellBounds.yMin)));
        }

        // Update is called once per frame
        void Update()
        {

        }

        private void OnDrawGizmos()
        {
            if (false && testneighbors != null && map != null)
            {
                foreach (coordinates neighbor in testneighbors)
                {
                    Gizmos.DrawWireSphere(CenteredCellToWorld(neighbor), .5f);
                }
                Gizmos.DrawWireSphere(CenteredCellToWorld(testself), 1f);
            }
        }
    }
}