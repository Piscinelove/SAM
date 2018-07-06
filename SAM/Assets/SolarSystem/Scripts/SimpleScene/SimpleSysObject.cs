using UnityEngine;
using System.Collections;

[AddComponentMenu("Scripts/Solar System/Simple Scene/SimpleSysObject")]
public class SimpleSysObject : MonoBehaviour {

	public const int orbitSample = 100;
	public static bool drawOrbit = false;
	public const float scaleFactor = 0.0001f;

	public float equatorialRadius;
	public float polarRadius;
	public Transform moveAround;
	public float period;
	public float axialTilt;
	public float rotationPeriod;

	private Transform myTransform;
	private Vector3 initialVector;
	private Quaternion moveAroundQuaternion;
	private static Material _lineMaterial;
	private static Color _deltaColor;

	void Awake () {
		myTransform = transform;
		initialVector = myTransform.position - moveAround.position;
		moveAroundQuaternion = Quaternion.Euler (0, 0, 0);
		myTransform.rotation *= Quaternion.Euler (0, 0, axialTilt);
		_deltaColor = Color.green * .5f / orbitSample;
		CreateLineMaterial ();
	}

	void Update () {
		moveAroundQuaternion *= Quaternion.Euler (0, -Time.deltaTime * SimpleViewer.timeScale / period, 0);
		myTransform.position = moveAroundQuaternion * initialVector + moveAround.position;
		myTransform.rotation *= Quaternion.Euler (0, -Time.deltaTime * SimpleViewer.timeScale / rotationPeriod, 0);
	}

	void OnRenderObject ()
	{
		if (!drawOrbit)
			return;

		_lineMaterial.SetPass(0);

		Quaternion rotate = moveAroundQuaternion;
		Color lerpColor = Color.green;

		GL.Begin(GL.LINES);

		GL.Color(lerpColor);
		GL.Vertex (myTransform.position);
		rotate *= Quaternion.Euler (0, 360.0f / orbitSample, 0);
		lerpColor -= _deltaColor;
		GL.Color(lerpColor);
		GL.Vertex (rotate * initialVector + moveAround.position);
		for (int i = 0; i < orbitSample - 1; ++i)
		{
			GL.Vertex (rotate * initialVector + moveAround.position);
			rotate *= Quaternion.Euler (0, 360.0f / orbitSample, 0);
			lerpColor -= _deltaColor;
			GL.Color(lerpColor);
			GL.Vertex (rotate * initialVector + moveAround.position);
		}
		GL.Vertex (rotate * initialVector + moveAround.position);
		lerpColor -= _deltaColor;
		GL.Color(lerpColor);
		GL.Vertex (myTransform.position);

		GL.End();
	}

	static void CreateLineMaterial ()
	{
		if (!_lineMaterial) {
			var shader = Shader.Find ("Hidden/Internal-Colored");
			_lineMaterial = new Material (shader);
			_lineMaterial.hideFlags = HideFlags.HideAndDontSave;
			_lineMaterial.SetInt ("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcColor);
			_lineMaterial.SetInt ("_DstBlend", (int)UnityEngine.Rendering.BlendMode.One);
			_lineMaterial.SetInt ("_Cull", (int)UnityEngine.Rendering.CullMode.Off);
			_lineMaterial.SetInt ("_ZWrite", 0);
		}
	}
}
