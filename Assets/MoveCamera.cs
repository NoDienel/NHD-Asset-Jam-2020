using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class MoveCamera : MonoBehaviour
{
    public Vector2 PanSpeed = new Vector2(10f,10f);
    public Vector2 RotateSpeed = new Vector2(10f, 10f);
    public float ScrollSpeed = 10f;

    public Rigidbody pivot;
    new public Rigidbody camera;
    // Start is called before the first frame update
    void Start()
    {
        camera = this.GetComponent<Rigidbody>();
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        Vector3 dirToPivot = pivot.position - camera.position;
        //Look at focus point
        camera.rotation = Quaternion.LookRotation(dirToPivot, Vector3.up);


        float arbitraryLimit = 1f;
        //Scroll wheel zoom
        //Debug.Log("Distance from camera to pivotpoint " + Vector3.Distance(pivot.position, camera.position));
        if (Vector3.Distance(pivot.position, camera.position) > arbitraryLimit || Input.mouseScrollDelta.y <= 0)
        {
            this.transform.position += camera.transform.forward * Input.mouseScrollDelta.y * Time.fixedDeltaTime * ScrollSpeed;
        }

        float hz = Input.GetAxis("Horizontal");
        float vt = Input.GetAxis("Vertical");
        // WASD movement
        if (!Input.GetKey(KeyCode.LeftShift))
        {
            Vector3 vtVector = Vector3.Cross(camera.transform.right, Vector3.up);
            Vector3 hzVector = Vector3.Cross(vtVector, Vector3.up);
            vtVector *= vt * Time.fixedDeltaTime * PanSpeed.y; hzVector *= -hz * Time.fixedDeltaTime * PanSpeed.x;
            pivot.position += vtVector + hzVector;
            camera.position += vtVector + hzVector;
        }
        else
        {
            pivot.rotation = Quaternion.AngleAxis(RotateSpeed.x * Time.deltaTime * -hz, Vector3.up) * pivot.rotation;
            pivot.rotation = Quaternion.AngleAxis(RotateSpeed.y * Time.deltaTime * vt, -camera.transform.right)* pivot.rotation;
        }
    }
}
