using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Priority_Queue;
using TileMechanics.Behavior;

public class DijkstraNode : FastPriorityQueueNode
{
    public Vector2Int coordinates;
    public TileBehavior behavior;
    public int distance;
    public DijkstraNode from;
}
