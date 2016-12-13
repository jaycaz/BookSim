using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MathNet.Numerics.LinearAlgebra;

public class LinePageSim : MonoBehaviour {

    LineRenderer line;
    int N; // num vertices
    int M; // num edges
    List<Tuple<Vector3, Vector3>> edges;
    Matrix<float> X;
    Matrix<float> e1;
    Matrix<float> e2;
    Vector<float> edgeLen;

    // Use this for initialization
    void Start()
    {

        line = GetComponent<LineRenderer>();
        Debug.Assert(line != null, "Line Sim needs a LineRenderer to act on");

        N = line.numPositions;

        // Initialize all edges and edge constraints
        for (int i = 0; i < N - 1; i++)
        {
            var newEdge = new Tuple<Vector3, Vector3>(line.GetPosition(i), line.GetPosition(i + 1));
            edges.Add(newEdge);
        }
        M = edges.Count;
        var pos = new Vector3[N];
        int s = line.GetPositions(pos);

        // Create matrices out of edges for faster computation
        X = CreateMatrix.Dense<float>(N, 3);
        for (int i = 0; i < N; i++)
        {
            var p = line.GetPosition(i);
            X.SetRow(i, new float[] { p.x, p.y, p.z });
        }

        //e1 = CreateMatrix.Dense<float>(M, 3);
        //e2 = CreateMatrix.Dense<float>(M, 3);
        //for (int i = 0; i < M; i++)
        //{
        //    e1[i, 0] = edges[i].Item1.x;
        //    e1[i, 1] = edges[i].Item1.y;
        //    e1[i, 2] = edges[i].Item1.z;

        //    e2[i, 0] = edges[i].Item2.x;
        //    e2[i, 1] = edges[i].Item2.y;
        //    e2[i, 2] = edges[i].Item2.z;
        //}
        //var edgeLen = e2 - e1;
    }

    // Update is called once per frame
    void Update()
    {

        // Enforce developability by calculating
        // generalized cylinder to fit points to
        // [Schreck et al 2015]

        var positions = new Vector3[N];
        int s = line.GetPositions(positions);
        // Just set same positions for now

        line.SetPositions(positions);

        Debug.Log("LinePageSim Update");
    }

    //// Enforce edge lengths via constrained optimization with LMs
    //// [Nocedal & Wright 2000]

    // Enforce edge lengths via position based dynamics
    // [Muller, Kim, & Chentanez 2012]
    void EdgeLengthStep(int iters)
    {
        // Initial conditions
            
    }

}
