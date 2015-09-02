using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

[System.Serializable]
[ExecuteInEditMode]
public class Simulator : MonoBehaviour {

	public bool DEBUG_PATHFINDING = false;

	private int NUM_ERRORS = 0;

	[SerializeField]
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
		if (DEBUG_PATHFINDING && Application.isPlaying) {
			DEBUG_PATHFINDING = false;
			TestPathFinding();

		}
	}


	void TestPathFinding(){
		Debug.Log ("Testing pathfinding...this may take a while");
		int nr = 0;
		int nr_errors = 0;

		for (int x = 0; x < network.nodes.Count; ++x) {
			if(network.nodes[x].isControlPoint) continue;
			for (int y = 0; y < network.nodes.Count; ++y) {
				if(network.nodes[y].isControlPoint) continue;
				if(x==y) continue; //don't test path from and to the same node
			
				List<Edge> path = network.PathTo(x,y);
				bool pass = CheckPath(path,x,y);
				if(!pass) ++nr_errors; 
				string[] listString = new string[path.Count];
				for(int i = 0; i < path.Count; ++i){
					listString[i] = path[i].ToString();
				}
				Debug.Log (pass + ": Test nr " + nr + ": from " + x + " to " + y + "; List size: " + path.Count + ": " + string.Join(", ",listString) );
				++nr;
			}	
		}
		Debug.Log ("Testing complete, errors: " + nr_errors); 
	}

	private bool CheckPath(List<Edge> path, int start, int goal){

		//check if we can reach our destination through the edges
		bool edge_check = true;

		if (path == null || path.Count == 0) {
			return true;
		}

		if (path.Count == 1) {
			if(path[0].reverse)
				return (path[0].n1 == start && path[0].n0 == goal);
			else
				return (path[0].n0 == start && path[0].n1 == goal);
		}

		Edge prevEdge;
		Edge curEdge; 
		for (int i = 1; i < path.Count && edge_check; ++i) {
			prevEdge = path[i-1];
			curEdge = path[i];

			if(prevEdge.reverse && curEdge.reverse){
				if(!(prevEdge.n0 == curEdge.n1))
					edge_check = false;
			}
			else if(prevEdge.reverse && !curEdge.reverse){
				if(!(prevEdge.n0 == curEdge.n0))
					edge_check = false;
			}
			else if(!prevEdge.reverse && curEdge.reverse){
				if(!(prevEdge.n1 == curEdge.n1))
					edge_check = false;
			}
			else if(!prevEdge.reverse && !curEdge.reverse) {
				if(!(prevEdge.n1 == curEdge.n0))
					edge_check = false;
			}
		}

		Debug.Assert (edge_check);
		return edge_check;
	}
}
