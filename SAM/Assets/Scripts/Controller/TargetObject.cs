/**
* Rafael Peixoto 2018 - All Rights Reserved
* Virtual Reality with AI chatbot - VRAI Project
* 
* This is the controller of the current target object
* When attached to the active main camera, it fires a raycast to
* every object that is seen by the user
*/

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

    /*
     *  Update() method
     *  Called every frame and change the current target
     */
    private void Update ()
    {
        RaycastHit hit;

        Debug.DrawRay(transform.position, transform.forward * 3000, Color.red);

        // Fires a raycast and if the raycast hits something set the object to the current target
        if (Physics.Raycast(transform.position, transform.forward, out hit, 3000))
        {
            target = hit.transform.name;
        }

	}
}
