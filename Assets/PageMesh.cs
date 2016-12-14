using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PageMesh : MonoBehaviour {

    public float pageHeight = 1.0f;
    public bool front = true;

    LineRenderer line;
    MeshRenderer mRenderer;
    MeshFilter mFilter;

    bool firstFrame = true;

    // Use this for initialization
    void Start() {
        line = transform.parent.gameObject.GetComponentInChildren<LineRenderer>();
        Debug.Assert(line != null, "PageMesh needs to find a LineRenderer to fit");
        mFilter = GetComponent<MeshFilter>();
        mRenderer = GetComponent<MeshRenderer>();
    }

    void InitMesh()
    {
        // Initialize mesh
        Mesh mesh = new Mesh();

        // Create vertices along line points; connect with tris
        var verts = new List<Vector3>();
        var tris = new List<int>();
        for (int i = 0; i < line.numPositions; i++)
        {
            Vector3 p = line.GetPosition(i);
            Vector3 v0 = new Vector3(p.x, p.y, pageHeight / 2);
            verts.Add(v0);
            Vector3 v1 = new Vector3(p.x, p.y, -pageHeight / 2);
            verts.Add(v1);
        }

        for (int i = 0; i < line.numPositions - 1; i++)
        {
            int v0 = 2 * i;
            int v1 = 2 * i + 1;
            int v2 = 2 * i + 2;
            int v3 = 2 * i + 3;

            if(front)
            {
                // front tris
                tris.AddRange(new int[] { v0, v1, v2 });
                tris.AddRange(new int[] { v1, v3, v2 });
            }
            else
            {
                // back tris
                tris.AddRange(new int[] { v0, v2, v1 });
                tris.AddRange(new int[] { v1, v2, v3 });
            }
        }

        mesh.vertices = verts.ToArray();
        mesh.triangles = tris.ToArray();

        mFilter.mesh = mesh;
        mRenderer.enabled = true;
    }

    void UpdateMesh()
    {
        // Change vertices to match line
        var verts = mFilter.mesh.vertices;
        for (int i = 0; i < line.numPositions; i++)
        {
            var p = line.GetPosition(i);
            verts[2 * i].Set(p.x, p.y, -pageHeight / 2);
            verts[2 * i + 1].Set(p.x, p.y, pageHeight / 2);
        }
        mFilter.mesh.vertices = verts;
    }

	
	// Update is called once per frame
	void Update () {
		
        if(firstFrame)
        {
            InitMesh();
            firstFrame = false;
        }

        UpdateMesh();
        mFilter.mesh.RecalculateNormals();
    }
}
