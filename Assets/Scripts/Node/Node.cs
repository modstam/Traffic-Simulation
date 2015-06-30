using UnityEngine;
using System.Collections;

public class Node : MonoBehaviour {


	public Node[] neighbors;

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {

	}

	void OnDrawGizmos(){
		for(int i = 0; i < neighbors.Length; ++i){
			if(neighbors[i] != null){
				Vector3 start = this.transform.position;
				Vector3 end = neighbors[i].transform.position;			
				Gizmos.DrawLine(start ,end);
			}
		}
	}
}
