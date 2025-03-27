using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using FMODUnity;
using FMOD.Studio;
using static AugustBase.All;

public class FMODController : MonoBehaviour {
	public static FMODController instance;

	[Serializable]
	public struct PlayingSound {
		public EventInstance eventInstance;
		public bool ignorePausing;
		public bool isVoiceLine;
	}

	[Serializable]
	public struct SoundToSoundPath {
		public Sound sound;
		public string path;
	}

	public SoundToSoundPath[] soundToSoundPath;

	void Awake() {
		if (instance != null) {
			Debug.LogError($"There should only be one '{nameof(FMODController)}'!");
			Destroy(this);
		}

		instance = this;

		currentMusicStage = MusicStage.TitleScreen;
		currentLevel = Level.IntroLevel;
	}

	public bool pauseAudio;

	void Start() {
		LoadSoundPathsFromDisk();

		PlaySound(Sound.MX_MainTheme, true);
	}

	void LoadSoundPathsFromDisk() {
		const string allSoundPathsTextFileName = "AllSoundPaths";

		string[] soundPathsAsLines;
		if (!LoadTextResourceAsLines(allSoundPathsTextFileName, out soundPathsAsLines)) return;

		var countSounds = Enum.GetNames(typeof(Sound)).Length;
		if (soundPathsAsLines.Length != countSounds) {
			Debug.LogError($"The amount of lines in '{allSoundPathsTextFileName}' ({soundPathsAsLines.Length}) does not match the amount of items in the {nameof(Sound)} enum ({countSounds}).");
			return;
		}

		soundToSoundPath = new SoundToSoundPath[soundPathsAsLines.Length];

		for (int i = 0; i < soundPathsAsLines.Length; ++i) {
			soundToSoundPath[i].sound = (Sound)i;
			soundToSoundPath[i].path = soundPathsAsLines[i];
		}
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

	public string GetSoundPath(Sound sound) {
		if (soundToSoundPath == null) return default;

		for (int i = 0; i < soundToSoundPath.Length; ++i) {
			if (soundToSoundPath[i].sound == sound) {
				return soundToSoundPath[i].path;
			}
		}

#if UNITY_EDITOR
		Debug.LogError($"No sound event set for sound '{sound.ToString()}'.");
#endif

		return default;
	}

	public static void PlaySound(Sound sound, bool ignorePausing = false, bool isVoiceLine = false) {
		if (FMODController.instance == null) {
			Debug.LogError($"Trying to play sound '{sound}' without the '{nameof(FMODController)}' loaded!");
			return;
		}

		instance._PlaySound(sound, ignorePausing, isVoiceLine);
	}
	void _PlaySound(Sound sound, bool ignorePausing = false, bool isVoiceLine = false) {
		if (sound == Sound.None) return;

		var soundPath = GetSoundPath(sound);
		if (soundPath == default) return;
		instance.PlayFMODSoundEvent(soundPath, ignorePausing, isVoiceLine);
	}

	public static void PlaySoundFrom(Sound sound, GameObject obj, bool ignorePausing = false, bool isVoiceLine = false) {
		if (FMODController.instance == null) {
			Debug.LogError($"Trying to play sound '{sound}' without the '{nameof(FMODController)}' loaded!");
			return;
		}

		instance._PlaySoundFrom(sound, obj, ignorePausing, isVoiceLine);
	}
	void _PlaySoundFrom(Sound sound, GameObject obj, bool ignorePausing = false, bool isVoiceLine = false) {
		if (sound == Sound.None) return;

		var soundPath = GetSoundPath(sound);
		if (soundPath == default) return;
		PlayFMODSoundEventFrom(soundPath, obj, ignorePausing, isVoiceLine);
	}

	public static void PlayFootstepSound(FootstepSoundType sound) {
		if (FMODController.instance == null) {
			Debug.LogError($"Trying to play footstep sound '{sound}' without the '{nameof(FMODController)}' loaded!");
			return;
		}

		instance._PlayFootstepSound(sound);
	}
	void _PlayFootstepSound(FootstepSoundType sound) {
		RuntimeManager.StudioSystem.setParameterByName("Footsteps", (float)sound);

		var footstepSoundEvent = GetSoundPath(Sound.SFX_Walking);
		PlayFMODSoundEvent(footstepSoundEvent);
	}

	public static void PlayVoiceLineAudio(string path) {
		if (FMODController.instance == null) {
			Debug.LogError($"Trying to play sound from path '{path}' without the '{nameof(FMODController)}' loaded!");
			return;
		}

		FMODController.instance.PlayFMODSoundEvent(path, false, true);
	}

	public static void PlayVoiceLineAudioFrom(string path, GameObject obj) {
		if (FMODController.instance == null) {
			Debug.LogError($"Trying to play sound from path '{path}' without the '{nameof(FMODController)}' loaded!");
			return;
		}

		FMODController.instance.PlayFMODSoundEventFrom(path, obj, false, true);
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
		IntroCutscene,
		MainStage,
	}

	public Level currentLevel;
	public enum Level {
		IntroLevel,
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
		MX_MainTheme,
		SFX_Walking,
		SFX_UISelect,
		SFX_UIHover,
		SFX_GateOpen,
		SFX_GateClose,
		SFX_GlassShatter,
		VO_Wizard_PushGrunt,
		VO_Wizard_LaserDeath,
		SFX_ParasolOpen,
		SFX_ParasolClose,
		SFX_BoxDestroy,
		SFX_BoxPush,
		SFX_ResetSpell,
		SFX_CrystalPush,
		SFX_CrystalRotate,
		SFX_CrystalDestroy,
		SFX_CrystalExplode,
		SFX_LaserHum,
		SFX_LaserReceiver,
		SFX_LaserCharge,
		SFX_LaserChain,
		SFX_LaserEmitter,
		SFX_LaserSplitter,
		SFX_LaserSplitterPush,
		AMB_Wind,
		AMB_Torch,
		SFX_DoorLocked,
		SFX_DoorOpen,
	}
}
