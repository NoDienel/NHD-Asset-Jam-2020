using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

namespace TileMechanics.Behavior
{
    public class Mill : BuildingBehavior
    {
        new private void Awake()
        {
            baseItemChangePerCycle[TileItem.ID("water")] += 500;
            itemsMax[TileItem.ID("water")] += 450;
            base.Awake();
        }

        public override void RequestItemsFromCastle(Castle castle)
        {
            int grain = TileItem.ID("grain");
            int takeGrain = -20;
            //take up to takeGrain grain
            int takengrainLeftover = castle.changeItemCount(takeGrain, grain);
            int takengrainCount = -20 - takengrainLeftover;
            int givenGrainCount = -takengrainCount;
            //add as much of that grain to the mill as possibe
            int givengrainLeftover = this.changeItemCount(givenGrainCount, grain);
            //return what cannot fit into the mill to the castle
            castle.changeItemCount(givengrainLeftover, grain);
        }
    }
}
