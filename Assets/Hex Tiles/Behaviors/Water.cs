using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TileMechanics.Behavior {
    public class Water : LandBehavior
    {
        // Start is called before the first frame update
        new private void Awake()
        {
            baseItemChangePerCycle[TileItem.ID("water")] += 500;
            itemsMax[TileItem.ID("water")] += 450;
            base.Awake();
        }
        // Update is called once per frame
        void Update()
        {

        }
    } 
}
