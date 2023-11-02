using UnityEngine;

public class OrbitCameraController : MonoBehaviour
{
	[SerializeField] private Transform CameraOrbit;
	[SerializeField] private Transform CameraAngle;
	[SerializeField] private Transform CameraTransform;
	[SerializeField] private Camera Camera;
	[SerializeField] private Vector3 CameraSpeed;

	Vector3 CameraPosition;
	

	private void Awake()
	{
		CameraPosition = new Vector3(CameraOrbit.localEulerAngles.y, 
			CameraAngle.localEulerAngles.x,
			CameraTransform.localPosition.z);
	}

	public void ResetCameraPosition()
	{
		CameraOrbit.localEulerAngles = new Vector3(0f, CameraPosition.x, 0f);
		CameraAngle.localEulerAngles = new Vector3(CameraPosition.y, 0f, 0f);
		CameraTransform.localPosition = new Vector3(0f, 0f, CameraPosition.z);
	}
	
	
	private void Update()
	{
		bool mousePressed = Input.GetMouseButton(0);
		float xInput = mousePressed ? Input.GetAxis("Mouse X") : -Input.GetAxis("Horizontal");
		float yInput = mousePressed ? -Input.GetAxis("Mouse Y") : Input.GetAxis("Vertical");
		float zInput = Input.GetAxis("Mouse ScrollWheel") + Input.GetAxis("Zoom");
		Vector3 input = new Vector3( xInput * CameraSpeed.x, yInput * CameraSpeed.y, zInput * CameraSpeed.z) * Time.deltaTime;
		CameraOrbit.Rotate(0f, input.x, 0f, Space.Self);
		CameraAngle.Rotate(input.y, 0f, 0f, Space.Self);
		CameraTransform.Translate(0f,0f, input.z, Space.Self);

		if (Input.GetKeyDown(KeyCode.R))
		{
			ResetCameraPosition();
		}
	}
}
