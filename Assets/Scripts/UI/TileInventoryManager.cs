using System.Collections.Generic;
using UnityEngine;
using System.Reflection;
using System;
using System.Linq;
using UnityEngine.UI;
using UnityEngine.EventSystems;

namespace TileMechanics.Behavior
{
    /// <summary>
    /// This class sets up the runtime tile-editor
    /// </summary>
    public class TileInventoryManager : MonoBehaviour
    {
        public GameObject buttonTemplate;
        public TileInventoryButton lookButton;
        public TileInventoryButton destroyButton;
        public Transform content;
        public Image currentDisplay;

        public TileInventoryButton currentlySelectedInventoryItem;
        private List<TileInventoryButton> allInventoryItems = new List<TileInventoryButton>();

        public void SecondaryInitialize()
        {
            allInventoryItems.Add(lookButton);
            currentlySelectedInventoryItem = allInventoryItems[0];
            currentDisplay.sprite = currentlySelectedInventoryItem.sprite;

            allInventoryItems.Add(destroyButton);
            //index 0 is the look tool tile, index 1 is the destroy tile
            int index = 2;
            foreach (TileTemplate Template in TileManager.Instance.Templates.Values)
            {
                if (Template.Behavior == null) Template.Behavior = typeof(Destroy);
                if (Template.Behavior.IsSubclassOf(typeof(BuildingBehavior)))
                {
                    Debug.Log(Template.name + " Inherits from BuildingBehavior");

                    if (Template.name == "Default")
                    {
                        Debug.Log("Do not create a UI Tile element for the default tile type");
                        return;
                    }
                    GameObject newButton = Instantiate(buttonTemplate);
                    TileInventoryButton newInventoryTile = newButton.AddComponent<TileInventoryButton>();

                    newInventoryTile.template = Template;
                    Sprite newSprite = getSpriteFromFile(Template.name);
                    newButton.transform.GetChild(0).GetComponent<Image>().sprite = newSprite;
                    newInventoryTile.sprite = newSprite;
                    newButton.transform.SetParent(content);

                    //Yes I have to do this or else it'll pass a referecnce of index
                    int i = index;
                    newButton.GetComponent<Button>().onClick.AddListener(delegate { this.ButtonPushed(i); });
                    
                    allInventoryItems.Add(newInventoryTile);

                    index++;
                }
            }

        }
        public void ButtonPushed(int ID)
        {
            Debug.Log("Button with ID " + ID + " was just pushed");
            currentlySelectedInventoryItem = allInventoryItems[ID];
            currentDisplay.sprite = currentlySelectedInventoryItem.sprite;
        }

        private Sprite getSpriteFromFile(string filename)
        {

            filename = filename.ToLower();

            //retrieve first entry of getfiles
            Sprite[] data = Resources.LoadAll<Sprite>("Tile Sprites");

            foreach (Sprite s in data)
            {
                if (s.name.Contains(filename))
                {
                    Debug.Log(filename + " Name of loaded sprite " + s.name);
                    return s;
                }
            }
            return null;
        }

        /// <summary>
        /// Borrowed this code from Jacobs Data Solutions at stack overflow
        /// Will find all the inheriting classes from a base class
        /// Modified somewhat to work with monobehaviors
        /// </summary>
        public static class ReflectiveEnumerator
        {
            static ReflectiveEnumerator() { }

            public static IEnumerable<Type> GetEnumerableOfType<T>()
            {
                List<Type> objects = new List<Type>();
                foreach (Type type in
                    Assembly.GetAssembly(typeof(T)).GetTypes()
                    .Where(myType => myType.IsClass && !myType.IsAbstract && myType.IsSubclassOf(typeof(T))))
                {
                    objects.Add(type);
                }
                return objects;
            }
        }
    }
}