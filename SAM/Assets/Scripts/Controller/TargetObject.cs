using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TargetObject : MonoBehaviour {

    public bool zooming;
    public float zoomSpeed;
    public Camera camera;


    // Update is called once per frame
    private void Update ()
    {
        RaycastHit hit;
        camera = Camera.main;

        Debug.DrawRay(transform.position, transform.forward * 3000, Color.red);

        if (Physics.Raycast(transform.position, transform.forward, out hit, 3000))
        {
            Debug.Log("Le raycast touche un objet !");
            Debug.Log(hit.transform.name);
            if(hit.transform.name == "Earth")
            {
                camera.fieldOfView = Mathf.Lerp(camera.fieldOfView, 20, Time.deltaTime * 5);
            }
            else
            {
                camera.fieldOfView = Mathf.Lerp(camera.fieldOfView, 60, Time.deltaTime * 5);
            }
        }

	}
}
