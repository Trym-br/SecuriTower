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
		public bool isResetSpell; // @Hardcoded
		public int  id;
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

		currentlyPlayingSounds = new();

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

	// An id of 0 is invalid.
	int currentId = 0;
	int NewID() {
		currentId += 1;
		return currentId;
	}

	List<PlayingSound> currentlyPlayingSounds = new();

	public int PlayFMODSoundEvent(EventReference eR, bool ignorePausing = false, bool isVoiceLine = false, bool isResetSpell = false) {
		EventInstance eI = RuntimeManager.CreateInstance(eR);
		eI.start();

		var id = NewID();

		PlayingSound playingSound;
		playingSound.eventInstance = eI;
		playingSound.ignorePausing = ignorePausing || pauseAudio;
		playingSound.isVoiceLine = isVoiceLine;
		playingSound.isResetSpell = isResetSpell;
		playingSound.id = id;

		currentlyPlayingSounds.Add(playingSound);

		return id;
	}

	public int PlayFMODSoundEvent(string eR, bool ignorePausing = false, bool isVoiceLine = false, bool isResetSpell = false) {
		EventInstance eI = RuntimeManager.CreateInstance(eR);
		eI.start();

		var id = NewID();

		PlayingSound playingSound;
		playingSound.eventInstance = eI;
		playingSound.ignorePausing = ignorePausing || pauseAudio;
		playingSound.isVoiceLine = isVoiceLine;
		playingSound.isResetSpell = isResetSpell;
		playingSound.id = id;

		currentlyPlayingSounds.Add(playingSound);

		return id;
	}

	public int PlayFMODSoundEventFrom(string eR, GameObject obj, bool ignorePausing = false, bool isVoiceLine = false, bool isResetSpell = false) {
		EventInstance eI = RuntimeManager.CreateInstance(eR);
		RuntimeManager.AttachInstanceToGameObject(eI, obj);
		eI.start();

		var id = NewID();

		PlayingSound playingSound;
		playingSound.eventInstance = eI;
		playingSound.ignorePausing = ignorePausing || pauseAudio;
		playingSound.isVoiceLine = isVoiceLine;
		playingSound.isResetSpell = isResetSpell;
		playingSound.id = id;

		currentlyPlayingSounds.Add(playingSound);

		return id;
	}

	public int PlayFMODSoundEventFrom(EventReference eR, GameObject obj, bool ignorePausing = false, bool isVoiceLine = false, bool isResetSpell = false) {
		EventInstance eI = RuntimeManager.CreateInstance(eR);
		RuntimeManager.AttachInstanceToGameObject(eI, obj);
		eI.start();

		var id = NewID();

		PlayingSound playingSound;
		playingSound.eventInstance = eI;
		playingSound.ignorePausing = ignorePausing || pauseAudio;
		playingSound.isVoiceLine = isVoiceLine;
		playingSound.isResetSpell = isResetSpell;
		playingSound.id = id;

		currentlyPlayingSounds.Add(playingSound);

		return id;
	}

	public UnityEvent onVoiceLineEnd = new();

	bool shouldStopResetSpell;

	void FixedUpdate() {
		if (currentlyPlayingSounds != null) {
			for (int i = 0; i < currentlyPlayingSounds.Count; ++i) {
				var it = currentlyPlayingSounds[i];

				PLAYBACK_STATE state;
				FMOD.RESULT fmod_result = it.eventInstance.getPlaybackState(out state);
				if (fmod_result == FMOD.RESULT.OK) {
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

						if (shouldStopResetSpell && it.isResetSpell) {
							currentlyPlayingSounds[i].eventInstance.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
							currentlyPlayingSounds.RemoveAt(i);
							i -= 1;

							shouldStopResetSpell = false;
						}
					}
				} else {
					Debug.LogError($"Could not get playback state of {it.eventInstance.ToString()}. (FMOD returned {fmod_result})");
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

	public static int PlaySound(Sound sound, bool ignorePausing = false, bool isVoiceLine = false) {
		if (FMODController.instance == null) {
			Debug.LogError($"Trying to play sound '{sound}' without the '{nameof(FMODController)}' loaded!");
			return 0;
		}

		return instance._PlaySound(sound, ignorePausing, isVoiceLine);
	}
	int _PlaySound(Sound sound, bool ignorePausing = false, bool isVoiceLine = false) {
		if (sound == Sound.None) return 0;

		var soundPath = GetSoundPath(sound);
		if (soundPath == default) return 0;
		return instance.PlayFMODSoundEvent(soundPath, ignorePausing, isVoiceLine);
	}

	public static int PlaySoundFrom(Sound sound, GameObject obj, bool ignorePausing = false, bool isVoiceLine = false) {
		if (FMODController.instance == null) {
			Debug.LogError($"Trying to play sound '{sound}' without the '{nameof(FMODController)}' loaded!");
			return 0;
		}

		return instance._PlaySoundFrom(sound, obj, ignorePausing, isVoiceLine);
	}
	int _PlaySoundFrom(Sound sound, GameObject obj, bool ignorePausing = false, bool isVoiceLine = false) {
		if (sound == Sound.None) return 0;

		var soundPath = GetSoundPath(sound);
		if (soundPath == default) return 0;
		return PlayFMODSoundEventFrom(soundPath, obj, ignorePausing, isVoiceLine);
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

	public static int PlayVoiceLineAudio(string path) {
		if (FMODController.instance == null) {
			Debug.LogError($"Trying to play sound from path '{path}' without the '{nameof(FMODController)}' loaded!");
			return 0;
		}

		return FMODController.instance.PlayFMODSoundEvent(path, false, true);
	}

	public static int PlayVoiceLineAudioFrom(string path, GameObject obj) {
		if (FMODController.instance == null) {
			Debug.LogError($"Trying to play sound from path '{path}' without the '{nameof(FMODController)}' loaded!");
			return 0;
		}

		return FMODController.instance.PlayFMODSoundEventFrom(path, obj, false, true);
	}

	public static void StopSound(int id) {
		if (FMODController.instance == null) {
			Debug.LogError($"Trying to stop sound '{id}' without the '{nameof(FMODController)}' loaded!");
			return;
		}

		if (id <= 0) {
			Debug.LogError($"Trying to stop a sound with an invalid id of {id}!");
			return;
		}

		FMODController.instance._StopSound(id);
	}
	void _StopSound(int id) {
		if (currentlyPlayingSounds == null) return;

		for (int i = 0; i < currentlyPlayingSounds.Count; ++i) {
			if (currentlyPlayingSounds[i].id == id) {
				currentlyPlayingSounds[i].eventInstance.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);

				// There shouldn't be multiple sounds with the same id.
				break;
			}
		}
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
		WaitingSpace,
		Credits,
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
		SFX_StaffPickup,
		SFX_BowRemove,
	}

	public static void BeginResetSpell() {
		if (FMODController.instance == null) return;
		FMODController.instance.shouldStopResetSpell = false;
		FMODController.instance.PlayFMODSoundEvent("event:/VO/Wizard/vo_wizzard_spell_s", false, false, true);
	}

	public static void StopResetSpell() {
		if (FMODController.instance == null) return;
		FMODController.instance.shouldStopResetSpell = true;
	}

	void _ResetSpellComplete() {
		for (int i = 0; i < currentlyPlayingSounds.Count; ++i) {
			// Dumbass C#
			var it = currentlyPlayingSounds[i];
			it.isResetSpell = false;
			currentlyPlayingSounds[i] = it;
		}
	}
	public static void ResetSpellComplete() {
		if (FMODController.instance == null) return;
		FMODController.instance._ResetSpellComplete();
	}

	public static void PlayLaserChainSound(GameObject playFrom) {
		// @Temp TODO: change for actual sound when Robin has it in the thing.
		PlaySoundFrom(Sound.SFX_GateClose, playFrom);
		print($"Playing laser chain sound from {playFrom.name}");
	}
}
