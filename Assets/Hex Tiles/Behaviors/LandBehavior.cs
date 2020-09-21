using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;


namespace TileMechanics.Behavior {
    abstract public class LandBehavior : TileBehavior
    {
        private TileTemplate buildingTemplate;
        //Why did I do this? It would have been so much easier if the building had ownership somehow
        public BuildingBehavior BuiltBuilding;
        protected List<TileTemplate> forbiddenBuildings = new List<TileTemplate>();

        public int BuildingTimer = -1;
        private Vector3 worldPosition;


        public override void Initialzie(TileTemplate self, Vector2Int position, Vector2Int[] adjacent)
        {
            //Default forbidden buildings
            var Templates = TileManager.Instance.Templates;
            if(Templates.ContainsKey("Mine"))
                forbiddenBuildings.Add(Templates["Mine"]);

            worldPosition = TileManager.Instance.CenteredCellToWorld(position);
            base.Initialzie(self, position, adjacent);
        }
        public override void SecondaryInitialize()
        {
            if (BuiltBuilding != null)
                BuiltBuilding.SecondaryInitialize();
            base.SecondaryInitialize();
        }
        public override void TertiaryInitialize()
        {
            if (BuiltBuilding != null)
                BuiltBuilding.TertiaryInitialize();
            base.TertiaryInitialize();
        }
        public override void OnTurnEnd()
        {
            base.OnTurnEnd(); 

            if (BuiltBuilding != null)
            {
                BuiltBuilding.OnTurnEnd();
            }

            BuildingTimer -= 1;
            if (BuildingTimer == 0)
            {
                FinishBuilding();
                BuildingTimer = -1;
            }
        }
        public override void AfterTurnEnd()
        {
            if (BuiltBuilding != null)
            {
                BuiltBuilding.AfterTurnEnd();
            }
        }

        public virtual bool StartBuilding(TileTemplate building, int buildtime)
        {
            if (forbiddenBuildings.Contains(building))
            {
                return false;
            }
            if(BuiltBuilding != null)
            {
                DestroyBuilding();
            }
            if (building.name.Contains("Castle"))
            {
                Debug.LogError("Too many castles! Only one castle should exist. It's the player's piece!");
            }
            buildingTemplate = building;
            BuildingTimer = buildtime;
            return true;
        }

        protected void FinishBuilding()
        {
            BuiltBuilding = buildingTemplate.InitializeTile(this.position, this.neighbors) as BuildingBehavior;
            BuiltBuilding.LandUnderBuilding = this;

            float distanceToPlaceBelowGrid = 50;
            Debug.Log(this.name + this.position + " finished building template " + buildingTemplate.name);
            if (!buildingTemplate.name.Contains("Road"))
                this.transform.position = worldPosition - new Vector3(0, distanceToPlaceBelowGrid, 0);
        }

        public void DestroyBuilding()
        {
            if (BuiltBuilding != null)
            {
                BuiltBuilding.PrepareForDestruction();
                Destroy(BuiltBuilding.gameObject);
                BuiltBuilding = null;
                this.transform.position = worldPosition;
                buildingTemplate = null;
            }
            else
                CancelBuilding();
        }

        public void CancelBuilding()
        {
            buildingTemplate = null;
            BuildingTimer = -1;
        }

        public int[] GetTileTotalItemCounts()
        {
            int[] counts = new int[TileItem.Count];
            for(int i = 0; i < counts.Length; i++)
            {
                int totalCountForThisItem = BuiltBuilding == null ? this.getItemCount(i) : this.getItemCount(i) + BuiltBuilding.getItemCount(i);
                counts[i] += totalCountForThisItem;
            }
            return counts;
        }

        private void OnGUI()
        {
            string s = "";
            if(BuildingTimer > 0)
            {
                s = "Building\n" + buildingTemplate.name + "\nin: " + BuildingTimer;
            }
            Vector3 screen = Camera.main.WorldToScreenPoint(this.UIPoint.transform.position);
            UIText.text = s;
        }
    }
}
