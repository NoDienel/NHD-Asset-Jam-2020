using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TileMechanics.Behavior {
    /// <summary>
    /// Keeps the time in 'cycles' or turns
    /// </summary>
    public class TimeKeeper : MonoBehaviour
    {
        public static TimeKeeper Instance;

        public int startCycle = 0;
        public int currentCycle = 0;

        // Start is called before the first frame update
        void Start()
        {
            if (Instance != null)
            {
                Destroy(this.gameObject);
            }
            else
            {
                Instance = this;
            }
        }

        public void NextDay()
        {
            DailyBehaviors();
            currentCycle++;
        }

        private void DailyBehaviors()
        {
            //Cache this  not just for performance but also so we can change TileManager.Instance.Tiles by initializing a new tile if a tile 'builds'
            var tiles = TileManager.Instance.Tiles.Values;
            Debug.Log("Time Keeper is activating On Turn End behaviors");
            foreach (TileBehavior tile in tiles)
            {
                tile.OnTurnEnd();
            }
            Debug.Log("Time Keeper is activating After Turn End behaviors");
            foreach (TileBehavior tile in tiles)
            {
                tile.AfterTurnEnd();
            }
            TileManager.Instance.SetState();
        }

        // Update is called once per frame
        void Update()
        {

        }
    } 
}
