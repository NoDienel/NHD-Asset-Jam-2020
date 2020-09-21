using UnityEngine;
using UnityEngine.Tilemaps;
using System;
using System.IO;
using Dummiesman; //This is the worst name for a namespace ever but the OBJ loader works well
using TileMechanics.Behavior;
using System.Reflection;
using UnityEngine.UI;

namespace TileMechanics
{
    using Coordinates = UnityEngine.Vector2Int;

    /// <summary>
    /// A single Tile Object for each unique tile type
    /// Exists only to store data to be copied from
    /// <para>Example: A template exists for a 'grass' tile, which Tilemanager can ask as many times as it likes to assemble a ready 'Grass' gameobject complete with standard components and unique tile behaviors</para>
    /// </summary>
    [CreateAssetMenu(fileName = "Tile", menuName = "TileTemplate", order = 360)]
    public class TileTemplate : Tile
    {
        public float UIHoverDist = .8f;
        public int buildtime = 1;

        public Type Behavior;
        [HideInInspector]
        public GameObject Geometry;
        [HideInInspector]
        public Mesh CollisionMesh;
        [HideInInspector]


        private void Awake()
        {
            if (this.name == "" || this.name == "TileTemplate")
                this.name = "Grass";
            Initialize();
        }
        void Initialize()
        {
            LoadTemplateGeometry();
            FindTemplateBehaviorType();
        }

        /// <summary>
        /// Creates the GameObjects from the basemesh, and attaches the appropriate scripts to make it function as a TileBehavior
        /// <para>Then it returns the TileBehavior of that gameobject, which is what actually handles the 'in game' mechanics of the tile</para>
        /// </summary>
        /// <param name="position">tile array coordinates of the new tile</param>
        /// <param name="adjacent">an array of all the positions of all the tiles adjacent to the new tile</param>
        /// <returns></returns>
        public TileBehavior InitializeTile(Coordinates position, Coordinates[] adjacent)
        {
            //Debug.Log("Found " + this.name);
            if (Geometry == null) Initialize();
            GameObject newTile = ConstructTile(TileManager.Instance.CenteredCellToWorld(position));
            TileBehavior behavior = AttachBehavior(newTile, position, adjacent);

            behavior.UIPoint = new GameObject(this.name + "_UIPivotPoint");
            behavior.UIPoint.transform.parent = newTile.transform;
            behavior.UIPoint.transform.position = newTile.transform.position + Vector3.up * UIHoverDist;

            GameObject newCanvas = new GameObject("Canvas");
            behavior.UI = newCanvas.AddComponent<Canvas>();
            behavior.UI.transform.position = behavior.UIPoint.transform.position;
            CanvasScaler scale = newCanvas.AddComponent<CanvasScaler>();
            scale.scaleFactor = 10f;
            scale.dynamicPixelsPerUnit = 10f;
            newCanvas.AddComponent<GraphicRaycaster>();
            behavior.UI.transform.SetParent(behavior.UIPoint.transform);

            RectTransform newRectTransform = behavior.UI.GetComponent<RectTransform>();
            newRectTransform.localScale = new Vector3(.03f, .03f, .03f);
            behavior.UI.renderMode = RenderMode.WorldSpace;
            newRectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, 10);
            newRectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, 20); 
            behavior.UI.transform.position = behavior.UIPoint.transform.position;
            newRectTransform.ForceUpdateRectTransforms();

