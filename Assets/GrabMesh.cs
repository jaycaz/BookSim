using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GrabMesh : MonoBehaviour {

    public MeshFilter meshFilter;
    public float moveSensitivity = 1.0f;
    Mesh mesh;

    int grabVertex;
    bool isGrabbing;
    Vector3 lastMousePos;

	// Use this for initialization
	void Start () {
        mesh = meshFilter.mesh;
        Debug.Assert(mesh != null);
	}
	
	// Update is called once per frame
	void Update () {
        float minDist = Mathf.Infinity;
        int minVertex = -1;
        Vector3 mouseWorld = Input.mousePosition;
        for(int i = 0; i < mesh.vertexCount; i++)
        {
            // Find closest point to mouse
            Vector3 vertex = meshFilter.transform.TransformPoint(mesh.vertices[i]);
            Vector3 vertexScreen = Camera.main.WorldToScreenPoint(vertex);
            float dist = Vector3.SqrMagnitude(vertexScreen - mouseWorld);
            if(dist < minDist)
            {
                minDist = dist;
                minVertex = i;
            }
        }
        Debug.LogFormat("Closest vertex: {0} ({1}m)", minVertex, minDist);
        Debug.DrawRay(Camera.main.transform.position, 
            meshFilter.transform.TransformPoint(mesh.vertices[minVertex])
            - Camera.main.transform.position,
            Color.red);

        Color[] colors = new Color[mesh.vertexCount];
        for(int i = 0; i < mesh.vertexCount; i++)
        {
            colors[i] = Color.black;
        }
        colors[minVertex] = Color.red;
        mesh.colors = colors;


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

        if(isGrabbing)
        {
            Vector3 dmouse = Input.mousePosition - lastMousePos;
            Vector3[] verts = mesh.vertices;
            verts[grabVertex] += dmouse * moveSensitivity;
            mesh.vertices = verts;
        }

        lastMousePos = Input.mousePosition;
	}
}
