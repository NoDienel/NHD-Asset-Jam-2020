using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TileMechanics.Behavior {
    public class Default : Grass
    {
        // Start is called before the first frame update
        public override void Initialzie(TileTemplate self, Vector2Int position, Vector2Int[] adjacent)
        {
            this.name = "Grass";
            base.Initialzie(self, position, adjacent);
        }

        // Update is called once per frame
        void Update()
        {

        }
    } 
}
