using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(SimpleSysObject))]
public class SimpleSysObjectEditor : Editor {

	private SimpleSysObject sysObject;

	public override void OnInspectorGUI ()
	{
		sysObject = target as SimpleSysObject;

		DrawDefaultInspector ();

		if (GUILayout.Button ("Modify Radius"))
		{
			sysObject.transform.localScale = new Vector3 (sysObject.equatorialRadius, sysObject.polarRadius, sysObject.equatorialRadius)
				* SimpleSysObject.scaleFactor * 2;
		}
	}
}
