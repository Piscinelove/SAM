using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

[ExecuteInEditMode, AddComponentMenu("Scripts/Solar System/Tool/AddNeededInputAxis")]
public class AddNeededInputAxis : MonoBehaviour {

	public const string CamUpDownAxisName = "Camera UpDown";
	public const string CamRotateAxisName = "Camera Rotate";

#if UNITY_EDITOR

    void Start ()
	{
		if (AddNeededAxis (CamUpDownAxisName, "f", "r", 3) && AddNeededAxis (CamRotateAxisName, "q", "e", 4))
		{
			DestroyImmediate (this);
		}
	}

	bool AddNeededAxis (string axisName, string negButton, string posButton, int axisNum)
	{
		bool hasAxis = false;
		SerializedObject serializedObject = new SerializedObject (AssetDatabase.LoadAllAssetsAtPath ("ProjectSettings/InputManager.asset") [0]);
		SerializedProperty axesProperty = serializedObject.FindProperty ("m_Axes");

		axesProperty.Next (true);
		axesProperty.Next (true);
		while (axesProperty.Next (false))
		{
			SerializedProperty axis = axesProperty.Copy ();
			axis.Next (true);
			if (axis.stringValue == axisName)
			{
				hasAxis = true;
				break;
			}
		}

		if (!hasAxis)
		{
			axesProperty = serializedObject.FindProperty ("m_Axes");
			axesProperty.arraySize++;
			SerializedProperty axisProperty = axesProperty.GetArrayElementAtIndex (axesProperty.arraySize - 1);
			axisProperty.Next (true);
			axisProperty.stringValue = axisName;
			axisProperty.Next (false);
			axisProperty.stringValue = "";
			axisProperty.Next (false);
			axisProperty.stringValue = "";
			axisProperty.Next (false);
			axisProperty.stringValue = negButton;
			axisProperty.Next (false);
			axisProperty.stringValue = posButton;
			axisProperty.Next (false);
			axisProperty.stringValue = "";
			axisProperty.Next (false);
			axisProperty.stringValue = "";
			axisProperty.Next (false);
			axisProperty.floatValue = 3;
			axisProperty.Next (false);
			axisProperty.floatValue = 0.001f;
			axisProperty.Next (false);
			axisProperty.floatValue = 3;
			axisProperty.Next (false);
			axisProperty.boolValue = true;
			axisProperty.Next (false);
			axisProperty.boolValue = false;
			axisProperty.Next (false);
			axisProperty.intValue = 0;
			axisProperty.Next (false);
			axisProperty.intValue = axisNum;
			axisProperty.Next (false);
			axisProperty.intValue = 0;
			serializedObject.ApplyModifiedProperties ();
			Debug.Log (axisName + " Input Axis Added!");
			return true;
		}
		return false;
	}

#endif
}
