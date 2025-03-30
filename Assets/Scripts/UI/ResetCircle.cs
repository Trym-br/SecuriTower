using UnityEngine;

public class ResetCircle : MonoBehaviour {

    private Material material;

    void Awake() {
        material = GetComponent<SpriteRenderer>().material;
    }
    void Update() {
        material.SetFloat("_FillAmount", PlayerController.instance.resetTimerProgress);
    }
}
