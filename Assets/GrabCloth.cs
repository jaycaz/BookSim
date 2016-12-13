using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GrabCloth : MonoBehaviour {

    public Cloth cloth;
    Mesh clothMesh;

    int grabVertex;
    bool isGrabbing;
    Vector3 lastMousePos;

	// Use this for initialization
	void Start () {
        Debug.Assert(cloth != null);
        clothMesh = cloth.GetComponent<SkinnedMeshRenderer>().sharedMesh;
        Debug.Assert(clothMesh != null);
	}
	
	// Update is called once per frame
	void Update () {
        float minDist = Mathf.Infinity;
        int minVertex = -1;
        Vector3 mouseScreen = Input.mousePosition;
        for(int i = 0; i < clothMesh.vertexCount; i++)
        {
            // Find closest point to mouse
            Vector3 vertex = cloth.transform.TransformPoint(clothMesh.vertices[i]);
            Vector3 vertexScreen = Camera.main.WorldToScreenPoint(vertex);
            float dist = Vector3.SqrMagnitude(vertexScreen - mouseScreen);
            if(dist < minDist)
            {
                minDist = dist;
                minVertex = i;
            }
        }
        Debug.LogFormat("Closest vertex: {0} ({1}m)", minVertex, minDist);
        Debug.DrawRay(Camera.main.transform.position, 
            cloth.transform.TransformPoint(clothMesh.vertices[minVertex]) - Camera.main.transform.position,
            Color.red);

        Color[] colors = new Color[clothMesh.vertexCount];
        for(int i = 0; i < clothMesh.vertexCount; i++)
        {
            colors[i] = Color.black;
        }
        colors[minVertex] = Color.red;
        clothMesh.colors = colors;

        if (Input.GetMouseButtonDown(0))
        {
            isGrabbing = true;
            grabVertex = minVertex;
        }
        if (Input.GetMouseButtonUp(0))
        {
            isGrabbing = false;
            grabVertex = -1;
        }

        Vector3 mouseWorld = Camera.main.ScreenToWorldPoint(mouseScreen);
        if(isGrabbing)
        {
            Vector3 dmouse = mouseWorld - lastMousePos;
            Vector3[] verts = clothMesh.vertices;
            verts[grabVertex] += dmouse;
            clothMesh.vertices = verts;
        }

        lastMousePos = mouseWorld;
	}
}