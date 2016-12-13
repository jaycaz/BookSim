using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PageCollection : MonoBehaviour {

    LinePageSim[] pages;

	// Use this for initialization
	void Start () {
        pages = GetComponentsInChildren<LinePageSim>();	
	}
	
	// Update is called once per frame
	void Update () {
		
	}
}
