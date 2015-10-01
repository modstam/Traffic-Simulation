using UnityEngine;
using System.Collections;
using UnityEngine.UI;
public class SpawnInterval : MonoBehaviour {

	public float spawnInterval;
	public InputField field;

	// Use this for initialization
	void awake () {
		this.field = this.GetComponent<InputField> ();
	}
	
	// Update is called once per frame
	void Update () {
	
	}


	public void submitSpawnInterval(){
		this.field = this.GetComponent<InputField> ();
		spawnInterval = float.Parse (field.text);
		Debug.Log ("Updating spawn interval to " +spawnInterval);


		Simulator simulator = FindObjectOfType<Simulator> ();
		if (simulator) {
			simulator.setSpawnInterval(spawnInterval);	
		}
	}
}
