using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GrabLine : MonoBehaviour {

    public LineRenderer line;
    int N;

    bool isGrabbing;
    int grabVertex;
    Vector3 lastMousePos;

	// Use this for initialization
	void Start () {
        Debug.Assert(line != null);
        N = line.numPositions;
	}
	
	// Update is called once per frame
	void Update () {
        float minDist = Mathf.Infinity;
        int minVertex = -1;
        Vector3 mouseScreen = Input.mousePosition;
        for(int i = 0; i < N; i++)
        {
            // Find closest point to mouse
            Vector3 vertex = line.transform.TransformPoint(line.GetPosition(i));
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
            line.transform.TransformPoint(line.GetPosition(minVertex)) - Camera.main.transform.position,
            Color.red);

        //Color[] colors = new Color[N];
        //for(int i = 0; i < N; i++)
        //{
        //    colors[i] = Color.black;
        //}
        //colors[minVertex] = Color.red;

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
            Vector3 pos = line.GetPosition(grabVertex);
            pos += dmouse;
            line.SetPosition(grabVertex, pos);
        }

        lastMousePos = mouseWorld;
	}
}
