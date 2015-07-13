using UnityEngine;
using System.Collections;
using System;

public class Network : MonoBehaviour {

	public Road[] roads;

	// Use this for initialization
	void Start () {
		
	}

	public void Reset(){
		roads = new Road[1];
		roads[0] = new Road();
		roads[0].Reset();
	}

	public void AddRoad(int roadIndex, int pointIndex){

		if(pointIndex == roads[roadIndex].points.Length-1) roads[roadIndex].AddRoad(pointIndex); 
		else
		{

			//TODO fix connections, correct spawnpoints etc etc
			Array.Resize(ref roads, roads.Length+1);
			roads[roads.Length-1] = new Road();
			roads[roads.Length-1].Reset();

		}

	}

	public Road GetRoad(int index){
		return roads[index];
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

}
