using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR;

public class RenderScalerModification : MonoBehaviour {

    // Use this for initialization
    [SerializeField] private float m_RenderScale = 1f;              //The render scale. Higher numbers = better quality, but trades performance

    void Start()
    {
        XRSettings.eyeTextureResolutionScale = m_RenderScale;
    }

    // Update is called once per frame
    void Update () {
		
	}
}
