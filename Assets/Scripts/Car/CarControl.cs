using UnityEngine;
using System.Collections;

public class CarControl : MonoBehaviour {

	private float speedLimit = 0;
	private Rigidbody myBody;
	public float distanceTraveled = 0;
	public float targetDistance = 0;
	private Vector3 startPos;
	private Vector3 targetDirection;
    private bool going = false;

	public Transform graphicTransform;
    public Car myCar;


	void Awake(){
		startPos = transform.position;
	}

	// Use this for initialization
	void Start () {
		//transform.rotation = Quaternion.LookRotation(new Vector3(1,0,0));
		//Go (1000, 20, new Vector3(0,0,1));
	}

	public void Go(float distance, float speedLimit, Vector3 direction) {
		targetDirection = direction;
		if(direction.y != 0f)
			Debug.Log("New direction: " + direction);
		transform.rotation = Quaternion.LookRotation(direction);
		startPos = transform.position;
		distanceTraveled = 0;
		this.speedLimit = speedLimit;
		targetDistance = distance;
        //myBody.velocity = speedLimit * direction;
        going = true;
	}

	void Stop() {
		//myBody.velocity = Vector3.zero;
		speedLimit = 0;
        going = false;
        myCar.onStop();
	}
	
	// Update is called once per frame
	void Update () {
        if (going)
        {
            distanceTraveled = (startPos - transform.position).magnitude;


            if (distanceTraveled > targetDistance)
            {
                Stop();
                //Go(Random.Range(500f, 1500f), Random.Range(10f, 25f), new Vector3(Random.Range(-1f, 1f), 0, Random.Range(-1f, 1f)));
            }
            else
            {
                Movement();
            }
        }
	}

	void  Movement (){
		transform.Translate(transform.forward * 1 * Time.deltaTime * speedLimit, Space.World);
		
	}

    public void SetCar(Car car)
    {
        myCar = car;
    }


}
