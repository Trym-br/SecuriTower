using UnityEngine;

public class MusicLevelSetter : MonoBehaviour {
	const string playerTag = "Player";

	public FMODController.Level setLevelTo;

	void FixedUpdate() {
		if (FMODController.instance != null) {
			FMODController.instance.currentLevel = setLevelTo;
		}
	}
}
