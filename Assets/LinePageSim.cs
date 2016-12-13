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
    public float timeScale = 1.0f;

    // Radial heightmaps calculated to prevent inter-page collision
    public Tuple<float, float>[] polar { get; private set; }
    public GameObject anchor;
    Vector3 rotOrigin;
    float baseAngle;
    LinePageSim nextPage;
    LinePageSim prevPage;
    float extent = 0.0f;

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
        anchor = transform.parent.gameObject;

        Debug.Assert(anchor != null, "Page cannot find anchor object as reference");
        Debug.Assert(N >= line.numPositions, "N needs to be at least the same as line.numPositions");

        // Get begin and end points from line, and interpolate N points between
        // Also retransform so we can get make the transform component the identity
        pos = new Vector3[N];
        baseAngle = Quaternion.Angle(transform.localRotation, Quaternion.identity) * Mathf.Deg2Rad;
        for(int i = 0; i < N; i++)
        {
            var p = Vector3.Lerp(line.GetPosition(0), line.GetPosition(line.numPositions-1), (float) i / N);
            var r = p.magnitude;
            var t = Mathf.Atan2(p.y, p.x);
            if (p.y < 0) t += 2.0f * Mathf.PI;

            t += baseAngle;
            pos[i] = new Vector3(r * Mathf.Cos(t), r * Mathf.Sin(t), p.z);
        }
        transform.localRotation = Quaternion.identity;
        line.numPositions = N;
        line.SetPositions(pos);

        ppos = new Vector3[N];
        vel = new Vector3[N];
        polar = new Tuple<float, float>[N];

        restDist = new float[N-1];
        for(int i = 0; i < N-1; i++)
        {
            restDist[i] = Vector3.Distance(pos[i + 1], pos[i]);
        }

        rotOrigin = line.GetPosition(0);
        Debug.LogFormat("Origin: {0}", rotOrigin);
        Debug.LogFormat("Base Angle: {0}", baseAngle * Mathf.Rad2Deg);
 
        for (int i = 0; i < N; i++)
        {
            var dist = Vector3.Distance(pos[i], rotOrigin);
            if (dist > extent)
            {
                extent = dist;
            }
        }
        Debug.LogFormat("Extent: {0}", extent);
    }

    void FloorRegionCheck()
    {
        var region = new List<Tuple<float, float>>();
        if(prevPage)
        {
            var p = prevPage.polar;
            return;
        }
        else
        {
            region.Add(new Tuple<float, float>(0.0f, 0.0f));
            region.Add(new Tuple<float, float>(extent, 0.0f));
        }

        RegionMove(region, true);
    }

    void CeilRegionCheck()
    {
        var region = new List<Tuple<float, float>>();
        if(prevPage)
        {
            var p = prevPage.polar;
            return;
        }
        else
        {
            region.Add(new Tuple<float, float>(0.0f, Mathf.PI));
            region.Add(new Tuple<float, float>(extent, Mathf.PI));
        }

        RegionMove(region, false);
    }

    void RegionMove(List<Tuple<float, float>> region, bool floor)
    {
        // Move any point if outside of this region
        for(int i = 0; i < N; i++)
        {
            float r = polar[i].Item1;
            float t = polar[i].Item2;

            for(int j = 0; j < region.Count-1; j++)
            {
                // Find portion of region that this point falls between
                float r0 = region[j].Item1;
                float r1 = region[j+1].Item1;
                if(r > r0 && r < r1)
                {
                    //Debug.LogFormat("Checking region ({0}, {1})", r0, r1);
                    float t01 = Mathf.Lerp(region[j].Item2,
                                            region[j + 1].Item2,
                                            (r1 - r) / (r1 - r0));

                    // Check if outside interpolated borderline btwn r0, r1
                    if((!floor && t > t01) || (floor && t < t01))
                    {
                        //Debug.LogFormat("Region {0} contact: {1} => {2}", i, t, t01);
                        // Displace point along theta
                        pos[i].x = r * Mathf.Cos(t01);
                        pos[i].y = r * Mathf.Sin(t01);
                        break;
                    }
                }
            }
        }
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        float dt = Time.fixedDeltaTime * timeScale;
        default_inv_mass = new float[N];
        inv_mass = new float[N];
        for(int i = 0; i < N; i++)
        {
            default_inv_mass[i] = 1.0f / (totalMass / N);
        }

        int s = line.GetPositions(pos);
        ppos = pos;

        // Update polar coords for region calculation
        for(int i = 0; i < N; i++)
        {
            Vector3 p = pos[i];
            float r = p.magnitude;
            float theta = Mathf.Atan2(p.y, p.x);
            if (p.y < 0)
            {
                theta += 2.0f * Mathf.PI;
            }

            polar[i] = new Tuple<float, float>(r, theta);
        }
        //Debug.LogFormat("Polar {0} : <{1}, {2}>", N - 1, polar[N - 1].Item1, polar[N - 2].Item2);

        // Reset mass
        for (int i = 0; i < N; i++)
        {
            inv_mass[i] = default_inv_mass[i];
        }
        
        // Apply external forces
        for(int i = 0; i < N; i++)
        {
            var f = 10.0f * Vector3.down;
            vel[i] += f * dt;
        }

        // Apply mass changes
        if(grab.isGrabbing)
        {
            inv_mass[grab.grabVertex] = 0.0001f;
        }

        // Apply velocity change from user input, if any
        foreach(var a in anchoredVertices)
        {
            inv_mass[a] = 0.0f;
            vel[a] = new Vector3();
        }

        if(grab.isGrabbing)
        {
            vel[grab.grabVertex] = grab.grabVel;
        }

        for(int i = 0; i < N; i++)
        {
            ppos[i] = pos[i] + dt * vel[i];
        }

        FloorRegionCheck();
        CeilRegionCheck();

        // Solve constraints
        for (int i = 0; i < constraintSteps; i++)
        {
            SolveConstraints();
        }

        // Final position update
        for (int i = 0; i < N; i++)
        {
            vel[i] = (ppos[i] - pos[i]) / dt;
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
