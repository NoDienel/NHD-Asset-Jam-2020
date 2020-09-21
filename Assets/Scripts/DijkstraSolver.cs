using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using Priority_Queue;

namespace TileMechanics.Behavior
{
    using coordinates = Vector2Int;
    /// <summary>
    /// Works as one would expect for a shortest path algorithm
    /// Happened to be learning about this in AI and had already messed with it a bit
    /// </summary>
    public class DijkstraSolver
    {
        static int arbitraryMaxNodes = 60;
        FastPriorityQueue<DijkstraNode> queue = new FastPriorityQueue<DijkstraNode>(arbitraryMaxNodes);
        List<DijkstraNode> offQueue = new List<DijkstraNode>();

        /// <summary>
        /// Returns an array with the shortest path between two nodes
        /// <para>Returns null if there is no connection found</para>
        /// </summary>
        /// <param name="start">first node</param>
        /// <param name="end">end node</param>
        /// <returns></returns>
        public TileBehavior[] Solve(DijkstraNode start, DijkstraNode end)
        {
            if (start == end)
            {
                return new TileBehavior[] { end.behavior };
            }
            start.distance = 0;
            this.queue.ResetNode(start);
            queue.Enqueue(start, start.distance);
            Debug.Log("Entered Dijkstra with node " + queue.First.behavior + queue.First.behavior.position);
            Debug.Log("Heading for node " + end.behavior.name + end.behavior.position);
            while (queue.Count > 0)
            {
                Debug.Log("Endered Queue, size: " + queue.Count);
                DijkstraNode head = queue.Dequeue();
                offQueue.Add(head);
                queue.ResetNode(head);
                //Not good, sorta just trusting that everything is where it should be
                //but the deadline just keeps getting closer
                Road tile;

                tile = head.behavior as Road;

                string debugName = tile.name + tile.position;

                Debug.Log("Pop off " + debugName);

                Debug.Log(tile.adjacentRoads.Count + " Roads connected to " + debugName);
                //This. This line of code looking up roads at the last possible moment before checking is what finally fixed my road system
                tile.findAdjacentRoads();
                Debug.Log(tile.adjacentRoads.Count + " Roads connected to " + debugName + "After calculation");

                foreach (coordinates position in tile.adjacentRoads)
                {
                    Debug.Log("Checking connected coordinates " + position);
                    LandBehavior land = TileManager.Instance.Tiles[position] as LandBehavior;
                    DijkstraNode connectedRoad = land.BuiltBuilding.node;
                    if (!offQueue.Contains(connectedRoad) && !queue.Contains(connectedRoad))
                    {
                        connectedRoad.distance = head.distance + 1;
                        connectedRoad.from = head;
                        queue.Enqueue(connectedRoad, connectedRoad.distance);
                    }
                    else if (offQueue.Contains(connectedRoad))
                    {
                        //If the distance found through a 'solved' node is less than the node this came from, make it the new shortest path
                        if (connectedRoad.distance < head.from.distance)
                        {
                            head.distance = connectedRoad.distance + 1;
                            head.from = connectedRoad;
                        }
                    }
                    else if (queue.Contains(connectedRoad))
                    {
                        Debug.Log("Queue already contains value " + connectedRoad.behavior + connectedRoad.behavior.position);
                    }
                    //Debug.Log("Road " + tile.name + tile.position +
                    //    " has neighbor " + connectedRoad.behavior.name + connectedRoad.behavior.position +
                    //    " and the endpoint is " + end.behavior.name + end.behavior.position);
                    if (connectedRoad.behavior == end.behavior)
                    {
                        break;
                    }
                }
                
            }

            if (end.from == null)
            {
                Debug.LogWarning("No Path found; node not conntected");
            }
            else
            {
                List<TileBehavior> path = new List<TileBehavior>();
                DijkstraNode current = end;
                path.Add(current.behavior);
                while (current.from != null)
                {
                    DijkstraNode old = current;
                    current = current.from;
                    old.from = null;
                    path.Add(current.behavior);
                }
                //Path began at end and went to start
                path.Reverse();
                //Make sure we can reuse the end node because it never got reset
                queue.Clear(); 
                foreach (DijkstraNode node in queue)
                {
                    queue.ResetNode(node);
                }
                return path.ToArray();
            }
            //Null behavior if all else fails
            queue.Clear();
            foreach (DijkstraNode node in queue)
            {
                queue.ResetNode(node);
            }
            return null;
        }
    }
}
