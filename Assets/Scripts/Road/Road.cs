using UnityEngine;
using System.Collections.Generic;
using System;

[System.Serializable]
public class Road {

	[SerializeField]
	public List<int> nodeIndexes; //This is the indexes for the nodes on our road
	[SerializeField]
	public Bezier.BezierControlPointMode[] modes;
	

	public void Reset(List<int> start){
		if(start.Count == 4){
			nodeIndexes = start;
						
			modes = new Bezier.BezierControlPointMode[] {
				Bezier.BezierControlPointMode.Free,
				Bezier.BezierControlPointMode.Free
			};
		}
		else throw new Exception(@"[Reset(Node[] start) -> Argument MUST be of length 4]");
		
	}
	

	public int ControlPointCount {
		get {
			return nodeIndexes.Count;
		}
	}
	
	
	public int GetControlPointIndex (int index) {

		return nodeIndexes [index];
	}
	
	

	public void AddRoad (List<int> indexes) {

		nodeIndexes.Add (indexes [0]);
		nodeIndexes.Add (indexes [1]);
		nodeIndexes.Add (indexes [2]);
		
		
	}
	
	public int nodeCount {
		get {
			return (nodeIndexes.Count - 1) / 3;
		}
	}

	public List<Node> GetRoadNodes(Network network){
		List<Node> list = new List<Node> ();
		for (int i = 0; i < nodeIndexes.Count; ++i) {
			list.Add(network.nodes[nodeIndexes[i]]);
		}
		return list;
	}
	
	

	

}