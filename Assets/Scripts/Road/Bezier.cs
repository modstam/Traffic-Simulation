using UnityEngine;
using System.Collections.Generic;

public static class Bezier{
	

	public enum BezierControlPointMode {
		Free,
		Aligned,
		Mirrored
	}

	public static Vector3 GetPoint (Network network, Road road, float t) {
		int i;
		if (t >= 1f) {
			t = 1f;
			i = road.nodeIndexes.Count - 4;
		}
		else {
			t = Mathf.Clamp01(t) * road.nodeCount;
			i = (int)t;
			t -= i;
			i *= 3;
		}
		
		//convert to world space
		return network.transform.TransformPoint(BezierCurve(
			network.nodes[road.nodeIndexes[i]].pos, 
			network.nodes[road.nodeIndexes[i + 1]].pos, 
			network.nodes[road.nodeIndexes[i + 2]].pos, 
			network.nodes[road.nodeIndexes[i + 3]].pos, 
			t
			));
	}
	
	public static Vector3 BezierCurve(Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3, float t) {
		t = Mathf.Clamp01(t);
		float oneMinusT = 1f - t;
		return
			oneMinusT * oneMinusT * oneMinusT * p0 +
				3f * oneMinusT * oneMinusT * t * p1 +
				3f * oneMinusT * t * t * p2 +
				t * t * t * p3;
	}

	

	public static void EnforceMode (Network network, int road, int index) {
		List<Node> points = network.roads[road].GetRoadNodes(network); 
		
		int modeIndex = (index + 1) / 3;
		//Debug.Log (index + " : " + modeIndex + " : " +  network.roads [road].modes.Length + " : " + network.roads[road].nodeIndexes.Count);

		BezierControlPointMode mode = network.roads[road].modes[modeIndex];
	
		if (mode == BezierControlPointMode.Free || (modeIndex == 0 || modeIndex == network.roads[road].modes.Length - 1)) {
			return;
		}
		
		int middleIndex = modeIndex * 3;
		int fixedIndex, enforcedIndex;
		if (index <= middleIndex) {
			fixedIndex = middleIndex - 1;
			if (fixedIndex < 0) {
				fixedIndex = network.roads[road].nodeIndexes.Count - 2;
			}
			enforcedIndex = middleIndex + 1;
			if (enforcedIndex >= network.roads[road].nodeIndexes.Count) {
				enforcedIndex = 1;
			}
		}
		else {
			fixedIndex = middleIndex + 1;
			if (fixedIndex >= network.roads[road].nodeIndexes.Count) {
				fixedIndex = 1;
			}
			enforcedIndex = middleIndex - 1;
			if (enforcedIndex < 0) {
				enforcedIndex = network.roads[road].nodeIndexes.Count - 2;
			}
		}
		
		Vector3 middle = points[middleIndex].pos;
		Vector3 enforcedTangent = middle - points[fixedIndex].pos;
		if (mode == BezierControlPointMode.Aligned) {
			enforcedTangent = enforcedTangent.normalized * Vector3.Distance(middle, points[enforcedIndex].pos);
		}
		points[enforcedIndex].pos = middle + enforcedTangent;
	}


	public static Vector3 GetFirstDerivative (Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3, float t) {
		t = Mathf.Clamp01(t);
		float oneMinusT = 1f - t;
		return
			3f * oneMinusT * oneMinusT * (p1 - p0) +
				6f * oneMinusT * t * (p2 - p1) +
				3f * t * t * (p3 - p2);
	}


	public static BezierControlPointMode GetControlPointMode (Network network, int road, int pointindex) {
		//Debug.Log ("index: " + index + " size: " +  modes.Length);
		return network.roads[road].modes[(pointindex + 1) / 3];
	}



	public static void SetControlPoint (Network network, int road, int pointindex, Vector3 point) {
		List<Node> points = network.roads[road].GetRoadNodes(network); 

		if (pointindex % 3 == 0) {
			Vector3 delta = point - points[pointindex].pos;
			
			if (pointindex > 0) {
				points[pointindex - 1].pos += delta;
			}
			if (pointindex+ 1 < network.roads[road].nodeIndexes.Count) {
				points[pointindex + 1].pos += delta;
			}
		}
		points[pointindex].pos = point;
		EnforceMode(network, road, pointindex);
	}
	
	public static void SetControlPointMode (Network network, int road, int pointindex, BezierControlPointMode mode) {
		int modeIndex = (pointindex + 1) / 3;
		network.roads[road].modes[modeIndex] = mode;

		EnforceMode(network, road, pointindex);
	}




}
