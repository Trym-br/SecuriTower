using UnityEngine;

public class VolumeSliderModifier : MonoBehaviour {
	public void SetVolume(FMODController.VolumeSlider slider, float volume) {
		if (FMODController.instance == null) {
			Debug.LogError($"Trying to set '{slider}' volume to '{volume}' without the '{nameof(FMODController)} loaded!'");
			return;
		}

		FMODController.instance.SetVolume(slider, volume);
	}

	public float GetVolume(FMODController.VolumeSlider slider) {
		if (FMODController.instance == null) {
			Debug.LogError($"Trying to get the '{slider}' volume without the '{nameof(FMODController)} loaded!'");
			return 0.0f;
		}

		return FMODController.instance.GetVolume(slider);
	}
}
