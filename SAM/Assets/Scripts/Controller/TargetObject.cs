using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TargetObject : MonoBehaviour {

    private Camera cameraView;
    public string target;

    void Start()
    {
        cameraView = GetComponent<Camera>();

    }


    // Update is called once per frame
    private void Update ()
    {
        RaycastHit hit;


        Debug.DrawRay(transform.position, transform.forward * 3000, Color.red);

        if (Physics.Raycast(transform.position, transform.forward, out hit, 3000))
        {
            target = hit.transform.name;
            //camera.fieldOfView = Mathf.Lerp(camera.fieldOfView, 20, Time.deltaTime * 5);
            //camera.fieldOfView = Mathf.Lerp(camera.fieldOfView, 60, Time.deltaTime * 5);
        }

	}
}
