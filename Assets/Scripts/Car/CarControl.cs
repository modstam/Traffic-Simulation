using UnityEngine;
using System.Collections;

public class CarControl : MonoBehaviour {

	private float speedLimit = 0;
	private Rigidbody myBody;
	private float distanceTraveled = 0;

	void Awake(){
		myBody = GetComponent<Rigidbody>();
		if(!myBody)
			Debug.LogError("No rigidbody found on CarControl object!");
	}

	// Use this for initialization
	void Start () {
		Go (10, 20, new Vector3(1,0,0));
	}

	void Go(float distance, float speedLimit, Vector3 direction) {
		this.speedLimit = speedLimit;
		myBody.velocity = speedLimit * direction;
	}

	
	// Update is called once per frame
	void Update () {
	
	}
}
