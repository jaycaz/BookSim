using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LinePageSim : MonoBehaviour {

    // Simulation parameters
    public int N = 2; // num vertices
    public float totalMass = 0.01f;
    public float stretchStrength = 1.0f;
    public int constraintSteps = 2;
    public int[] anchoredVertices;

    GrabLine grab;
    LineRenderer line;
    float[] default_inv_mass;
    float[] inv_mass;

    Vector3[] pos;
    Vector3[] ppos;
    Vector3[] vel;

    float[] restDist;

    // Use this for initialization
    void Start()
    {
        grab = GetComponent<GrabLine>();
        line = GetComponent<LineRenderer>();
        Debug.Assert(N >= line.numPositions, "N needs to be at least the same as line.numPositions");

        // Get begin and end points from line, and interpolate N points between
        pos = new Vector3[N];
        for(int i = 0; i < N; i++)
        {
            pos[i] = Vector3.Lerp(line.GetPosition(0), line.GetPosition(line.numPositions-1), (float) i / N);
        }
        line.numPositions = N;
        line.SetPositions(pos);

        ppos = new Vector3[N];
        vel = new Vector3[N];

        restDist = new float[N-1];
        for(int i = 0; i < N-1; i++)
        {
            restDist[i] = Vector3.Distance(pos[i + 1], pos[i]);
        }

        default_inv_mass = new float[N];
        inv_mass = new float[N];
        for(int i = 0; i < N; i++)
        {
            default_inv_mass[i] = 1.0f / (totalMass / N);
        }
    }

    // Update is called once per frame
    void Update()
    {
        int s = line.GetPositions(pos);
        ppos = pos;

        // Reset mass
        for(int i = 0; i < N; i++)
        {
            inv_mass[i] = default_inv_mass[i];
        }
        
        // Apply mass changes
        if(grab.isGrabbing)
        {
            inv_mass[grab.grabVertex] = 0.0001f;
        }
        foreach(var a in anchoredVertices)
        {
            inv_mass[a] = 0.0f;
        }

        // Apply external forces
        // (right now there are none)        

        // Apply velocity change from user input, if any
        for(int i = 0; i < N; i++)
        {
            vel[i] = new Vector3();
        }

        if(grab.isGrabbing)
        {
            vel[grab.grabVertex] = grab.grabVel;
        }

        for(int i = 0; i < N; i++)
        {
            ppos[i] = pos[i] + Time.deltaTime * vel[i];
        }

        // Solve constraints
        for(int i = 0; i < constraintSteps; i++)
        {
            SolveConstraints();
        }

        // Final position update
        for (int i = 0; i < N; i++)
        {
            vel[i] = (ppos[i] - pos[i]) / Time.deltaTime;
            pos[i] = ppos[i];
        }
        line.SetPositions(pos);
    }

    public void SolveConstraints()
    {
        // Update stretch constraints
        for(int i = 0; i < N-1; i++)
        {
            Vector3 d = (ppos[i] - ppos[i + 1]);
            float s = (d.magnitude - restDist[i]) / (inv_mass[i] + inv_mass[i+1]);
            Vector3 dp0 = -inv_mass[i] * s * d.normalized;
            Vector3 dp1 = inv_mass[i + 1] * s * d.normalized;

            ppos[i] += stretchStrength * dp0;
            ppos[i+1] += stretchStrength * dp1;
        }
    }
}
