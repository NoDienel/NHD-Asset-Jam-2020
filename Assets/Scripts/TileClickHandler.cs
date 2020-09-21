using UnityEngine;
using TileMechanics.Behavior;
using TileMechanics;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using System.Collections.Generic;
/// <summary>
/// Attach to main camera for convenient raycasting shenanigans
/// </summary>
public class TileClickHandler : MonoBehaviour
{
    public TileInventoryManager TileSelector;
    public TextManager HintText;
    public UnityEvent HitTile = new UnityEvent();
    public GameObject mostRecentHitTile;

    public TileBehavior mostRecentBehavior;


    new Camera camera;
    private void Start()
    {
        camera = this.GetComponent<Camera>();
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Mouse0))
        {
            Ray ray = camera.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;
            if (Physics.Raycast(ray, out hit, 25f))
            {
                //Only try to start building if the mouse is not over a UI element
                if (hit.collider.gameObject.CompareTag("Tile"))
                {
                    Transform t = hit.collider.transform;
                    {
                        mostRecentHitTile = t.root.gameObject;
                        mostRecentBehavior = mostRecentHitTile.GetComponent<TileBehavior>();

                        LandBehavior hitLand = null;
                        if (mostRecentBehavior.GetType().IsSubclassOf(typeof(BuildingBehavior)))
                        {
                            BuildingBehavior building = mostRecentBehavior as BuildingBehavior;
                            hitLand = building.LandUnderBuilding;
                        }
                        else if (mostRecentBehavior.GetType().IsSubclassOf(typeof(LandBehavior)))
                        {
                            hitLand = mostRecentBehavior as LandBehavior;
                        }
                        else
                        {
                            Debug.LogError("Land hit is not of standard types");
                        }
                        string TileSelected = TileSelector.currentlySelectedInventoryItem.template.name;
                        Debug.Log("Tile selected is " + TileSelected);
                        if (TileSelected.Contains("Look"))
                        {
                            //do nothing
                        } 
                        else if (TileSelected.Contains("Destroy"))
                        {
                            Debug.Log("Destroy tile's building");
                            hitLand.DestroyBuilding();
                        }
                        else
                        {
                            TileTemplate template = TileSelector.currentlySelectedInventoryItem.template;
                            if (hitLand.StartBuilding(template, template.buildtime))
                            {
                                //Good, building has successfully started
                            }
                            else
                            {
                                HintText.UpdateText("Cannot place building type <" + template.name + "> on top of tile type <" + mostRecentBehavior.name + ">");
                            }
                        }

                        HitTile.Invoke();
                    }
                }
            }
        }
    }

    /// <summary>
    /// Got this Code from zzeeshann on answers.unity
    /// </summary>
    /// <returns></returns>
    private bool IsPointerOverUIObject()
    {
        PointerEventData eventDataCurrentPosition = new PointerEventData(EventSystem.current);
        eventDataCurrentPosition.position = new Vector2(Input.mousePosition.x, Input.mousePosition.y);
        List<RaycastResult> results = new List<RaycastResult>();
        EventSystem.current.RaycastAll(eventDataCurrentPosition, results);
        return results.Count > 0;
    }
}
