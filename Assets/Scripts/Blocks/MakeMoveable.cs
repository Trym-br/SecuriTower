using UnityEngine;

public class MakeMoveable : MonoBehaviour {
	LayerMask getStoppedBy = ~0;

	public bool canBeMovedInConjunction = true;

	Vector3 originalPosition;
	void Start() {
		originalPosition = transform.position;
	}

	public void Reset() {
		transform.position = originalPosition;
	}

	public bool TryMoveInDirection(Vector2 direction) {
		var moveTo = direction;
		moveTo.x += transform.position.x;
		moveTo.y += transform.position.y;
		Collider2D[] collidersAtTarget = Physics2D.OverlapCircleAll(moveTo, 0.01f, getStoppedBy);

		bool canMove = collidersAtTarget.Length == 0;
		if (canBeMovedInConjunction) {
		for (int i = 0; i < collidersAtTarget.Length; ++i) {
			if (collidersAtTarget[i].TryGetComponent<MakeMoveable>(out var moveableAtTarget)) {
				if (moveableAtTarget.TryMoveInDirection(direction)) {
					canMove = true;
				}
			}
		}
		}

		if (!canMove) return false;

		var newPosition = transform.position;
		newPosition.x = moveTo.x;
		newPosition.y = moveTo.y;
		transform.position = newPosition;

		return true;
	}
}
