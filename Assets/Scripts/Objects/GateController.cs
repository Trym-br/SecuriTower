using System;
using System.Linq;
using UnityEngine;

public class GateController : MonoBehaviour {
	[SerializeField] private CrystalController[] Inputs;

	Collider2D _collider2D;
	Animator animator;

	void Awake() {
		_collider2D = GetComponent<Collider2D>();
		animator = GetComponent<Animator>();
	}

	void FixedUpdate() {
		if (Inputs == null || Inputs.Length == 0) {
			Debug.LogError($"'{this.name}' does not have any receiver crystals set in the Inputs array.");
			return;
		}

		bool open = Inputs.All(x=> x.isLaserOn == true);
		_collider2D.enabled = !open;
		animator.SetBool("Open", open);
	}

	public void PlayOpeningSound() {
		FMODController.PlaySoundFrom(FMODController.Sound.SFX_GateOpen, gameObject);
	}

	public void PlayClosingSound() {
		FMODController.PlaySoundFrom(FMODController.Sound.SFX_GateClose, gameObject);
	}
}