            GameObject UIText = new GameObject("Text");
            UIText.AddComponent<CanvasRenderer>();
            RectTransform TextRect = UIText.AddComponent<RectTransform>();
            TextRect.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, 10);
            TextRect.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, 20);
            TextRect.localScale = new Vector3(.03f, .03f, .03f);
            Text text = UIText.AddComponent<Text>();
            text.alignment = TextAnchor.MiddleCenter;
            text.horizontalOverflow = HorizontalWrapMode.Overflow;
            text.verticalOverflow = VerticalWrapMode.Overflow;
            Font ArialFont = (Font)Resources.GetBuiltinResource(typeof(Font), "Arial.ttf");
            text.font = ArialFont;
            text.fontSize = 7;
            //text.text = "Test";
            text.enabled = true;
            text.color = Color.black;
            UIText.transform.SetParent(behavior.UI.transform);
            UIText.transform.position = behavior.UIPoint.transform.position;
            behavior.UIText = text;

            return behavior;
        }

        private GameObject ConstructTile(Vector3 pos)
        {
            GameObject tile = Instantiate(Geometry, pos, Quaternion.identity);
            tile.SetActive(true);
            tile.AddComponent<MeshFilter>().mesh = CollisionMesh;
            tile.AddComponent<MeshCollider>();
            tile.tag = "Tile";

            return tile;
        }

        private TileBehavior AttachBehavior(GameObject tile, Coordinates pos, Coordinates[] adj)
        {

            TileBehavior behavior;
            if (Behavior != null)
            {
                behavior = tile.AddComponent(Behavior) as TileBehavior;
                Debug.Log("This is the name of the  new behavior : " + behavior.name);

                //A bit sloppy, but essentially places all roads on top of the ground
                //In reality if I had more time I know there's definitely a better way of doing this.
                if (behavior.name == "Road_mesh(Clone)") tile.transform.position += new Vector3(0, .2f, 0);
                behavior.Initialzie(this, pos, adj);
            }
            else 
            {
                //Some default behaviour
                behavior = tile.AddComponent<Grass>();
                Debug.LogWarning("No behavior found on template " + this.name + "\nDefault loaded: " + behavior.name);
                behavior.Initialzie(this, pos, adj);
            }
            return behavior;
        }
        /// <summary>
        /// I don't think it's a very good idea to do this in general. It's a bit too rigid and based off of folder locations, but it's quick
        /// </summary>
        private void LoadTemplateGeometry()
        {
            Debug.Log("Loading Geo for " + this.name);
            string folderPath = Application.dataPath + "/Hex Tiles/Meshes/";
            string[] filesPaths = Directory.GetFiles(folderPath, "*" + this.name.ToLower() + "*" + ".obj");
            foreach (string filePath in filesPaths)
            {
                if (File.Exists(filePath))
                {
                    //Debug.Log("File successfully found for " + this.name + " at " + filePath);
                    OBJLoader GameObjectLoader = new OBJLoader();
                    Geometry = GameObjectLoader.Load(filePath);
                    CollisionMesh = new Mesh();
                    CollisionMesh.name = "CollisionMesh" + this.name;
                    ObjImporter MeshLoader = new ObjImporter();
                    CollisionMesh = MeshLoader.ImportFile(filePath);
                    break;
                }
            }
            if (Geometry == null) Debug.LogError("Geometry not found: Make sure it's added to the meshes and correctly named");
            Geometry.name = this.name + "_mesh";
            Geometry.SetActive(false);
        }

        private void FindTemplateBehaviorType()
        {
            Behavior = GetType("TileMechanics.Behavior." + this.name);
            if (Behavior == null)
            {
                Debug.LogError("Behavior completely failed to load for " + this.name);
                Behavior = Type.GetType(this.name);
            }
            Debug.Log("Retrieving behavior for " + this.name + '=' + Behavior);
        }

        /// <summary>
        /// Got this off the internet from a friendly folk at stackexchange
        /// </summary>
        /// <returns></returns>
        public static Type GetType(string TypeName)
        {

            // Try Type.GetType() first. This will work with types defined
            // by the Mono runtime, in the same assembly as the caller, etc.
            var type = Type.GetType(TypeName);

            // If it worked, then we're done here
            if (type != null)
                return type;

            // If the TypeName is a full name, then we can try loading the defining assembly directly
            if (TypeName.Contains("."))
            {

                // Get the name of the assembly (Assumption is that we are using 
                // fully-qualified type names)
                var assemblyName = TypeName.Substring(0, TypeName.IndexOf('.'));

                // Attempt to load the indicated Assembly
                var assembly = Assembly.Load(assemblyName);
                if (assembly == null)
                    return null;

                // Ask that assembly to return the proper Type
                type = assembly.GetType(TypeName);
                if (type != null)
                    return type;

            }

            // If we still haven't found the proper type, we can enumerate all of the 
            // loaded assemblies and see if any of them define the type
            var referencedAssemblies = System.AppDomain.CurrentDomain.GetAssemblies();
            foreach (var assemblyName in referencedAssemblies)
            {

                // Load the referenced assembly
                if (assemblyName != null)
                {
                    // See if that assembly defines the named type
                    type = assemblyName.GetType(TypeName);
                    if (type != null)
                        return type;
                }
            }

            // The type just couldn't be found...
            Debug.LogError("Could not find type specified by string '" + TypeName +"'");
            return null;

        }

        public override void RefreshTile(Vector3Int position, ITilemap tilemap)
        {
            base.RefreshTile(position, tilemap);
        }

        public override void GetTileData(Vector3Int position, ITilemap tilemap, ref TileData tileData)
        {
            base.GetTileData(position, tilemap, ref tileData);
        }


    }
}

