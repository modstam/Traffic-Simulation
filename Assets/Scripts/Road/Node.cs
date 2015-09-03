using UnityEngine;
using System.Collections.Generic;
using System;

[System.Serializable]
public class Node{

	[SerializeField]
	public List<int> connections;

	[SerializeField]
	public Vector3 pos;

	[SerializeField]
	public Quaternion rot;

	[SerializeField]
	public bool isControlPoint;

	[SerializeField]
	public bool isActive = true;

	public Node(Vector3 pos){
		this.pos = pos;
		connections = new List<int>();
		isControlPoint = false;
	}

	public Node(Vector3 pos, Quaternion rot){
		this.pos = pos;
		this.rot = rot;
		connections = new List<int>();
		isControlPoint = false;
	}
	
	public void Reset(){
		connections = new List<int>();
	}

	public void AddConnection(int nodeindex){
		if (!connections.Contains (nodeindex))
			connections.Add (nodeindex);
	}

	public void RemoveConnection(int nodeindex){
		if (connections.Contains (nodeindex))
			connections.Remove (nodeindex);
	}


	public int NumConnections(){
		return connections.Count;
	}

	public int GetConnectionIndex(int connectionIndex){
		return connections[connectionIndex];
	}

	public Node GetConnectionNode(Network network, int connectionIndex){
		return network.nodes[connections[connectionIndex]];
	}

	public bool HasConnection(int index){
		return connections.Contains (index);
	}

	
}
