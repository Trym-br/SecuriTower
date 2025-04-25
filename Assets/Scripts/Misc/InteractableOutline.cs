using UnityEngine;

public class InteractableOutline : MonoBehaviour {
	Material material;
	bool isEnabled;

	void Start() {
        material = GetComponent<Renderer>().material;

		isEnabled = true;
		DisableOutline();
	}

    public void EnableOutline() {
		if (isEnabled) return;

        material.SetFloat("_Alpha", 1.0f);
		isEnabled = true;
    }

    public void DisableOutline() {
		if (!isEnabled) return;

        material.SetFloat("_Alpha", 0.0f);
		isEnabled = false;
    }
}
