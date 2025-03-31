using System.Collections.Generic;
using UnityEngine;

public class MakeMoveable : MonoBehaviour, IResetable {
	const string crystalTag = "Crystal";

	LayerMask getStoppedBy = ~0;
	ContactFilter2D filter = new ContactFilter2D();
	private Rigidbody2D rb;

	public bool canBeMovedInConjunction = true;

	Vector3 originalPosition;
	void Start() {
		originalPosition = transform.position;
		filter.useTriggers = false;
		rb = GetComponent<Rigidbody2D>();
	}

	public void Reset() {
		transform.position = originalPosition;
	}

	public bool TryMoveInDirection(Vector2 direction) {
		var moveTo = direction;
		moveTo.x += transform.position.x;
		moveTo.y += transform.position.y;
		// Collider2D[] collidersAtTarget = Physics2D.OverlapCircleAll(moveTo, 0.1f);
		// Collider2D[] collidersAtTarget; 
		List<Collider2D> collidersAtTarget = new List<Collider2D>();
		Physics2D.OverlapCircle(moveTo, 0.1f, filter, collidersAtTarget);

		// foreach (var collider in collidersAtTarget)
		// {
		// 	print(this.name + " is being stopped from moving by: "  + collider.gameObject.name);
		// }

		bool canMove = collidersAtTarget.Count == 0;
		if (canBeMovedInConjunction) {
			for (int i = 0; i < collidersAtTarget.Count; ++i) {
				if (collidersAtTarget[i].TryGetComponent<MakeMoveable>(out var moveableAtTarget)) {
					if (moveableAtTarget.TryMoveInDirection(direction)) {
						canMove = true;
					}
				}
			}
		}
		// print(this.name + " is trying to be moved: " + canMove);
		if (!canMove) return false;

		var newPosition = transform.position;
		newPosition.x = moveTo.x;
		newPosition.y = moveTo.y;
		// transform.position = newPosition;
		// Physics.SyncTransforms();
		// Vector3 velocity = (newPosition - transform.position) / 0.5f;
		// print("Moving: " + this.name + " with " + velocity + " speed");
		// rb.linearVelocity = velocity;
		rb.MovePosition(newPosition);

		// @Hardcoded
		if (CompareTag(crystalTag)) {
			FMODController.PlaySound(FMODController.Sound.SFX_CrystalPush);
		} else {
			FMODController.PlaySound(FMODController.Sound.SFX_BoxPush);
		}

		return true;
	}
}
