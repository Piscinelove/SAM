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
            if (hit.transform.name == "Earth")
            {
                TypeWriting typeWriting = hit.transform.GetComponentInChildren<TypeWriting>();
                Animator interfaceAnimation = hit.transform.GetComponentInChildren<Animator>();
                if (!typeWriting.isStarted)
                {
                    typeWriting.StartCoroutine("TypeIn");
                    interfaceAnimation.SetTrigger("FadeIn");
                }
                camera.fieldOfView = Mathf.Lerp(camera.fieldOfView, 20, Time.deltaTime * 5);
            }
            else if (hit.transform.name == "Flag")
            {
                TypeWriting typeWriting = hit.transform.GetComponentInChildren<TypeWriting>();
                Animator interfaceAnimation = hit.transform.GetComponentInChildren<Animator>();
                if (!typeWriting.isStarted)
                {
                    typeWriting.StartCoroutine("TypeIn");
                    interfaceAnimation.SetTrigger("FadeIn");
                }


            }
            else if (hit.transform.name == "Apollo Lunar Module")
            {
                TypeWriting typeWriting = hit.transform.GetComponentInChildren<TypeWriting>();
                Animator interfaceAnimation = hit.transform.GetComponentInChildren<Animator>();
                if (!typeWriting.isStarted)
                {
                    typeWriting.StartCoroutine("TypeIn");
                    interfaceAnimation.SetTrigger("FadeIn");
                }


            }
            else
            {
                camera.fieldOfView = Mathf.Lerp(camera.fieldOfView, 60, Time.deltaTime * 5);
            }


        }

	}
}
