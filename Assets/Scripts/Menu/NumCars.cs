using UnityEngine;
using System.Collections;
using UnityEngine.UI;
public class NumCars : MonoBehaviour {
	
	public int numCars;
	public InputField field;
	public Text text;
	Simulator simulator;
	
	// Use this for initialization
	void Start () {
		this.field = this.GetComponent<InputField> ();
		simulator = FindObjectOfType<Simulator> ();
	}
	
	// Update is called once per frame
	void Update () {
		if (simulator) {
			text.text = "Currently " + simulator.carsSpawned + " cars" + 
				" (" + simulator.carsToSpawn + ")" ;	
		}
	}
	
	
	public void submit(){
		this.field = this.GetComponent<InputField> ();
		numCars = int.Parse (field.text);
		Debug.Log ("Updating number of cars to " + numCars);

		simulator = FindObjectOfType<Simulator> ();
		if (simulator) {
			simulator.setNumCars(numCars);	
		}
	}
}
