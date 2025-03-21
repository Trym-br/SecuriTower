//
// -- USAGE --
// Put this file somewhere in your project such that Unity can find it, then
// just import everything by adding the following line to the top of your
// script:
// > using static AugustBase.All;
//
// (Last edited 09.12.2024)
//

using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Rendering.Universal;
using Random = UnityEngine.Random;

using System;

namespace AugustBase {
	// Yes stupid name, but C# is also stupid. This name should not be required.
	public static class All {
		public static void StopProgram() {
#if UNITY_EDITOR
			UnityEditor.EditorApplication.isPlaying = false;
#else
			Application.Quit();
#endif
		}

		public static T GetComponentOrStop<T>(this GameObject obj) where T : Component, new() {
			var result = obj.GetComponent<T>();
#if UNITY_EDITOR
			if (result == null) {
				Debug.LogError("'" + obj.name + "' does not have a '" + (new T()).GetType().Name + "', but wants one!");
				StopProgram();
			}
#endif

			return result;
		}

		public static T FindExactlyOneObjectOrStop<T>() where T : UnityEngine.Object, new() {
			var foundObjects = UnityEngine.Object.FindObjectsByType<T>(FindObjectsSortMode.None);

			if (foundObjects.Length > 1) {
#if UNITY_EDITOR
				Debug.LogError("Too many '" + foundObjects[0].GetType().Name + "'s! There is only supposed to be one, but found " + foundObjects.Length + "!");
				StopProgram();
#endif
			} else if (foundObjects == null || foundObjects.Length == 0) {
#if UNITY_EDITOR
				Debug.LogError("Found no '" + (new T()).GetType().Name + "'! Please create one!");
				StopProgram();
#endif
				return default;
			}

			return foundObjects[0];
		}

		public static GameObject FindExactlyOneGameObjectWithTagOrStop(string tag) {
			GameObject[] found = GameObject.FindGameObjectsWithTag(tag);
			if (found == null || found.Length == 0) {
#if UNITY_EDITOR
				Debug.LogError("Tried to find a game object with tag '" + tag + "', but couldn't find any.");
				StopProgram();
#endif
				return default;
			} else if (found.Length > 1) {
#if UNITY_EDITOR
				Debug.LogError("Expected to find exactly 1 game object with tag '" + tag + "', but found " + found.Length + ".");
				StopProgram();
#endif
			}

			return found[0];
		}

		// Not recursive! Only gets the first matching child of the transform, not a child of a child.
		public static Transform GetFirstChildByName(this Transform transform, string name) {
			for (int i = 0; i < transform.childCount; ++i) {
				var child = transform.GetChild(i);

				if (child.gameObject.name == name) {
					return child;
				}
			}

			return null;
		}

		// Not recursive!
		public static Transform GetFirstChildByNameOrStop(this Transform transform, string name) {
			var child = transform.GetFirstChildByName(name);

#if UNITY_EDITOR
			if (child == null) {
				Debug.LogError($"No child of '{transform.gameObject.name}' is called '{name}', but we expect there to be one.");
				StopProgram();
			}
#endif

			return child;
		}

		//
		// Math
		//

		public static float Clamp(float n, float min, float max) {
			if (n < min) return min;
			if (n > max) return max;
			return n;
		}

		public static int Clamp(int n, int min, int max) {
			if (n < min) return min;
			if (n > max) return max;
			return n;
		}

		public static uint Clamp(uint n, uint min, uint max) {
			if (n < min) return min;
			if (n > max) return max;
			return n;
		}

		public static long Clamp(long n, long min, long max) {
			if (n < min) return min;
			if (n > max) return max;
			return n;
		}

		public static ulong Clamp(ulong n, ulong min, ulong max) {
			if (n < min) return min;
			if (n > max) return max;
			return n;
		}

		public static short Clamp(short n, short min, short max) {
			if (n < min) return min;
			if (n > max) return max;
			return n;
		}

		public static ushort Clamp(ushort n, ushort min, ushort max) {
			if (n < min) return min;
			if (n > max) return max;
			return n;
		}

		public static char Clamp(char n, char min, char max) {
			if (n < min) return min;
			if (n > max) return max;
			return n;
		}

