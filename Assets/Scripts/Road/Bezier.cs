using UnityEngine;
using System;
using System.Collections.Generic;

public static class Bezier{
	

	public enum BezierControlPointMode {
		Free,
		Aligned,
		Mirrored
	}

	public static Vector3 GetPoint (Network network, Edge edge, float t) {
		int i;
		if (t >= 1f) {
			t = 1f;
			i = 4;
		}
		else {
			t = Mathf.Clamp01(t) * 4;
			i = (int)t;
			t -= i;
			i *= 3;
		}
		
		//convert to world space
		return network.transform.TransformPoint(BezierCurve(
			network.nodes[edge.n0].pos, 
			network.nodes[edge.c0].pos, 
		    network.nodes[edge.c1].pos, 
		    network.nodes[edge.n1].pos, 
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



	public static Vector3 GetFirstDerivative (Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3, float t) {
		t = Mathf.Clamp01(t);
		float oneMinusT = 1f - t;
		return
			3f * oneMinusT * oneMinusT * (p1 - p0) +
				6f * oneMinusT * t * (p2 - p1) +
				3f * t * t * (p3 - p2);
	}

	

	public static void SetControlPoint (Network network, Edge edge, Node node, Vector3 point) {

		Vector3 delta = point - node.pos;

		if (node == network.nodes[edge.n0] || node == network.nodes[edge.n1] ) {
			//	node.pos += delta;
			network.nodes[edge.c0].pos += delta;
			network.nodes[edge.c1].pos += delta;
	
		}


		else if (node == network.nodes[edge.c0]) {
			network.nodes[edge.c0].pos += delta;
		}
		else if (node == network.nodes[edge.c1]) {
			network.nodes[edge.c1].pos += delta;
		}

		node.pos = point;
		//EnforceMode(network, edge, pointindex);
	}




}
