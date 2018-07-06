using UnityEngine;
using System.Collections;

[AddComponentMenu("Scripts/Solar System/RingManager")]
public class RingManager : MonoBehaviour {

	private Material _ringMaterial;
    private Transform _ring;

	void Awake () {
        _ring = transform.Find("Ring");
        MeshRenderer mr = _ring.GetComponent<MeshRenderer>() as MeshRenderer;
        _ringMaterial = mr.material;
    }
	
	void LateUpdate () {
        // Suppose the sun is always at (0, 0, 0)
        Vector3 lightDir = transform.position.normalized;
        Vector3 lightLocalDir = transform.InverseTransformDirection(lightDir);
        float shadow = 1.5f / (Mathf.Abs(Vector3.Dot(Vector3.up, -lightLocalDir)) + .5f) - 1.0f;
        lightLocalDir.y = 0;
        float angle = Vector3.Angle(Vector3.forward, lightLocalDir);
        Vector3 cross = Vector3.Cross(Vector3.forward, lightLocalDir);
        float dir = (cross.y > 0) ? 1.0f : -1.0f;
        _ring.localRotation = Quaternion.Euler(0, dir * angle, 0);
        _ringMaterial.SetVector("_Light", new Vector4(shadow, 0.0f, 0.0f, 0.0f));
    }
}