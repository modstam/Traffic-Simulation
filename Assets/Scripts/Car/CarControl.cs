using UnityEngine;
using System.Collections;

public class CarControl : MonoBehaviour {

	private float speedLimit = 0;
	private Rigidbody myBody;
	private float distanceTraveled = 0;
	private float targetDistance = 0;
	private Vector3 startPos;

	void Awake(){
		myBody = GetComponent<Rigidbody>();
		if(!myBody)
			Debug.LogError("No rigidbody found on CarControl object!");
		startPos = transform.position;
	}

	// Use this for initialization
	void Start () {
		Go (1000, 20, new Vector3(1,0,0));
	}

	void Go(float distance, float speedLimit, Vector3 direction) {
		startPos = transform.position;
		distanceTraveled = 0;
		this.speedLimit = speedLimit;
		targetDistance = distance;
		myBody.velocity = speedLimit * direction;
	}

	void Stop() {
		myBody.velocity = Vector3.zero;
	}
	
	// Update is called once per frame
	void Update () {
		distanceTraveled += (startPos - transform.position).magnitude;
		if(distanceTraveled > targetDistance) {
			Stop ();
		}
	}
}
