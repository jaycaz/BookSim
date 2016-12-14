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
    public float edgePadding = 0.01f;
    public float extent = 2.0f;

    public bool move = true;

    // Radial heightmaps calculated to prevent inter-page collision
    public Tuple<float, float>[] polar { get; private set; }
    GameObject anchor;
    Vector3 rotOrigin;
    float baseAngle;
    public LinePageSim nextPage;
    public LinePageSim prevPage;

    bool isGrabbing;
    public PageCollection pages;
    int grabVertex;
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
            var p = Vector3.Lerp(line.GetPosition(0), line.GetPosition(line.numPositions-1), (float) (i+1) / N);
            var r = p.magnitude;
            var t = Mathf.Atan2(p.x, -p.y);
            if (p.x < 0) t += 2.0f * Mathf.PI;

            t += baseAngle;
            pos[i] = new Vector3(r * Mathf.Sin(t), r * -Mathf.Cos(t), p.z);
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

        UpdatePolarCoords();

        rotOrigin = line.GetPosition(0);
        Debug.LogFormat("Origin: {0}", rotOrigin);
        Debug.LogFormat("Base Angle: {0}", baseAngle * Mathf.Rad2Deg);
    }

    void UpdatePolarCoords()
    {
        // Update polar coordinates for boundary calculation
        for(int i = 0; i < N; i++)
        {
            Vector3 p = ppos[i];
            float r = p.magnitude;
            float theta = Mathf.Atan2(p.x, -p.y);
            if (p.x < 0)
                theta += 2.0f * Mathf.PI;

            polar[i] = new Tuple<float, float>(r, theta);
        }
    }

    void BoundsCheck()
    {
        var floor = GetFloorRegion();
        var ceil = GetCeilRegion();

        // Check all points and push them back within bounds, if necessary
        for(int i = 0; i < N; i++)
        {
            float r = polar[i].Item1;
            float t = polar[i].Item2;
            float ft = GetBoundAngle(polar[i], floor, false);
            float ct = GetBoundAngle(polar[i], ceil, true);

            // Positive = within bounds
            // If both bounds are < 0, choose the closest bound
            float dft = t - ft;
            float dct = ct - t;

            float targetAngle;
            if (dft >= 0 && dct >= 0) continue;
            else if (dft < 0 && dct < 0)
            {
                if (dft < dct)
                    targetAngle = ct;
                else
                    targetAngle = ft;
            }
            else if (dft < 0)
            {
                targetAngle = ft;
            }
            else
            {
                targetAngle = ct;
            }

            //// Floor only
            //if (dft >= 0) continue;
            //targetAngle = ft;

            // Displace point along theta
            float dx = r * Mathf.Sin(targetAngle) - ppos[i].x;
            float dy = r * -Mathf.Cos(targetAngle) - ppos[i].y;
            //pos[i].x += dx;
            //pos[i].y += dy;
            ppos[i].x += dx;
            ppos[i].y += dy;
            //vel[i] = new Vector3();
        }

    }

    List<Tuple<float, float>> GetFloorRegion()
    {
        var region = new List<Tuple<float, float>>();
        region.Add(new Tuple<float, float>(0.0f, Mathf.PI / 2 + edgePadding));
        if(nextPage)
        {
            var p = nextPage.polar;
            float maxR = -1.0f;
            for(int i = 0; i < p.Length; i++)
            {
                float r = p[i].Item1;
                float t = p[i].Item2;

                if(r > maxR)
                {
                    maxR = r;
                    region.Add(new Tuple<float, float>(r, t + edgePadding));
                }
            }
        }
        region.Add(new Tuple<float, float>(extent, Mathf.PI / 2 + edgePadding));
        return region;
    }

    List<Tuple<float, float>> GetCeilRegion()
    {
        var region = new List<Tuple<float, float>>();
        region.Add(new Tuple<float, float>(0.0f, 3 * Mathf.PI / 2 - edgePadding));
        if(prevPage)
        {
            var p = prevPage.polar;
            float maxR = -1.0f;
            for(int i = 0; i < p.Length; i++)
            {
                float r = p[i].Item1;
                float t = p[i].Item2;

                if(r > maxR)
                {
                    maxR = r;
                    region.Add(new Tuple<float, float>(r, t - edgePadding));
                }
            }
        }
        region.Add(new Tuple<float, float>(extent, 3 * Mathf.PI / 2 - edgePadding));

        return region;
    }

    float GetBoundAngle(Tuple<float, float> p, List<Tuple<float, float>> region, bool ceil)
    {
        float r = p.Item1;
        float t = p.Item2;

        for(int j = 0; j < region.Count-1; j++)
        {
            // Find portion of region that this point falls between
            float r0 = region[j].Item1;
            float r1 = region[j+1].Item1;
            if(r >= r0 && r <= r1)
            {
                float t01 = Mathf.Lerp(region[j].Item2,
                                        region[j + 1].Item2,
                                        (r1 - r) / (r1 - r0));

                return t01;
            }
        }

        if(ceil)
        {
            return Mathf.Infinity;
        }
        else
        {
            return -Mathf.Infinity;
        }
    }

    // Update is called once per frame
    //void FixedUpdate()

    public void Tick()
    {
        if(!move)
        {
            UpdatePolarCoords();
            return;
        }

        float dt = Time.fixedDeltaTime * timeScale;
        default_inv_mass = new float[N];
        inv_mass = new float[N];
        for(int i = 0; i < N; i++)
        {
            default_inv_mass[i] = 1.0f / (totalMass / N);
        }

        int s = line.GetPositions(pos);
        ppos = pos;

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
        if(pages.grabLine == line)
        {
            inv_mass[pages.grabVertex] = 0.0001f;
        }

        // Apply velocity change from user input, if any
        foreach(var a in anchoredVertices)
        {
            inv_mass[a] = 0.0f;
            vel[a] = new Vector3();
        }

        if(pages.grabLine == line)
        {
            vel[pages.grabVertex] = pages.grabVel;
        }

        for(int i = 0; i < N; i++)
        {
            ppos[i] = pos[i] + dt * vel[i];
        }

        // Solve constraints
        for (int i = 0; i < constraintSteps; i++)
        {
            UpdatePolarCoords();
            BoundsCheck();
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
