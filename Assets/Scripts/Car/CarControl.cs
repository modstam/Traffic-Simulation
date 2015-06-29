using UnityEngine;
using System.Collections;

public class CarControl : MonoBehaviour {

	private float speedLimit = 0;
	private Rigidbody myBody;

	void Awake(){
		myBody = GetComponent<Rigidbody>();
		if(!myBody)
			Debug.LogError("No rigidbody found on CarControl object!");
	}

	// Use this for initialization
	void Start () {
	
	}

	void Go(float distance, float speedLimit) {

	}

	
	// Update is called once per frame
	void Update () {
	
	}
}
