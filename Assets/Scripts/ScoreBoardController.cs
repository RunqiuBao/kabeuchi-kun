using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
public class ScoreBoardController : MonoBehaviour {
	Renderer renderer;


	// Use this for initialization
	void Start () {
		renderer = GetComponent<Renderer>();
	}
	
	// Update is called once per frame
	void Update () {
		
	}


	public void enterReplay() {
        var MessageTextObj = GameObject.Find("ScoreBoardText2");
        Text txt = MessageTextObj.GetComponent<Text>();
        txt.text = "-REPLAY-";
		renderer.material.color = Color.grey;
	}

	public void enterPlay()  {
		var MessageTextObj = GameObject.Find("ScoreBoardText2");
        Text txt = MessageTextObj.GetComponent<Text>();
        txt.text = "--LIVE--";
		renderer.material.color = Color.black;
	}
}
