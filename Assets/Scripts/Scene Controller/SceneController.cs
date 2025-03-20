//#define USE_MAIN_MENU_SCENE_LOADING

using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.SceneManagement;

[Serializable]
public struct Level {
	public string name;

	[Tooltip("The scene that becomes the active scene when this level is loaded.")]
	[HideInInspector]
	public string setAsActiveScene;

	[FormerlySerializedAs("additionalScenes")]
	public string[] scenes;
}

public class SceneController : MonoBehaviour {
	[HideInInspector]
	public SceneController instance;

	[Tooltip("These scenes will get loaded additively.")]
	public string[] loadOnStartup;

#if USE_MAIN_MENU_SCENE_LOADING
	[Space(3)]
	public string[] mainMenuScenes;
#endif

	[Header("Levels")]
	public int currentLevel;
	public Level[] levels;
	void Awake() {
		if (instance != null) {
			Destroy(this);
			return;
		}

		instance = this;

#if !UNITY_EDITOR
		currentLevel = 0;
#endif

		if (loadOnStartup != null) {
			for (int i = 0; i < loadOnStartup.Length; ++i) {
				LoadScene(loadOnStartup[i]);
			}
		}

		SceneManager.sceneLoaded += OnSceneLoaded;
	}

	void Start() {
		LoadLevelScenes(currentLevel);
		previousCurrentLevel = currentLevel;
	}

#if USE_MAIN_MENU_SCENE_LOADING // From Jokern VR, may need so keeping it here.
	public void CloseMainMenu() {
		if (!mainMenuIsLoaded) return;

		if (mainMenuScenes != null) {
			for (int i = 0; i < mainMenuScenes.Length; ++i) {
				if (mainMenuScenes[i] != null) {
					UnloadScene(mainMenuScenes[i]);
				}
			}
		}

		LoadLevelScenes(currentLevel);
		previousCurrentLevel = currentLevel;

		mainMenuIsLoaded = false;
	}

	[HideInInspector] public bool mainMenuIsLoaded;
	public void OpenMainMenu() {
		if (mainMenuIsLoaded) return;

		UnloadLevelScenes(currentLevel);
		previousCurrentLevel = currentLevel;

		if (mainMenuScenes != null) {
			for (int i = 0; i < mainMenuScenes.Length; ++i) {
				if (mainMenuScenes[i] != null) {
					LoadScene(mainMenuScenes[i]);
				}
			}
		}

		mainMenuIsLoaded = true;
	}
#endif

	int previousCurrentLevel;
	void Update() {
		if (previousCurrentLevel != currentLevel) {
			if (currentLevel < 0 || levels.Length <= currentLevel) {
				LogNoSuchLevelExists(currentLevel);
			} else {
				UnloadLevelScenes(previousCurrentLevel);
				LoadLevelScenes(currentLevel);

				previousCurrentLevel = currentLevel;
			}
		}
	}

	void LogNoSuchLevelExists(int level) {
		Debug.LogError($"Try to load level {level}, but no such level exists. Please assign levels to the Levels field on the Scene Controller.");
	}

	public void LoadScene(int buildIndex) => SceneManager.LoadSceneAsync(buildIndex, LoadSceneMode.Additive);
	public void LoadScene(string name)    => SceneManager.LoadSceneAsync(name,       LoadSceneMode.Additive);

	public void SetActiveScene(int buildIndex) {
		var scene = SceneManager.GetSceneByBuildIndex(buildIndex);
		SceneManager.SetActiveScene(scene);
	}

	public void SetActiveScene(string name) {
		var scene = SceneManager.GetSceneByName(name);
		SceneManager.SetActiveScene(scene);
	}

	void OnSceneLoaded(Scene scene, LoadSceneMode mode) {
		if (!scene.isLoaded) {
			// Sanity check.
			Debug.LogError("OnSceneLoaded got a scene that isn't loaded!?");
		}

		if (scene.name == levels[currentLevel].setAsActiveScene) {
			SceneManager.SetActiveScene(scene);
		}
	}

	void UnloadLevelScenes(int level) {
		if (level < 0 || levels.Length <= level) {
			LogNoSuchLevelExists(level);
			return;
		}

		if (levels[level].scenes == null) return;

		for (int i = 0; i < levels[level].scenes.Length; ++i) {
			if (levels[level].scenes[i] != null) {
				UnloadScene(levels[level].scenes[i]);
			}
		}
	}

	void LoadLevelScenes(int level) {
		if (level < 0 || levels.Length <= level) {
			LogNoSuchLevelExists(level);
			return;
		}

		if (levels[level].scenes == null) return;

		for (int i = 0; i < levels[level].scenes.Length; ++i) {
			if (levels[level].scenes[i] != null) {
				LoadSceneIfNotLoaded(levels[level].scenes[i]);
			}
		}
	}

	public void LoadLevel(int level) {
		if (level < levels.Length) {
#if USE_MAIN_MENU_SCENE_LOADING
			if (mainMenuIsLoaded) CloseMainMenu();
#endif

			UnloadLevelScenes(currentLevel);
			currentLevel = level;
			LoadLevelScenes(currentLevel);
		} else {
			LogNoSuchLevelExists(level);
		}
	}

	public void LoadNextLevel()     => currentLevel += 1;
	public void LoadPreviousLevel() => currentLevel -= 1;

	public bool SceneIsLoaded(int buildIndex) {
		for (int i = 0; i < SceneManager.sceneCount; ++i) {
			var scene = SceneManager.GetSceneAt(i);

			if (scene.buildIndex < 0) {
				Debug.LogError($"Scene '{scene.name}' is not in the build!");
			} else if (scene.buildIndex == buildIndex) {
				return true;
			}
		}

		return false;
	}

	public bool SceneIsLoaded(string name) {
		return SceneIsLoaded(SceneNameToBuildIndex(name));
	}

	public void LoadSceneIfNotLoaded(int buildIndex) {
		if (!SceneIsLoaded(buildIndex)) {
			LoadScene(buildIndex);
		}
	}

	public void LoadSceneIfNotLoaded(string name) {
		if (!SceneIsLoaded(name)) {
			LoadScene(name);
		}
	}

	public void UnloadScene(int buildIndex) {
		if (SceneIsLoaded(buildIndex)) {
			SceneManager.UnloadSceneAsync(buildIndex);
		}
	}

	public void UnloadScene(string name) {
		if (SceneIsLoaded(name)) {
			SceneManager.UnloadSceneAsync(name);
		}
	}

	public int SceneNameToBuildIndex(string name) {
		return SceneManager.GetSceneByName(name).buildIndex;
	}

	public void QuitEverything() {
#if UNITY_EDITOR
		UnityEditor.EditorApplication.isPlaying = false;
#else
		Application.Quit();
#endif
	}
}
