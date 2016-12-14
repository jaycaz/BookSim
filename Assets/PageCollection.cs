using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PageCollection : MonoBehaviour {

    List<LinePageSim> pages;
    List<LinePageSim> lPages;
    List<LinePageSim> rPages;

    public int initialTicks = 10;

    public bool allPagesActive = false;
    
    public bool isGrabbing { get; private set; }
    public LineRenderer grabLine { get; private set; }
    public int grabVertex { get; private set; }
    public Vector3 grabVel { get; private set; }
    Vector3 lastMousePos;

    // Use this for initialization
    void Start()
    {
        // Add all child pages to list
        pages = new List<LinePageSim>();
        lPages = new List<LinePageSim>();
        rPages = new List<LinePageSim>();

        for (int i = 0; i < transform.GetChildCount(); i++)
        {
            GameObject c = transform.GetChild(i).gameObject;
            LinePageSim page = c.GetComponentInChildren<LinePageSim>();
            if (page)
            {
                page.pages = GetComponent<PageCollection>();
                pages.Add(page);
                rPages.Add(page);
            }

        }

        // Establish page/page connections
        for (int i = 0; i < pages.Count; i++)
        {
            if (i > 0)
            {
                pages[i].prevPage = pages[i - 1];
            }
            if (i < pages.Count - 1)
            {
                pages[i].nextPage = pages[i + 1];
            }
        }

        if (!allPagesActive)
        {
            // Run for a few ticks so pages can settle before locking them
            for (int i = 0; i < initialTicks; i++)
            {
                RunAllPages();
            }

            // Now lock pages
            for (int i = 0; i < pages.Count; i++)
            {
                if (!allPagesActive)
                {
                    pages[i].move = false;
                }
            }

            // Only activate top page
            if (rPages.Count > 0)
            {
                rPages[0].move = true;
            }
        }
    }

    void RunAllPages()
    {
        // Run all simulations from bottom to top (i.e. forwards for L pages, backwards for R pages)
        for (int i = rPages.Count - 1; i >= 0; i--)
        {
            rPages[i].GetComponent<LinePageSim>().Tick();
        }

        for (int i = 0; i < lPages.Count; i++)
        {
            lPages[i].GetComponent<LinePageSim>().Tick();
        }
    }

    // Update is called once per frame
    void Update() {

        RunAllPages();

        // Calculate closest point from all lines to mouse
        float minDist = Mathf.Infinity;
        LineRenderer minLine = null;
        int minVertex = -1;
        Vector3 mouseScreen = Input.mousePosition;
        for(int i = 0; i < pages.Count; i++)
        {
            // Only moving pages can be grabbed
            if(!pages[i].move)
            {
                continue;
            }
            for(int j = 0; j < pages[i].N; j++)
            {
                // Find closest point to mouse
                LineRenderer line = pages[i].GetComponent<LineRenderer>();
                Vector3 vertex = line.transform.TransformPoint(line.GetPosition(j));
                Vector3 vertexScreen = Camera.main.WorldToScreenPoint(vertex);
                float dist = Vector3.SqrMagnitude(vertexScreen - mouseScreen);
                if(dist < minDist)
                {
                    minDist = dist;
                    minLine = line;
                    //minVertex = j;

                    // Snapping to end of page for now
                    minVertex = pages[i].N - 1;
                }
            }
        }
        //Debug.LogFormat("Closest vertex: {0} ({1}m)", minVertex, minDist);
        Debug.DrawRay(Camera.main.transform.position, 
            minLine.transform.TransformPoint(minLine.GetPosition(minVertex)) - Camera.main.transform.position,
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
            grabLine = minLine;
            grabVertex = minVertex;
        }
        if (Input.GetMouseButtonUp(0))
        {
            isGrabbing = false;
            grabLine = null;
            grabVertex = -1;
        }

        Vector3 mouseWorld = Camera.main.ScreenToWorldPoint(mouseScreen);
        if(isGrabbing)
        {
            Vector3 dmouse = mouseWorld - lastMousePos;
            Vector3 pos = grabLine.GetPosition(grabVertex);
            // TODO: May need to change to Time.fixedDeltaTime
            grabVel = dmouse / Time.deltaTime;
            pos += dmouse;
            grabLine.SetPosition(grabVertex, pos);
        }

        lastMousePos = mouseWorld;

        // Moving across y axis switches page status
        if(grabLine)
        {
            var v = grabLine.GetPosition(grabVertex);
            var grabPage = grabLine.GetComponent<LinePageSim>();
            if (v.x < 0 && !lPages.Contains(grabPage))
            {
                if(lPages.Count > 0 && !allPagesActive)
                {
                    lPages[lPages.Count - 1].move = false;
                }
                lPages.Add(grabPage);
                rPages.Remove(grabPage);
                if(rPages.Count > 0 && !allPagesActive)
                {
                    rPages[0].move = true;
                }
            }

            if(v.x > 0 && !rPages.Contains(grabPage))
            {
                if(rPages.Count > 0 && !allPagesActive)
                {
                    rPages[0].move = false;
                }
                rPages.Insert(0, grabPage);
                lPages.Remove(grabPage);
                if(lPages.Count > 0 && !allPagesActive)
                {
                    lPages[lPages.Count - 1].move = true;
                }
            }
        }

        // Recolor pages to label them
        foreach(var p in rPages)
        {
            Color targetColor;
            if(p.move)
            {
                targetColor = Color.red;
            }
            else
            {
                targetColor = Color.grey;
            }

            p.GetComponent<LineRenderer>().startColor = targetColor;
            p.GetComponent<LineRenderer>().endColor = targetColor;
        }

        foreach(var p in lPages)
        {
            Color targetColor;
            if(p.move)
            {
                targetColor = Color.magenta;
            }
            else
            {
                targetColor = Color.grey;
            }

            p.GetComponent<LineRenderer>().startColor = targetColor;
            p.GetComponent<LineRenderer>().endColor = targetColor;
        }


        //if(grabLine)
        //{
        //    grabLine.startColor = Color.green;
        //    grabLine.endColor = Color.green;
        //}
	}
}
