using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GrabCloth : MonoBehaviour {

    public Cloth cloth;
    Mesh clothMesh;

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
        Vector3 mousePoint = Input.mousePosition;
        for(int i = 0; i < clothMesh.vertexCount; i++)
        {
            // Find closest point to mouse
            Vector3 vertexScreen = Camera.main.WorldToScreenPoint(clothMesh.vertices[i]);
            float dist = Vector3.SqrMagnitude(vertexScreen - mousePoint);
            if(dist < minDist)
            {
                minDist = dist;
                minVertex = i;
            }
        }
        Debug.LogFormat("Closest vertex: {0} ({1}m)", minVertex, minDist);
        Debug.DrawRay(Camera.main.transform.position, 
            clothMesh.vertices[minVertex] - Camera.main.transform.position,
            Color.red);

        Color[] colors = new Color[clothMesh.vertexCount];
        for(int i = 0; i < clothMesh.vertexCount; i++)
        {
            colors[i] = Color.black;
        }
        colors[minVertex] = Color.red;
        clothMesh.colors = colors;
	}
}
