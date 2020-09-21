using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

namespace TileMechanics.Behavior
{
    abstract public class House : BuildingBehavior
    {
        new private void Awake()
        {
            itemsMax[TileItem.ID("housing")] = 50;
        }

    }
}
