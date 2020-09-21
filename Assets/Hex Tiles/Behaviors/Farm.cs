using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TileMechanics.Behavior
{
    public class Farm : BuildingBehavior
    {
        // Start is called before the first frame update
        new void Awake()
        {
            baseItemChangePerCycle[TileItem.ID("grain")] += 20;
            itemsMax[TileItem.ID("grain")] += 150;
            base.Awake();
        }

        // Update is called once per frame
        void Update()
        {

        }
    }
}
