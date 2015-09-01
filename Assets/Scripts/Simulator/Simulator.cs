using UnityEngine;
using System.Collections;
using System;

[System.Serializable]
[ExecuteInEditMode]
public class Simulator : MonoBehaviour {
	
	Network network;

	void Awake(){
		if (!Application.isPlaying) {
			network = gameObject.GetComponent<Network>();
			if(network == null)
				this.network = gameObject.AddComponent<Network>();
		}
	}

	// Use this for initialization
	void Start () {

	}
	
	// Update is called once per frame
	void Update () {

	}
}
