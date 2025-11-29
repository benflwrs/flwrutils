using UnityEngine;

namespace com.benflwrs.flwrutils.Debug
{
	public class FreeCam : MonoBehaviour
	{
		[Header("Movement Settings")]
		[SerializeField] private float moveSpeed = 10f;
		[SerializeField] private float fastMoveMultiplier = 3f;
		[SerializeField] private float slowMoveMultiplier = 0.25f;

		[Header("Look Settings")]
		[SerializeField] private float mouseSensitivity = 2f;
		[SerializeField] private bool invertY = false;

		[Header("Scroll Speed Adjustment")]
		[SerializeField] private float minSpeed = 1f;
		[SerializeField] private float maxSpeed = 100f;
		[SerializeField] private float scrollSensitivity = 2f;

		private float rotationX = 0f;
		private float rotationY = 0f;

		void Start()
		{
			Cursor.lockState = CursorLockMode.Locked;
			Cursor.visible = false;

			// Initialize rotation to current transform rotation
			Vector3 currentRotation = transform.eulerAngles;
			rotationX = currentRotation.y;
			rotationY = currentRotation.x;
		}

		void Update()
		{
			HandleMouseLook();
			HandleMovement();
			HandleSpeedAdjustment();

			// Toggle cursor lock with Escape
			if (Input.GetKeyDown(KeyCode.Escape))
			{
				Cursor.lockState = Cursor.lockState == CursorLockMode.Locked ? CursorLockMode.None : CursorLockMode.Locked;
				Cursor.visible = !Cursor.visible;
			}
		}

		private void HandleMouseLook()
		{
			if (Cursor.lockState != CursorLockMode.Locked) return;

			float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity;
			float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity;

			rotationX += mouseX;
			rotationY += invertY ? mouseY : -mouseY;
			rotationY = Mathf.Clamp(rotationY, -90f, 90f);

			transform.rotation = Quaternion.Euler(rotationY, rotationX, 0f);
		}

		private void HandleMovement()
		{
			float currentSpeed = moveSpeed;

			// Fast movement with Shift
			if (Input.GetKey(KeyCode.LeftShift))
				currentSpeed *= fastMoveMultiplier;

			// Slow movement with Ctrl
			if (Input.GetKey(KeyCode.LeftControl))
				currentSpeed *= slowMoveMultiplier;

			// Get input
			float horizontal = Input.GetAxis("Horizontal"); // A/D
			float vertical = Input.GetAxis("Vertical");     // W/S
			float upDown = 0f;

			// E/Q for up/down
			if (Input.GetKey(KeyCode.E)) upDown = 1f;
			if (Input.GetKey(KeyCode.Q)) upDown = -1f;

			// Calculate movement
			Vector3 movement = transform.right * horizontal +
							  transform.forward * vertical +
							  Vector3.up * upDown;

			transform.position += movement * currentSpeed * Time.deltaTime;
		}

		private void HandleSpeedAdjustment()
		{
			float scroll = Input.GetAxis("Mouse ScrollWheel");
			if (scroll != 0f)
			{
				moveSpeed += scroll * scrollSensitivity;
				moveSpeed = Mathf.Clamp(moveSpeed, minSpeed, maxSpeed);
			}
		}
	}
}
