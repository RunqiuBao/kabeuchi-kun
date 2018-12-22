using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class KabeuchikunController : MonoBehaviour {

	float effectFadeSpeed = 2.0f;
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
	void hitEffect() {
		renderer.material.color = new Color(253f / 255f, 106f / 255f, 2f / 255f);
	}
	public void hit(){
		hitEffect();
	}
}
