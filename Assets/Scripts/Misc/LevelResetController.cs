using UnityEngine;
using static AugustBase.All;

public class LevelResetController : MonoBehaviour {
	public static LevelResetController instance;

	IResetable[] allResetables;
	void FindAllResetables() {
		// Level elements
		if (SceneController.instance != null) {
			GameObject level = SceneController.instance.levels[SceneController.instance.currentLevel];
			allResetables = level.GetComponentsInChildren<IResetable>(true);
		} else {
			// There is no SceneController!
			Debug.LogError("The level resetter currently depends on the scene controller being present as I have not yet made an alternative path.");
		}

		// The player
		if (PlayerController.instance != null) {
			Append(ref allResetables, PlayerController.instance.GetComponent<IResetable>());
		}
	}

	void Awake() {
		if (instance != null) {
			Destroy(this);
			return;
		}

		instance = this;
	}

	public void ResetLevel() {
		// @Performance: Avoid doing FindAllResetables() every time we reset the level.
		FindAllResetables();

		if (allResetables != null) {
			for (int i = 0; i < allResetables.Length; ++i) {
				// Should never be null. This is here in case Unity does dogshit.
				if (allResetables[i] != null) {
					allResetables[i].Reset();
				}
			}
		}
	}
}
