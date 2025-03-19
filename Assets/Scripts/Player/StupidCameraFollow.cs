using UnityEngine;

#if UNITY_EDITOR
public class StupidCameraFollow : MonoBehaviour {
	public Transform target;

	public float smoothing = 0.5f;

	const float cameraZ = -10.0f;

	Camera playerCamera;

	void LateUpdate() {
		if (playerCamera == null) {
			playerCamera = Camera.main;
			if (playerCamera == null) {
				return;
			}
		}

		var camPosition = playerCamera.transform.position;

		var delta = target.position - camPosition;
		delta *= smoothing;

		camPosition += delta * Time.deltaTime;
		camPosition.z = cameraZ;

		playerCamera.transform.position = camPosition;
	}
}
#endif
