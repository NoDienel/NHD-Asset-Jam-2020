using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TileMechanics.Behavior {
    public class Forest : LandBehavior
    {
        // Start is called before the first frame update
        new private void Awake()
        {
            baseItemChangePerCycle[TileItem.ID("wood")] += 5;
            itemsMax[TileItem.ID("wood")] += 50;
            base.Awake();
        }

        // Update is called once per frame
        void Update()
        {

        }
    } 
}
