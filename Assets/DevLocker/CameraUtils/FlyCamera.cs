using UnityEngine;

namespace DevLocker.CameraUtils
{
	/// <summary>
	/// Allows the camera to be controlled like the Scene View camera in the Unity Editor.
	/// This is an improved version of Christer Kaitila's FlyCamera script:
	/// https://gist.github.com/McFunkypants/5a9dad582461cb8d9de3
	/// </summary>
	public class FlyCamera : MonoBehaviour
	{

		/*
	    Writen by Windexglow 11-13-10.  Use it, edit it, steal it I don't care.  
	    Converted to C# 27-02-13 - no credit wanted.
	    Simple flycam I made, since I couldn't find any others made public.  
	    Made simple to use (drag and drop, done) for regular keyboard layout  
	    wasd : basic movement
		qe : up / down
		right-click-drag: rotate
	    shift : Makes camera accelerate
	    space : Moves camera on X and Z axis only.  So camera doesn't gain any height
		scroll : pan / zoom.
		*/

		public float mainSpeed = 10.0f;		// Regular speed
		public float shiftAdd = 50.0f;		// Multiplied by how long shift is held.  Basically running
		public float maxShift = 100.0f;		// Maximum speed when holdin gshift
		public float rotateSpeed = 0.25f;	// How sensitive it with mouse
		public float dragSpeed = 2.0f;		// Drag with mouse scroll wheel.
		public float scrollWheelSensitivity = 2.0f;
		public bool rotateOnlyIfMousedown = true;
		public bool movementStaysFlat = true;

		private Vector3 lastMouse = new Vector3(255, 255, 255); //kind of in the middle of the screen, rather than at the top (play)

		private float totalRun = 1.0f;

		void Update()
		{
			var mousePos = Input.mousePosition;

			if (Input.GetMouseButtonDown(1) || Input.GetMouseButtonDown(2)) {
				lastMouse = mousePos; // $CTK reset when we begin
			}

			Vector3 mouseDiff = mousePos - lastMouse;

			if (!rotateOnlyIfMousedown ||
			    (rotateOnlyIfMousedown && Input.GetMouseButton(1))) {
				Vector3 rotate = mouseDiff;
				rotate = new Vector3(-rotate.y * rotateSpeed, rotate.x * rotateSpeed, 0);
				rotate = new Vector3(transform.eulerAngles.x + rotate.x, transform.eulerAngles.y + rotate.y, 0);
				transform.eulerAngles = rotate;
				// Mouse camera angle done.  
			}

			//Keyboard commands
			//float f = 0.0f;
			Vector3 p = GetBaseInput();
			if (Input.GetKey(KeyCode.LeftShift)) {
				totalRun += Time.deltaTime;
				p = p * totalRun * shiftAdd;
				p.x = Mathf.Clamp(p.x, -maxShift, maxShift);
				p.y = Mathf.Clamp(p.y, -maxShift, maxShift);
				p.z = Mathf.Clamp(p.z, -maxShift, maxShift);
			} else {
				totalRun = Mathf.Clamp(totalRun * 0.5f, 1f, 1000f);
				p = p * mainSpeed;
			}

			if (Input.GetMouseButton(2)) {
				Vector3 mouseDrag = mouseDiff;
				p -= mouseDrag * dragSpeed;
			}

			bool isMouseInsideScreen = 
				0 < mousePos.x && mousePos.x < Screen.width &&
				0 < mousePos.y && mousePos.y < Screen.height;
			if (!Input.GetMouseButton(0) && isMouseInsideScreen) {
				p += Vector3.forward * Input.GetAxis("Mouse ScrollWheel") * scrollWheelSensitivity;
			}

			p = p * Time.deltaTime;
			transform.Translate(p);

			lastMouse = mousePos;
		}

		private Vector3 GetBaseInput()
		{
			//returns the basic values, if it's 0 than it's not active.
			Vector3 p_Velocity = new Vector3();
			if (Input.GetKey(KeyCode.W)) {
				p_Velocity += new Vector3(0, 0, 1);
			}
			if (Input.GetKey(KeyCode.S)) {
				p_Velocity += new Vector3(0, 0, -1);
			}
			if (Input.GetKey(KeyCode.A)) {
				p_Velocity += new Vector3(-1, 0, 0);
			}
			if (Input.GetKey(KeyCode.D)) {
				p_Velocity += new Vector3(1, 0, 0);
			}
			if (Input.GetKey(KeyCode.Q)) {
				p_Velocity += new Vector3(0, -1, 0);
			}
			if (Input.GetKey(KeyCode.E)) {
				p_Velocity += new Vector3(0, 1, 0);
			}
			return p_Velocity;
		}
	}
}
