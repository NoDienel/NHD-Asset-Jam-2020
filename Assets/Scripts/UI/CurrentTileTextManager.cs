using UnityEngine;
using UnityEngine.UI;
using TileMechanics.Behavior;

public class CurrentTileTextManager : MonoBehaviour
{
    public TileClickHandler Clicker;
    private Text text;
    // Start is called before the first frame update
    void Start()
    {
        text = this.GetComponent<Text>();
    }

    public void UpdateText()
    {
        TileBehavior t = Clicker.mostRecentBehavior;
        text.text = "Cycle " + TimeKeeper.Instance.currentCycle + "\n";
        if(t != null)
            text.text += t.ToString();
    }
}
