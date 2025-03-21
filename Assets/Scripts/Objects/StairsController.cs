using UnityEngine;

public class StairsController : MonoBehaviour, IInteractable {
	public bool stairsGoUpwards;

	public void Interact() {
		if (stairsGoUpwards) {
			PlayerController.instance.GoUpStairs(transform.position);
		} else {
			PlayerController.instance.GoDownStairs(transform.position);
		}
	}
}
