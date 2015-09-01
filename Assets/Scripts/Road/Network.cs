using UnityEngine;
using System.Collections;
using System;
using System.Collections.Generic;

[System.Serializable]
public class Network : MonoBehaviour {

	[SerializeField]
	public List<Road> roads;

	[SerializeField]
	public int numCars = 0;

	[SerializeField]
	public List<Node> nodes;
	

	// Use this for initialization
	void Start () {
		
	}

	// Update is called once per frame
	void Update () {
		
	}
	


	public void Reset(){
		nodes = new List<Node> ();
		roads = new List<Road> ();
		roads.Add (new Road());


		Debug.Log("Reset network");

		nodes.Add(new Node (new Vector3 (1f, 0f, 0f)));
		nodes.Add(new Node (new Vector3 (2f, 0f, 0f)));
		nodes.Add(new Node (new Vector3 (3f, 0f, 0f)));
		nodes.Add(new Node (new Vector3 (4f, 0f, 0f)));
	
		roads[0].Reset(new List<int>{0,1,2,3});

		AddConnection(0,3);



	}
	
	public void AddRoad(int road, int pointIndex){

		Debug.Log ("Adding road at " + road + ", " + pointIndex);

		if (pointIndex == roads [road].nodeIndexes.Count - 1) {

			int roadLength = roads[road].nodeIndexes.Count;
			Vector3 point = nodes[roads[road].nodeIndexes[roadLength-1]].pos;

			point.x += 1f;
			nodes.Add (new Node(point));
			point.x += 1f;
			nodes.Add (new Node(point));
			point.x += 1f;
			nodes.Add (new Node(point));


			int n = nodes.Count;

			Array.Resize(ref roads[road].modes, roads[road].modes.Length + 1);
			roads[road].modes[roads[road].modes.Length - 1] = roads[road].modes[roads[road].modes.Length - 2];
			Bezier.EnforceMode(this, road, roads[road].nodeIndexes.Count - 4);

			List<int> indexList = new List<int>{
				n-3,
				n-2,
				n-1
			};
			roads [road].AddRoad (indexList); //send the indexes of the added nodes to the road
			AddConnection(n-1, roads[road].nodeIndexes[pointIndex]);//add connnections between all the nodes		


		}
		else{
			Debug.Log ("index: " + pointIndex);

			roads.Add(new Road());

			Vector3 point = nodes[roads[road].nodeIndexes[pointIndex]].pos;

			Debug.Log(point);

			point.x += 1;
			nodes.Add (new Node(point));
			point.x += 1;
			nodes.Add (new Node(point));
			point.x += 1;
			nodes.Add (new Node(point));


			List<int> nodeIndexes = new List<int>(){
				roads[road].nodeIndexes[pointIndex],
				nodes.Count -3,
				nodes.Count -2,
				nodes.Count -1
			};


			roads[roads.Count-1].Reset(nodeIndexes);
			AddConnection(roads[roads.Count-1].nodeIndexes[0], nodes.Count-1);

		}

	}
	public bool addCars(int numCars){
		Debug.Log ("Resizing car population to " + numCars);


		return false;
	}

	public Road GetRoad(int index){
		return roads[index];
	}

	public Node GetNode(int roadIndex, int nodeIndex){
		return nodes[GetRoad(roadIndex).nodeIndexes[nodeIndex]];
	}
	


	public void Merge(int fromRoad, int fromNode, int toRoad, int toNode){
	
		Debug.Log ("Merging r"+ fromRoad + "n"+fromNode + " and r"+ toRoad + "n"+toNode);
		Road selectedRoad = GetRoad(fromRoad);
		Road mergeToRoad = GetRoad(toRoad);

		int nodeIndex = selectedRoad.nodeIndexes [fromNode]; 
		//nodes [selectedRoad.nodeIndexes [fromNode]].RemoveConnection (nodeIndex);

		Node selectedNode = GetNode(fromRoad,fromNode);
		for(int i = 0; i < selectedNode.NumConnections(); ++i ){
			int oldConnection = selectedNode.GetConnectionIndex(i);
			nodes[oldConnection].RemoveConnection(nodeIndex);
			
		}

		//nodes [selectedRoad.nodeIndexes [fromNode]] = nodes [mergeToRoad.nodeIndexes [toNode]] ;
		selectedRoad.nodeIndexes [fromNode] = mergeToRoad.nodeIndexes [toNode];
		AddConnections(mergeToRoad.nodeIndexes [toNode], nodes[selectedRoad.nodeIndexes[fromNode]].connections);



	}

	public void UnMerge(int roadIndex, int nodeIndex){
		Debug.Log ("Unmerging r"+ roadIndex + "n"+nodeIndex);
		//todo
	}

	public void AddConnections(int fromNode, List<int> toNodes){
		for (int i = 0; i < toNodes.Count; ++i) {
			AddConnection(fromNode,toNodes[i]);
		}
	}

	public void AddConnection(List<int> nodeIndexes){
		for( int i = 1; i < nodeIndexes.Count; ++i){
			AddConnection(nodeIndexes[i-1],nodeIndexes[i]);
		}
		
	}
	public void AddConnection(int index1, int index2){
		nodes[index1].AddConnection(index2);
		nodes[index2].AddConnection(index1);
	}

	public void setNodePos(int index, Vector3 pos){

	}

	public List<Edge> pathTo(Node source, Node destination){

		return new List<Edge>();
	}



}