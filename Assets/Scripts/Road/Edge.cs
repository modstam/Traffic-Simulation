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

	public Edge(){

	}


	public Vector3 Progress(float t){

		return new Vector3();
	}

}
