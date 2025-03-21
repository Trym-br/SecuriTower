#define LEVELS_ARE_OBJECTS

#if !LEVELS_ARE_OBJECTS
//#	define USE_MAIN_MENU_SCENE_LOADING
//#	define USE_LEVEL_STRUCT
#endif

using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.SceneManagement;

#if USE_LEVEL_STRUCT
[Serializable]
public struct Level {
	public string name;

	[Tooltip("The scene that becomes the active scene when this level is loaded.")]
	[HideInInspector]
	public string setAsActiveScene;

	[FormerlySerializedAs("additionalScenes")]
	public string[] scenes;
}
#endif

public class SceneController : MonoBehaviour {
	[HideInInspector]
	public static SceneController instance;

	[Tooltip("These scenes will get loaded additively.")]
	public string[] loadOnStartup;

#if USE_MAIN_MENU_SCENE_LOADING
	[Space(3)]
	public string[] mainMenuScenes;
#endif

	[Header("Levels")]
	public int currentLevel;

#if LEVELS_ARE_OBJECTS
	public GameObject[] levels;
#else
#	if USE_LEVEL_STRUCT
	public Level[] levels;
#	else
	public string[] levels;
#	endif
#endif

	void Awake() {
		if (instance != null) {
			Destroy(this);
			return;
		}

		instance = this;

#if !UNITY_EDITOR
		currentLevel = 0;
#endif

#if LEVELS_ARE_OBJECTS
		if (levels != null) {
			for (int i = 0; i < levels.Length; ++i) {
				levels[i].SetActive(false);
			}
		}
#endif

		if (loadOnStartup != null) {
			for (int i = 0; i < loadOnStartup.Length; ++i) {
				LoadScene(loadOnStartup[i]);
			}
		}

		SceneManager.sceneLoaded += OnSceneLoaded;
	}

	void Start() {
		previousCurrentLevel = currentLevel;
		LoadLevelScenes(currentLevel);
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
#if UNITY_EDITOR
		if (!scene.isLoaded) {
			// Sanity check.
			Debug.LogError("OnSceneLoaded got a scene that isn't loaded!?");
		}
#endif

#if USE_LEVEL_STRUCT
		if (scene.name == levels[currentLevel].setAsActiveScene) {
			SceneManager.SetActiveScene(scene);
		}
#endif
	}

	void UnloadLevelScenes(int level) {
		if (level < 0 || levels.Length <= level) {
			LogNoSuchLevelExists(level);
			return;
		}

#if LEVELS_ARE_OBJECTS
		if (levels[level] == null) return;
		levels[level].SetActive(false);
#else
#	if USE_LEVEL_STRUCT
		if (levels[level].scenes == null) return;

		for (int i = 0; i < levels[level].scenes.Length; ++i) {
			if (levels[level].scenes[i] != null) {
				UnloadScene(levels[level].scenes[i]);
			}
		}
#	else
		if (levels[level] == null) return;
		UnloadScene(levels[level]);
#	endif
#endif
	}

	void LoadLevelScenes(int level) {
		if (level < 0 || levels.Length <= level) {
			LogNoSuchLevelExists(level);
			return;
		}

#if LEVELS_ARE_OBJECTS
		if (levels[level] == null) return;
		levels[level].SetActive(true);
#else
#	if USE_LEVEL_STRUCT
		if (levels[level].scenes == null) return;

		for (int i = 0; i < levels[level].scenes.Length; ++i) {
			if (levels[level].scenes[i] != null) {
				LoadSceneIfNotLoaded(levels[level].scenes[i]);
			}
		}
#	else
		if (levels[level] == null) return;
		//LoadSceneIfNotLoaded(levels[level]);
		LoadScene(levels[level]);
#	endif
#endif
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
