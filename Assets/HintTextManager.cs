using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HintTextManager : TextManager
{
    public float Delay = 5f;
    override public void UpdateText(string s)
    {
        base.UpdateText(s);
        StartCoroutine(waitThenBlank(s));
    }
    IEnumerator waitThenBlank(string s)
    {
        yield return new WaitForSeconds(Delay);
        base.UpdateText(s);
    }
}
