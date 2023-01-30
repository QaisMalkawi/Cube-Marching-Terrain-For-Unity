using UnityEngine;

[RequireComponent(typeof(Camera))]
public class FlyCamera : MonoBehaviour
{
	public float accSprintMultiplier = 4; // how much faster you go when "sprinting"
	public float lookSensitivity = 1; // mouse look sensitivity
	public float dampingCoefficient = 5; // how quickly you break to a halt after you stop your input

	Vector3 velocity; // current velocity

	Vector3 moveInput, Look;
	bool speedUp;

	static bool Focused
	{
		get => Cursor.lockState == CursorLockMode.Locked;
		set
		{
			Cursor.lockState = value ? CursorLockMode.Locked : CursorLockMode.None;
			Cursor.visible = value == false;
		}
	}

	void OnDisable() => Focused = false;

	Camera cam;

	private void Start()
	{
		cam = GetComponent<Camera>();

	}
	void Update()
	{
		Look.x = Input.GetAxis("Mouse X");
		Look.y = -Input.GetAxis("Mouse Y");

		moveInput.x = Input.GetAxis("Horizontal");
		moveInput.z = Input.GetAxis("Vertical");

		speedUp = Input.GetKey(KeyCode.LeftShift);
		Focused = true;

		// Input
		if (Focused)
			UpdateInput();

		// Physics
		velocity = Vector3.Lerp(velocity, Vector3.zero, dampingCoefficient * Time.deltaTime);
		transform.position += velocity * Time.deltaTime * (speedUp ? accSprintMultiplier : 1);

		Ray ray = cam.ViewportPointToRay(new Vector3(0.5f, 0.5f, 1f));

		if (Input.GetMouseButtonUp(0))
		{
			if (Physics.Raycast(ray, out RaycastHit hit))
			{
				if (hit.collider.TryGetComponent(out Chunk chunk))
				{
					chunk.PlaceTerrain(hit.point);
				}
			}
		}
		if (Input.GetMouseButtonUp(1))
		{
			if (Physics.Raycast(ray, out RaycastHit hit))
			{
				if (hit.collider.TryGetComponent(out Chunk chunk))
				{
					chunk.RemoveTerrain(hit.point);
				}
			}
		}
	}

	void UpdateInput()
	{
		velocity += transform.forward * moveInput.z;
		velocity += transform.right * moveInput.x;
		velocity += Vector3.up * moveInput.y;

		// Rotation
		Vector2 mouseDelta = lookSensitivity * Look;
		Quaternion rotation = transform.rotation;
		Quaternion horiz = Quaternion.AngleAxis(mouseDelta.x, Vector3.up);
		Quaternion vert = Quaternion.AngleAxis(mouseDelta.y, Vector3.right);
		transform.rotation = horiz * rotation * vert;
	}
}