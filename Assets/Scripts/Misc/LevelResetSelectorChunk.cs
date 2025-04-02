using UnityEngine;

// Any children of this chunk will get reset when 
public class LevelResetSelectorChunk : MonoBehaviour {
	public Vector2 chunkSize;
	public Vector2 chunkOffset;

	Vector3 GetChunkAreaPosition() {
		var pos = transform.position;

		pos.x += chunkOffset.x;
		pos.y += chunkOffset.y;
		pos.z = 0.0f;

		return pos;
	}

	public bool PlayerIsWithinChunkBounds() {
		Collider2D[] collidersInBounds = Physics2D.OverlapBoxAll(GetChunkAreaPosition(), chunkSize, 0.0f);

		// TODO: This is dumb. Just do a position comparison with PlayerController.instance.transform.position.
		for (int i = 0; i < collidersInBounds.Length; ++i) {
			if (collidersInBounds[i].CompareTag("Player")) {
				return true;
			}
		}

		return false;
	}

#if UNITY_EDITOR
	void OnDrawGizmos() {
		Gizmos.color = Color.blue;
		Gizmos.DrawWireCube(GetChunkAreaPosition(), chunkSize);
	}
#endif
}
