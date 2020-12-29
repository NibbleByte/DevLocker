using UnityEngine;

namespace DevLocker.CameraUtils
{
	/// <summary>
	/// Allows the camera to be controlled like the Scene View camera in the Unity Editor.
	/// Attach to your camera game object.
	/// This is an heavily improved version of Christer Kaitila's FlyCamera script:
	/// https://gist.github.com/McFunkypants/5a9dad582461cb8d9de3
	///
	/// Link: https://github.com/NibbleByte/DevLocker
	/// </summary>
	public class FlyCamera : MonoBehaviour
	{
		[Tooltip("Move speed.")]
		public float MoveSpeed = 10.0f;

		[Tooltip("Multiplied by how long shift is held.")]
		public float RunMultiplier = 5.0f;

		[Tooltip("Rotate sensitivity (with right mouse button)")]
		public float RotateSensitivity = 0.25f;

		[Tooltip("Pan sensitivity (with middle mouse button)")]
		public float PanSensitivity = 2.0f;

		[Tooltip("Scroll wheel movement sensitivity.")]
		public float ScrollWheelSensitivity = 200.0f;

		private Vector3 m_LastMousePos;

		void Update()
		{
			Vector3 mousePos = Input.mousePosition;

			if (Input.GetMouseButtonDown(1) || Input.GetMouseButtonDown(2) || Input.GetMouseButtonDown(3)) {
				m_LastMousePos = mousePos; // Reset when we begin
			}

			Vector3 mouseDelta = mousePos - m_LastMousePos;

			// Right mouse button rotate drag.
			if (Input.GetMouseButton(1) && mouseDelta != Vector3.zero) {
				Vector3 euler = transform.eulerAngles;
				euler.x += -mouseDelta.y * RotateSensitivity;
				euler.y += mouseDelta.x * RotateSensitivity;
				transform.eulerAngles = euler;
			}

			Vector3 velocity = MoveSpeed * GetKeyboardInputVelocity();

			if (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift)) {
				velocity *= RunMultiplier;
			}

			// Middle mouse button pan drag.
			if (Input.GetMouseButton(2)) {
				velocity -= mouseDelta * PanSensitivity;
			}

			// Reject scrolling input over the inspector or some other window.
			bool isMouseInsideScreen =
				0 < mousePos.x && mousePos.x < Screen.width &&
				0 < mousePos.y && mousePos.y < Screen.height;

			if (isMouseInsideScreen && !Input.GetMouseButton(0)) {
				velocity += Vector3.forward * Input.GetAxis("Mouse ScrollWheel") * ScrollWheelSensitivity;
			}

			m_LastMousePos = mousePos;

			// Avoid dirtying transform if not needed to.
			if (velocity == Vector3.zero)
				return;

			transform.Translate(velocity * Time.deltaTime);

		}

		// Returns velocity vector containing sum of all input directions. Zero vector if not moving.
		private Vector3 GetKeyboardInputVelocity()
		{
			Vector3 velocity = new Vector3();
			if (Input.GetKey(KeyCode.W)) {
				velocity += new Vector3(0, 0, 1);
			}
			if (Input.GetKey(KeyCode.S)) {
				velocity += new Vector3(0, 0, -1);
			}
			if (Input.GetKey(KeyCode.A)) {
				velocity += new Vector3(-1, 0, 0);
			}
			if (Input.GetKey(KeyCode.D)) {
				velocity += new Vector3(1, 0, 0);
			}
			if (Input.GetKey(KeyCode.Q)) {
				velocity += new Vector3(0, -1, 0);
			}
			if (Input.GetKey(KeyCode.E)) {
				velocity += new Vector3(0, 1, 0);
			}
			return velocity;
		}
	}
}
