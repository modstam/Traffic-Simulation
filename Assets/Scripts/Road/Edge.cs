using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;


[System.Serializable]
public class Edge{
	[SerializeField]
	public int n0; //node 0
	[SerializeField] 
	public int n1; //node 1
	[SerializeField]
	public int c0; //control 0
	[SerializeField]
	public int c1; //control 1
	[SerializeField]
	public bool reverse = false;



	public Edge(){

	}


	public Vector3 Progress(float t){
        //return Bezier.BezierCurve(n0, n1, c0, c1, t);
        return new Vector3();
    }

	public override string ToString ()
	{	
		if(reverse)
			return  "" + n1 + "->" + n0 + " (R)";
		else
			return  "" +n0 + "->" + n1;
	}

}
