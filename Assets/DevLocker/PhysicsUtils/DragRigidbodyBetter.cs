using System.Collections;
using UnityEngine;

/// <summary>
/// This script is a replacement for the now removed DragRigidbody that was packed with the Unity Standard Assets.
/// It adds some neat features and controls.
///
/// Link: https://github.com/NibbleByte/DevLocker
/// </summary>
namespace DevLocker.PhysicsUtils
{
	public class DragRigidbodyBetter : MonoBehaviour {

		[Tooltip("The spring force applied when dragging rigidbody. The dragging is implemented by attaching an invisible spring joint.")]
		public float Spring = 50.0f;
		public float Damper = 5.0f;
		public float Drag = 10.0f;
		public float AngularDrag = 5.0f;
		public float Distance = 0.2f;
		public float ScrollWheelSensitivity = 5.0f;
		public float RotateSpringSpeed = 10.0f;

		[Tooltip("Pin dragged spring to its current location.")]
		public KeyCode KeyToPinSpring = KeyCode.Space;

		[Tooltip("Delete all pinned springs.")]
		public KeyCode KeyToClearPins = KeyCode.Delete;

		[Tooltip("Twist spring.")]
		public KeyCode KeyToRotateLeft = KeyCode.Z;

		[Tooltip("Twist spring.")]
		public KeyCode KeyToRotateRight = KeyCode.C;

		[Tooltip("Set any LineRenderer prefab to render the used springs for the drag.")]
		public LineRenderer SpringRenderer;

		private int m_SpringCount = 1;
		private SpringJoint m_SpringJoint;
		private LineRenderer m_SpringRenderer;


		private void Update() {

			UpdatePinnedSprings();

			// Make sure the user pressed the mouse down
			if (!Input.GetMouseButtonDown(0)) {
				return;
			}

			var mainCamera = FindCamera();

			// We need to actually hit an object
			RaycastHit hit = new RaycastHit();
			if (
				!Physics.Raycast(mainCamera.ScreenPointToRay(Input.mousePosition).origin,
								 mainCamera.ScreenPointToRay(Input.mousePosition).direction, out hit, 100,
								 Physics.DefaultRaycastLayers)) {
				return;
			}
			// We need to hit a rigidbody that is not kinematic
			if (!hit.rigidbody || hit.rigidbody.isKinematic) {
				return;
			}

			if (!m_SpringJoint) {
				var go = new GameObject("Rigidbody dragger-" + m_SpringCount);
				go.transform.parent = transform;
				go.transform.localPosition = Vector3.zero;
				Rigidbody body = go.AddComponent<Rigidbody>();
				m_SpringJoint = go.AddComponent<SpringJoint>();
				body.isKinematic = true;
				m_SpringCount++;

				if (SpringRenderer) {
					m_SpringRenderer = GameObject.Instantiate(SpringRenderer.gameObject, m_SpringJoint.transform, true).GetComponent<LineRenderer>();
				}
			}

			m_SpringJoint.transform.position = hit.point;
			m_SpringJoint.anchor = Vector3.zero;

			m_SpringJoint.spring = Spring;
			m_SpringJoint.damper = Damper;
			m_SpringJoint.maxDistance = Distance;
			m_SpringJoint.connectedBody = hit.rigidbody;

			if (m_SpringRenderer) {
				m_SpringRenderer.enabled = true;
			}
			UpdatePinnedSprings();

			StartCoroutine(DragObject(hit.distance));
		}


		private IEnumerator DragObject(float distance) {
			var oldDrag = m_SpringJoint.connectedBody.drag;
			var oldAngularDrag = m_SpringJoint.connectedBody.angularDrag;
			m_SpringJoint.connectedBody.drag = Drag;
			m_SpringJoint.connectedBody.angularDrag = AngularDrag;
			var mainCamera = FindCamera();
			while (Input.GetMouseButton(0) && !Input.GetKeyDown(KeyToPinSpring)) {
				distance += Input.GetAxis("Mouse ScrollWheel") * ScrollWheelSensitivity;

				var ray = mainCamera.ScreenPointToRay(Input.mousePosition);
				m_SpringJoint.transform.position = ray.GetPoint(distance);

				var connectedPosition = m_SpringJoint.connectedBody.transform.position +
										m_SpringJoint.connectedBody.rotation * m_SpringJoint.connectedAnchor;

				var axis = m_SpringJoint.transform.position - connectedPosition;
				if (Input.GetKey(KeyToRotateLeft)) {
					m_SpringJoint.connectedBody.transform.Rotate(axis, RotateSpringSpeed, Space.World);
				}
				if (Input.GetKey(KeyToRotateRight)) {
					m_SpringJoint.connectedBody.transform.Rotate(axis, -RotateSpringSpeed, Space.World);
				}
				yield return null;
			}


			if (m_SpringJoint.connectedBody) {
				m_SpringJoint.connectedBody.drag = oldDrag;
				m_SpringJoint.connectedBody.angularDrag = oldAngularDrag;

				if (Input.GetKeyDown(KeyToPinSpring)) {
					m_SpringJoint = null;
					m_SpringRenderer = null;
				} else {
					m_SpringJoint.connectedBody = null;
					if (m_SpringRenderer) {
						m_SpringRenderer.enabled = false;
					}
				}
			}
		}


		private void UpdatePinnedSprings()
		{
			foreach (Transform child in transform) {
				var spring = child.GetComponent<SpringJoint>();
				var renderer = child.GetComponentInChildren<LineRenderer>();

				if (!spring.connectedBody)
					continue;

				var connectedPosition = spring.connectedBody.transform.TransformPoint(spring.connectedAnchor);

				if (renderer && renderer.positionCount >= 2) {
					renderer.SetPosition(0, spring.transform.position);
					renderer.SetPosition(1, connectedPosition);
				}
			}

			if (Input.GetKeyDown(KeyToClearPins)) {
				foreach (Transform child in transform) {
					if (m_SpringJoint == null || child.gameObject != m_SpringJoint.gameObject) {
						GameObject.Destroy(child.gameObject);
					}
				}
			}
		}

		private Camera FindCamera() {
			if (GetComponent<Camera>()) {
				return GetComponent<Camera>();
			}

			return Camera.main;
		}
	}
}
