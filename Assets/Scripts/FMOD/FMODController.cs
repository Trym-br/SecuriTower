using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using FMODUnity;
using FMOD.Studio;

public class FMODController : MonoBehaviour {
	public static FMODController instance;

	[Serializable]
	public struct PlayingSound {
		public EventInstance eventInstance;
		public bool ignorePausing;
		public bool isVoiceLine;
	}

	[Serializable]
	public struct SoundToEventReference {
		public Sound sound;
		public EventReference eventReference;
	}

	public SoundToEventReference[] soundToEventReference;

	void Awake() {
		if (instance != null) {
			Debug.LogError($"There should only be one '{nameof(FMODController)}'!");
			Destroy(this);
		}

		instance = this;

		currentMusicStage = MusicStage.TitleScreen;
		currentLevel = Level.Intro;
	}

	public bool pauseAudio;

	void Start() {
		PlaySound(Sound.Music, true);
	}

	List<PlayingSound> currentlyPlayingSounds = new();

	public void PlayFMODSoundEvent(EventReference eR, bool ignorePausing = false, bool isVoiceLine = false) {
		EventInstance eI = RuntimeManager.CreateInstance(eR);
		eI.start();

		PlayingSound playingSound;
		playingSound.eventInstance = eI;
		playingSound.ignorePausing = ignorePausing || pauseAudio;
		playingSound.isVoiceLine = isVoiceLine;

		currentlyPlayingSounds.Add(playingSound);
	}

	public void PlayFMODSoundEvent(string eR, bool ignorePausing = false, bool isVoiceLine = false) {
		EventInstance eI = RuntimeManager.CreateInstance(eR);
		eI.start();

		PlayingSound playingSound;
		playingSound.eventInstance = eI;
		playingSound.ignorePausing = ignorePausing || pauseAudio;
		playingSound.isVoiceLine = isVoiceLine;

		currentlyPlayingSounds.Add(playingSound);
	}

	public void PlayFMODSoundEventFrom(string eR, GameObject obj, bool ignorePausing = false, bool isVoiceLine = false) {
		EventInstance eI = RuntimeManager.CreateInstance(eR);
		RuntimeManager.AttachInstanceToGameObject(eI, obj);
		eI.start();

		PlayingSound playingSound;
		playingSound.eventInstance = eI;
		playingSound.ignorePausing = ignorePausing || pauseAudio;
		playingSound.isVoiceLine = isVoiceLine;

		currentlyPlayingSounds.Add(playingSound);
	}

	public void PlayFMODSoundEventFrom(EventReference eR, GameObject obj, bool ignorePausing = false, bool isVoiceLine = false) {
		EventInstance eI = RuntimeManager.CreateInstance(eR);
		RuntimeManager.AttachInstanceToGameObject(eI, obj);
		eI.start();

		PlayingSound playingSound;
		playingSound.eventInstance = eI;
		playingSound.ignorePausing = ignorePausing || pauseAudio;
		playingSound.isVoiceLine = isVoiceLine;

		currentlyPlayingSounds.Add(playingSound);
	}

	public UnityEvent onVoiceLineEnd = new();

	void FixedUpdate() {
		if (currentlyPlayingSounds != null) {
			for (int i = 0; i < currentlyPlayingSounds.Count; ++i) {
				var it = currentlyPlayingSounds[i];

				if (it.eventInstance.getPlaybackState(out PLAYBACK_STATE state) == FMOD.RESULT.OK) {
					if (state == PLAYBACK_STATE.STOPPED) {
						if (it.isVoiceLine) {
							if (onVoiceLineEnd != null) {
								onVoiceLineEnd.Invoke();
								onVoiceLineEnd.RemoveAllListeners();
							}
						}

						currentlyPlayingSounds.RemoveAt(i);
						i -= 1;
					} else {
						if (!it.ignorePausing) {
							currentlyPlayingSounds[i].eventInstance.setPaused(pauseAudio);
						}
					}
				} else {
					Debug.LogError($"Could not get playback state of {it.ToString()}");
					currentlyPlayingSounds.RemoveAt(i);
					i -= 1;
				}
			}
		}
	}

