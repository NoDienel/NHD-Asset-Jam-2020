using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TileMechanics.Behavior {
    public class Hill : LandBehavior
    {
        new private void Awake()
        {
            baseItemChangePerCycle[TileItem.ID("stone")] += 5;
            itemsMax[TileItem.ID("stone")] += 50;
            base.Awake();
        }

        // Update is called once per frame
        void Update()
        {

        }
    } 
}
