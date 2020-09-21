using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CollapseUIElement : MonoBehaviour
{
    public Rigidbody ElementRB;
    private Vector3 startPosition;
    private Vector3 endPosition;
    public Transform endTransform;
    public bool collapse = false;

    private void Start()
    {
        startPosition = ElementRB.position;
        endPosition = endTransform.position;
    }

    void Update()
    {
        Vector3 target;
        if (collapse)
            target = endPosition;
        else
            target = startPosition;
        if(Vector3.Distance(ElementRB.position, target) > .2f)
            ElementRB.position = Vector3.Slerp(ElementRB.position, target, .5f);
    }

    public void ToggleCollapse()
    {
        collapse = !collapse;
    }
}