	void Update() {
		RuntimeManager.StudioSystem.setParameterByName("Stage", (float)currentMusicStage);
		RuntimeManager.StudioSystem.setParameterByName("Level", (float)currentLevel);
	}

	public EventReference GetSoundPath(Sound sound) {
		if (soundToEventReference == null) return default;

		for (int i = 0; i < soundToEventReference.Length; ++i) {
			if (soundToEventReference[i].sound == sound) {
				return soundToEventReference[i].eventReference;
			}
		}

#if UNITY_EDITOR
		Debug.LogError($"No sound event set for sound '{sound.ToString()}'.");
#endif

		return default;
	}

	public void PlaySound(Sound sound, bool ignorePausing = false, bool isVoiceLine = false) {
		if (sound == Sound.None) return;

		var soundEvent = GetSoundPath(sound);
		PlayFMODSoundEvent(soundEvent, ignorePausing, isVoiceLine);
	}

	public void PlaySoundFrom(Sound sound, GameObject obj, bool ignorePausing = false, bool isVoiceLine = false) {
		if (sound == Sound.None) return;

		var soundEvent = GetSoundPath(sound);
		PlayFMODSoundEventFrom(soundEvent, obj, ignorePausing, isVoiceLine);
	}

	public void PlayFootstepSound(FootstepSoundType sound) {
		RuntimeManager.StudioSystem.setParameterByName("Footsteps", (float)sound);

		var footstepSoundEvent = GetSoundPath(Sound.Walking);
		PlayFMODSoundEvent(footstepSoundEvent);
	}

	public void PlayVoiceLineAudio(string path) {
		PlayFMODSoundEvent(path, false, true);
	}

	public void PlayVoiceLineAudioFrom(string path, GameObject obj) {
		PlayFMODSoundEventFrom(path, obj, false, true);
	}

	string GetVolumeSliderParameterName(VolumeSlider slider) {
		switch (slider) {
			case VolumeSlider.Master:       return "Master Volume";
			case VolumeSlider.Music:        return "MX Volume";
			case VolumeSlider.SoundEffects: return "SFX Volume";
			case VolumeSlider.Ambience:     return "AMB Volume";
			case VolumeSlider.VoiceLines:   return "VO Volume";

			case VolumeSlider.None: {
				Debug.LogError("Trying to get the parameter name of a none-slider!");
				return "";
			};
		}

		Debug.LogError("Trying to get the parameter name that does not exist!");
		return "";
	}

	public float GetVolume(VolumeSlider slider) {
		float volume = 0.0f;
		var parameterName = GetVolumeSliderParameterName(slider);
		if (parameterName == "") return volume;

		var fmodStatus = RuntimeManager.StudioSystem.getParameterByName(parameterName, out volume);

		if (fmodStatus != FMOD.RESULT.OK) {
			Debug.LogError($"FMOD is not ok! ({fmodStatus.ToString()}, trying to get parameter '{parameterName}')");
		}

		return volume;
	}

	public void SetVolume(VolumeSlider slider, float volume) {
		var parameterName = GetVolumeSliderParameterName(slider);
		if (parameterName == "") return;

		var fmodStatus = RuntimeManager.StudioSystem.setParameterByName(parameterName, volume);

		if (fmodStatus != FMOD.RESULT.OK) {
			Debug.LogError($"FMOD is not ok! ({fmodStatus.ToString()}, trying to get parameter '{parameterName}')");
		}
	}

	public enum VolumeSlider {
		None,
		Master,
		Music,
		SoundEffects,
		Ambience,
		VoiceLines
	}

	public MusicStage currentMusicStage;
	public enum MusicStage {
		TitleScreen,
		Intro,
		MainStage,
	}

	public Level currentLevel;
	public enum Level {
		Intro,
		Level1,
		Level2,
		Level3,
		Level4,
		Boss,
	}

	public enum FootstepSoundType {
		Stein,
		Teppe,
	}

	public enum Sound {
		None,
		Music,
		Walking,
	}
}
