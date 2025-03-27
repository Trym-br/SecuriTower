using UnityEngine;

public class MusicStageChanger : MonoBehaviour {
	// For the main menu.
	public void TitleScreenMusic() {
		if (FMODController.instance == null) {
			Debug.LogError($"Trying to set the music stage to 'Title Screen Music', but the '{nameof(FMODController)}' is not loaded!");
		} else {
			FMODController.instance.currentMusicStage = FMODController.MusicStage.TitleScreen;
		}
	}

	// For the intro cutscene.
	public void IntroCutsceneMusic() {
		if (FMODController.instance == null) {
			Debug.LogError($"Trying to set the music stage to 'Intro Cutscene', but the '{nameof(FMODController)}' is not loaded!");
		} else {
			FMODController.instance.currentMusicStage = FMODController.MusicStage.IntroCutscene;
		}
	}

	// For all the levels.
	public void MainStageMusic() {
		if (FMODController.instance == null) {
			Debug.LogError($"Trying to set the music stage to 'Main Stage', but the '{nameof(FMODController)}' is not loaded!");
		} else {
			FMODController.instance.currentMusicStage = FMODController.MusicStage.MainStage;
		}
	}
}
