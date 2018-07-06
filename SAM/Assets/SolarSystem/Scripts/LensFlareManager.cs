using UnityEngine;
using System.Collections;

[AddComponentMenu("Scripts/Solar System/LensFlareManager")]
public class LensFlareManager : MonoBehaviour {

	private Transform _cam;
    private Material _mat;

	void Start () {
		_cam = Camera.main.transform;
        MeshRenderer mr = GetComponent<MeshRenderer>() as MeshRenderer;
        _mat = mr.material;
    }
	
	void LateUpdate () {
        // Suppose the sun is always at (0, 0, 0)
        transform.position = _cam.position - _cam.position.normalized;
        transform.LookAt(_cam, _cam.up);
        float factor = Mathf.Pow(500 / _cam.position.magnitude, .7f);
        transform.localScale = new Vector3(factor, factor, 1);
        float colorFactor = Mathf.Clamp(0.001f * (_cam.position.magnitude - 300), 0.0f, 0.5f);
        Color flareColor = new Color(colorFactor, colorFactor, colorFactor);
        _mat.SetColor("_TintColor", flareColor);
        //Debug.Log(_cam.position.magnitude);
    }
}
