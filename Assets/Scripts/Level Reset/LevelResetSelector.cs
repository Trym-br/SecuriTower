using UnityEngine;
using static AugustBase.All;

public class LevelResetSelector : MonoBehaviour {
	public IResetable[] GetResetables() {
		var chunks = new LevelResetSelectorChunk[0];
		var resetables = new IResetable[0];

		for (int i = 0; i < transform.childCount; ++i) {
			var child = transform.GetChild(i);

			if (child.TryGetComponent(out LevelResetSelectorChunk chunk)) {
				Append(ref chunks, chunk);
			} else {
				// If the thing is not a chunk, try reseting it anyway.
				AppendAll(ref resetables, child.GetComponentsInChildren<IResetable>(true));
			}
		}

		for (int i = 0; i < chunks.Length; ++i) {
			// TODO: To avoid a very rare edge-case, we may want to get an SDF
			//       of the player vs chunk bounds and compare distances instead.
			if (chunks[i].PlayerIsWithinChunkBounds()) {
				AppendAll(ref resetables, chunks[i].gameObject.GetComponentsInChildren<IResetable>(true));
			}
		}

		return resetables;
	}
}