		public static byte Clamp(byte n, byte min, byte max) {
			if (n < min) return min;
			if (n > max) return max;
			return n;
		}

		public static Vector2 Abs(Vector2 v) {
			if (v.x < 0) v.x *= -1;
			if (v.y < 0) v.y *= -1;
			return v;
		}

		public static Vector3 Abs(Vector3 v) {
			if (v.x < 0) v.x *= -1;
			if (v.y < 0) v.y *= -1;
			if (v.z < 0) v.z *= -1;
			return v;
		}

		public static int Min(int a, int b) {
			if (a < b) return a;
			return b;
		}

		public static float FloorToNearest(float toFloor, float findNearest) {
			if (findNearest == 0.0f) return 0.0f;
			return Mathf.Floor(toFloor / findNearest) * findNearest;
		}

		//
		// Scenes
		//

		// Based on build index!
		public static void LoadNextScene() {
			int currentSceneBuildIndex = SceneManager.GetActiveScene().buildIndex;
			if (currentSceneBuildIndex + 1 < SceneManager.sceneCountInBuildSettings) {
				SceneManager.LoadScene(currentSceneBuildIndex + 1);
			} else {
				Debug.LogError("Could not load next scene: No scene has build index " + (currentSceneBuildIndex + 1) + ".");
				StopProgram();

				// In case stopping doesn't work?
				SceneManager.LoadScene(currentSceneBuildIndex);
			}
		}

		// Based on build index!
		public static void LoadPreviousScene() {
			int currentSceneBuildIndex = SceneManager.GetActiveScene().buildIndex;
			if (currentSceneBuildIndex > 0) {
				SceneManager.LoadScene(currentSceneBuildIndex - 1);
			} else {
				Debug.LogError("Could not load previous scene: No scene has build index -1.");
				StopProgram();

				SceneManager.LoadScene(currentSceneBuildIndex);
			}
		}

		public static void RestartCurrentScene() {
			SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
		}

		//
		// Arrays
		//

		public static void SetLength<T>(ref T[] array, int length) {
			Array.Resize(ref array, length);
		}

		public static void OrderedRemoveAt<T>(ref T[] array, int index) {
			if (array.Length == 0) return;
			if (index < 0 || array.Length <= index) return;

			for (int i = index; i + 1 < array.Length; ++i) {
				array[i] = array[i + 1];
			}

			SetLength(ref array, array.Length - 1);
		}

		// Unordered!
		public static void FastRemoveAt<T>(ref T[] array, int index) {
			if (array.Length == 0) return;
			if (index < 0 || array.Length <= index) return;

			array[index] = array[array.Length - 1];
			SetLength(ref array, array.Length - 1);
		}

		public static void InsertAt<T>(ref T[] array, T item, int index) {
			if (array.Length == 0) return;
			if (index < 0 || array.Length <= index) return;

			SetLength(ref array, array.Length + 1);

			for (int i = array.Length - 1; i > index; --i) {
				array[i] = array[i - 1];
			}

			array[index] = item;
		}

		// Appends an item to the end of an array.
		public static void Append<T>(ref T[] array, T item) {
			SetLength(ref array, array.Length + 1);
			array[array.Length - 1] = item;
		}

		public static T[] Clone<T>(T[] array) {
			return (T[])array.Clone();
		}

		//
		// Transforms
		//

		// Moves Transform 'toMove' towards 'targetPosition' based on smoothing factor 'factor'.
		// 'factor' is expected to be between 0.0f and 1.0f, where 1.0f instantly moves 'toMove' to
		// 'targetPosition', and 0.0f does not move 'toMove' at all. A smoothing factor of 0.5f
		// makes 'toMove' move half the distance to 'targetPosition' every time it is called.
		public static void MoveTowards(Transform toMove, Vector3 targetPosition, float factor = 0.5f) {
			if (factor <= 0.0f) return;

			if (factor >= 1.0f) {
				toMove.position = targetPosition;
				return;
			}

			var delta = targetPosition - toMove.position;
			delta *= factor;

			var newPosition = toMove.position;
			newPosition += delta;
			toMove.position = newPosition;
		}

