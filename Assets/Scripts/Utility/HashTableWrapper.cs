using UnityEngine;
using System.Collections;
using System;

[System.Serializable]
public class HashTableWrapper{

	[SerializeField]
	public Hashtable t;

	public HashTableWrapper(){
		t = new Hashtable ();
	}


}
