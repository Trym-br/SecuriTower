using UnityEngine;
using UnityEngine.SceneManagement;
using static AugustBase.All;

public class LevelResetController : MonoBehaviour {
	public static LevelResetController instance;

	public IResetable[] FindAllResetables(int levelNumber) {
		var allResetables = new IResetable[0];

		// Level elements
		if (SceneController.instance != null) {
			GameObject level = SceneController.instance.levels[levelNumber];

			// The level may have different sections to it that we don't want to reset.
			if (level.TryGetComponent(out LevelResetSelector selector)) {
				allResetables = selector.GetResetables();
			} else {
				allResetables = level.GetComponentsInChildren<IResetable>(true);
			}
		} else {
			// There is no SceneController!
			var roots = SceneManager.GetActiveScene().GetRootGameObjects();

			// @Dogshit @Performance: This is just for testing in the editor. In the real thing, we'll have the SceneController available.
			for (int i = 0; i < roots.Length; ++i) {
				var resetables = roots[i].GetComponentsInChildren<IResetable>(true);
				for (int j = 0; j < resetables.Length; ++j) {
					Append(ref allResetables, resetables[j]);
				}
			}

			Debug.LogWarning("The SceneController is not loaded, which means we don't know what game elements are part of which level. We therefore reset everything and not just the elements part of the active level.");
		}

		// The player
		if (PlayerController.instance != null) {
			Append(ref allResetables, PlayerController.instance.GetComponent<IResetable>());
		}

		return allResetables;
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
		var resetables = FindAllResetables(SceneController.instance.currentLevel);

		if (resetables != null) {
			for (int i = 0; i < resetables.Length; ++i) {
				// Should never be null. This is here in case Unity does dogshit.
				if (resetables[i] != null) {
					resetables[i].Reset();
				}
			}
		}
	}

	public bool dothing;
	void Update() {
		if (dothing) {
			dothing = false;

			print("Reseting all levels");
			ResetAllLevels();
			print("DONE");
		}
	}

	// TODO: This really should use an interface, but we're pressed for time.
	public void ResetAllLevels() {
		if (SceneController.instance == null) return;

		for (int i = 0; i < SceneController.instance.levels.Length; ++i) {
			var resetables = FindAllResetables(i);

			for (int j = 0; j < resetables.Length; ++j) {
				// Should never be null. This is here in case Unity does dogshit.
				if (resetables[j] != null) {
					resetables[j].Reset();
				}
			}
		}

		SceneController.instance.currentLevel = 0;
		PlayerController.instance.GameReset();
	}
}