		public static void MoveTowards(Transform toMove, Vector2 targetPosition, float factor = 0.5f) {
			MoveTowards(toMove, new Vector3(targetPosition.x, targetPosition.y, 0.0f), factor);
		}

		public static void HoverAt(Transform toHover, Vector3 position, float speed, float height) {
			var newPosition = position;
			newPosition.y += Mathf.Sin(Time.time * speed) * (height / 2);
			toHover.position = newPosition;
		}

		public static void HoverAt(Transform toHover, Vector2 position, float speed, float height) {
			var newPosition = position;
			newPosition.y += Mathf.Sin(Time.time * speed) * (height / 2);
			toHover.position = newPosition;
		}

		public static void CameraFollow2D(Vector2 target, float smoothing = 0.5f, Camera camera = null) {
			if (smoothing < 0.0f)      smoothing = 0.0f;
			else if (smoothing > 1.0f) smoothing = 1.0f;

			// Try finding a camera to attach to!
			if (camera == null) {
				camera = Camera.main;
				if (camera == null) {
					return;
				}
			}

			var newPosition = camera.transform.position;
			MoveTowards(camera.transform, target, smoothing);

			newPosition.x = camera.transform.position.x;
			newPosition.y = camera.transform.position.y;

			camera.transform.position = newPosition;
		}

		public static void SetCameraPosition2D(Vector2 target, Camera camera = null) {
			if (camera == null) {
				camera = Camera.main;
				if (camera == null) {
					return;
				}
			}

			var newPosition = camera.transform.position;
			newPosition.x = target.x;
			newPosition.y = target.y;
			camera.transform.position = newPosition;
		}

		public static void HideCursor() {
			Cursor.lockState = CursorLockMode.Locked;
			Cursor.visible   = false;
		}

		public static void ShowCursor() {
			Cursor.lockState = CursorLockMode.None;
			Cursor.visible   = true;
		}

		public static void PlaySoundWithRandomPitch(AudioSource source, AudioClip[] sounds) {
			if (source == null) return;
			if (sounds == null || sounds.Length == 0) return;

			PlaySoundWithRandomPitch(source, sounds[Random.Range(0, sounds.Length)]);
		}

		// NOTE: Modifies the pitch of the AudioSource!
		public static void PlaySoundWithRandomPitch(AudioSource source, AudioClip sound) {
			if (source == null || sound == null) return;

			source.pitch = Random.Range(0.8f, 1.2f);
			source.PlayOneShot(sound);
		}

#if false
		// TODO: Check if targetWeight actually does something.
		// TODO: CameraFollowAll2D(Vector3[]), CameraFollowAll2D(Transform[])
		public static void CameraFollowAll2D(Vector2[] targets, float targetWeight = 0.5f, float smoothing = 0.5f, Camera camera = null) {
			if (smoothing < 0.0f)      smoothing = 0.0f;
			else if (smoothing > 1.0f) smoothing = 1.0f;

			if (camera == null) {
				camera = Camera.current;
				if (camera == null) {
					return;
				}
			}

			Vector2 offset = Vector2.zero;
			Vector2 cameraPosition = new Vector2(camera.transform.position.x, camera.transform.position.y);
			for (int i = 0; i < targets.Length; ++i) {
				offset += (targets[i] - cameraPosition) * targetWeight;
			}

			MoveTowards(camera.transform,
			            camera.transform.position + new Vector3(offset.x, offset.y, 0.0f),
			            smoothing);
		}
#endif

		// NOTE: Trims whitespace at the end of each line!!!!
		public static bool LoadTextResourceAsLines(string name, out string[] result) {
			TextAsset textAsset = Resources.Load<TextAsset>(name);
			if (textAsset == null) {
				Debug.LogError($"We expect a resource called '{name}' to exist, but there isn't one!");
				result = default;
				return false;
			}

			if (textAsset.text.Length == 0) {
				Debug.LogWarning($"No text in resource '{name}'.");
			}

			result = textAsset.text.Split('\n');

			if (result.Length > 0) {
				for (int i = 0; i < result.Length; ++i) {
					result[i] = result[i].TrimEnd();
				}

				if (String.IsNullOrEmpty(result[result.Length - 1])) {
					SetLength(ref result, result.Length - 1);
				}
			}

			return true;
		}
	}
}
