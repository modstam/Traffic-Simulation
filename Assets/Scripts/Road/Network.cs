using UnityEngine;
using System.Collections;
using System;
using System.Collections.Generic;

public class Network : MonoBehaviour {

	public List<Road> roads;

	public int numCars = 0;

	// Use this for initialization
	void Start () {
		
	}

	public void Reset(){
		roads = new List<Road>();
		roads.Add (new Road());
		roads[0].Reset();
	}

	public void AddRoad(int roadIndex, int pointIndex){

		if(pointIndex == roads[roadIndex].points.Length-1) roads[roadIndex].AddRoad(pointIndex); 
		else
		{
			Debug.Log ("index: " + pointIndex);

			roads.Add(new Road());

			Vector3 point = roads[roadIndex].points[pointIndex].pos;

			Debug.Log(point);

			point.x += 1;
			Node node1 = new Node(point);
			point.x += 1;
			Node node2 = new Node(point);
			point.x += 1;
			Node node3 = new Node(point);


			Node[] nodes = new Node[]{
					roads[roadIndex].points[pointIndex],
					node1,
					node2,
					node3
			};

			roads[roads.Count-1].Reset(nodes);

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
		return GetRoad(roadIndex).points[nodeIndex];
	}
	
	// Update is called once per frame
	void Update () {
	
	}


	public Vector3 GetPoint (Road road, float t) {
		int i;
		if (t >= 1f) {
			t = 1f;
			i = road.points.Length - 4;
		}
		else {
			t = Mathf.Clamp01(t) * road.RoadCount;
			i = (int)t;
			t -= i;
			i *= 3;
		}
		
		//convert to world space
		return transform.TransformPoint(Bezier(
			road,
			road.points[i].pos, 
			road.points[i + 1].pos, 
			road.points[i + 2].pos, 
			road.points[i + 3].pos, 
			t
			));
	}
	
	public Vector3 Bezier(Road road,Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3, float t) {
		t = Mathf.Clamp01(t);
		float oneMinusT = 1f - t;
		return
			oneMinusT * oneMinusT * oneMinusT * p0 +
				3f * oneMinusT * oneMinusT * t * p1 +
				3f * oneMinusT * t * t * p2 +
				t * t * t * p3;
	}
	

	//This is the velocity along the curve
	public Vector3 GetVelocity (Road road, float t) {
		int i;
		if (t >= 1f) {
			t = 1f;
			i = road.points.Length - 4;
		}
		else {
			t = Mathf.Clamp01(t) * road.RoadCount;
			i = (int)t;
			t -= i;
			i *= 3;
		}
		return transform.TransformPoint(road.GetFirstDerivative(
			road.points[0].pos, 
		    road.points[1].pos,
		    road.points[2].pos, 
		    road.points[3].pos, t)) - transform.position;
	}


	public Vector3 GetDirection (Road road, float t) {
		return GetVelocity(road,t).normalized;
	}

	public void Merge(int fromRoad, int fromNode, int toRoad, int toNode){
		Debug.Log ("Merging r"+ fromRoad + "n"+fromNode + " and r"+ toRoad + "n"+toNode);
		Road selectedRoad = GetRoad(fromRoad);
		Road mergeToRoad = GetRoad(toRoad);

		mergeToRoad.AddConnection(toNode,GetNode(fromRoad,fromNode).connections.ToArray());

		Node selectedNode = GetNode(fromRoad,fromNode);
		for(int i = 0; i < selectedNode.NumConnections(); ++i ){
			selectedNode.GetConnection(i).RemoveConnection(selectedNode);
		}

		selectedRoad.points[fromNode] = mergeToRoad.points[toNode];


	}

	public void UnMerge(int roadIndex, int nodeIndex){
		Debug.Log ("Unmerging r"+ roadIndex + "n"+nodeIndex);
	}

}
