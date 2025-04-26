using UnityEngine;
using UnityEngine.Rendering.Universal;

public class RGBGamingOrb : MonoBehaviour {
	public Light2D theLight;

	const float speed = 0.35f;

	void Update() {
		float h, s, v;
		Color.RGBToHSV(theLight.color, out h, out s, out v);

		h += speed * Time.deltaTime;
		if (h < 0.0f) h = 1.0f;
		if (h > 1.0f) h = 0.0f;

		theLight.color = Color.HSVToRGB(h, s, v);
	}
}
