using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ColorEffectController : MonoBehaviour {

	public float effectFadeSpeed = 1.0f;
	new Renderer renderer;
	Color initialColor;
	void Start () {
		// state = State.off;
		renderer = GetComponent<Renderer>();
		initialColor = renderer.material.color;

	}
	
	void Update () {
		renderer.material.color = Color.Lerp(renderer.material.color, initialColor, Time.deltaTime * effectFadeSpeed);
	}

	public void startColorEffect(Color color) {
		renderer.material.color = color;
	}
}
