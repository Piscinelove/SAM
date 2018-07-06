using UnityEngine;
using UnityEngine.UI;

[AddComponentMenu("Scripts/Solar System/Simple Scene/SimpleViewer")]
public class SimpleViewer : MonoBehaviour {

	public static float timeScale = 1.0f;
    public static float rotateSpeed = 4.0f;
	private static float moveSpeed = 10.0f;
	private static Transform me;
	private static Transform selectedObject = null;
	private static Transform followObject = null;
	private static SimpleSysObject sysObj;
	private static Vector3 followVector;
	private static Text btnLText;
	private float _mouseHeldTime = 0.0f;

    void Awake ()
	{
		me = transform;
		btnLText = GameObject.Find ("BtnLText").GetComponent<Text> ();
    }

	public void LateUpdate ()
	{
		if (Input.GetMouseButtonUp (0) && _mouseHeldTime < .2f)
		{
			Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
			RaycastHit hit;
			if (Physics.Raycast(ray, out hit))
			{
				selectedObject = hit.transform;
			}
			else
			{
				selectedObject = null;
			}
		}
		if (Input.GetMouseButton(0))
		{
			me.Rotate (Input.GetAxis ("Mouse Y") * rotateSpeed, -Input.GetAxis ("Mouse X") * rotateSpeed, 0);
			_mouseHeldTime += Time.deltaTime;
		}
		else
			_mouseHeldTime = 0;

		me.Rotate (0, 0, Input.GetAxis(AddNeededInputAxis.CamRotateAxisName));
        moveSpeed *= Input.GetAxis("Mouse ScrollWheel") + 1;

		if (followObject)
		{
			followVector += (Input.GetAxis ("Horizontal") * me.right + Input.GetAxis ("Vertical") * me.forward +
			Input.GetAxis (AddNeededInputAxis.CamUpDownAxisName) * me.up).normalized * moveSpeed * Time.deltaTime;
			me.position = followObject.position + followVector;
		}
		else
		{
			me.position += (Input.GetAxis ("Horizontal") * me.right + Input.GetAxis ("Vertical") * me.forward +
				Input.GetAxis (AddNeededInputAxis.CamUpDownAxisName) * me.up).normalized * moveSpeed * Time.deltaTime;
		}
		
		bool followed = false;
		if (Input.GetKeyDown (KeyCode.G))
		{
			followObject = selectedObject;
			followed = true;
		}
		if (followed && followObject)
		{
			sysObj = followObject.GetComponent<SimpleSysObject> () as SimpleSysObject;
			Vector3 dir = -followObject.position.normalized;
			followVector = dir * sysObj.equatorialRadius * SimpleSysObject.scaleFactor * 3;
			me.position = followObject.position + followVector;
			me.LookAt (followObject.position);
		}

		if (Input.GetKeyDown(KeyCode.O))
			SimpleSysObject.drawOrbit = !SimpleSysObject.drawOrbit;

		if (Input.GetKeyDown(KeyCode.Equals))
		{
			timeScale *= 10;
		}
		else if (Input.GetKeyDown(KeyCode.Minus))
		{
			timeScale /= 10;
		}

		btnLText.text = "";
		btnLText.text = "Select : " + (selectedObject ? selectedObject.name : null);
		btnLText.text += "\nFollow : " + (followObject ? followObject.name : null);
		btnLText.text += "\nMove Speed : " + moveSpeed;
		btnLText.text += "\nTime Scale : x" + timeScale;
    }

}
