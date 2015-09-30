using UnityEngine;
using System;
using System.Collections.Generic;

public static class Bezier{
	


	/**
	 * Uses the cubic bezier formula to return a point on a plottet bezier curve,
	 * Supply the method with 4 input reference points and a progress float t
	 * */
	public static Vector3 BezierCurve(Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3, float t) {
		t = Mathf.Clamp01(t);
		float oneMinusT = 1f - t;
		return (
			oneMinusT * oneMinusT * oneMinusT * p0 +
				3f * oneMinusT * oneMinusT * t * p1 +
				3f * oneMinusT * t * t * p2 +
				t * t * t * p3);
	}

	/**
	 * Updates the position of a control node
	 * */
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
