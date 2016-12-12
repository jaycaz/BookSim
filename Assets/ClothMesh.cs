using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ClothMesh : MonoBehaviour {

    public SkinnedMeshRenderer cloth;
    MeshCollider collider;

	// Use this for initialization
	void Start () {
        Debug.Assert(cloth != null);
        collider = GetComponent<MeshCollider>();
        collider.sharedMesh = (cloth.sharedMesh);
	}
	
	// Update is called once per frame
	void Update () {
        Debug.Log(cloth.sharedMesh.vertices);
        Vector3[] verts = collider.sharedMesh.vertices;
        verts = cloth.sharedMesh.vertices;
        collider.sharedMesh.vertices = verts;
	}
}
