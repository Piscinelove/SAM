/**
* Rafael Peixoto 2018 - All Rights Reserved
* Virtual Reality with AI chatbot - VRAI Project
* 
* This is the controller that changes the current render scale
* Useful with Oculus GO support to be less blurry
*/

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR;

public class RenderScalerModification : MonoBehaviour {

    // Use this for initialization
    // The render scale. Higher numbers = better quality, but trades performance
    [SerializeField] private float m_RenderScale = 1f;              

    void Start()
    {
        // Change the current render scale
        XRSettings.eyeTextureResolutionScale = m_RenderScale;
    }

    // Update is called once per frame
    void Update () {
		
	}
}
