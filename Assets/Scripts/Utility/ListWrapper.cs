using UnityEngine;
using System.Collections.Generic;
using System;
using System.Collections;
/**
 * This class is nescessary since Unity does 
 * not support serialization of nested lists
 * 
 **/

[System.Serializable]
public class ListWrapper{

	[SerializeField]
	public List<Edge> edges;

	public ListWrapper(){
		edges = new List<Edge> ();
	}

}
