using UnityEngine;
using FMODUnity;
using FMOD.Studio;

public class DemoMusicPlayer : MonoBehaviour {
	public EventReference musicEvent;
	void Start() {
		RuntimeManager.PlayOneShot(musicEvent);
	}
}
