using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LineOfSightManager : MonoBehaviour
{
    
    public GameObject uavBodyRef;
    public GameObject uavCoverageRef;
    [SerializeField] private List<GameObject> peopleRefs;

    private float puntuation = 0.0f;
            
    // Start is called before the first frame update
    void Start()
    {

        uavBodyRef = gameObject.transform.Find("UAV_body").gameObject;
        uavCoverageRef = gameObject.transform.Find("UAV_coverage").gameObject;
        peopleRefs = new List<GameObject>();
    }

    public void loadUsers(List<GameObject> usersRefs)
    {
        peopleRefs.AddRange(usersRefs);
    }
    
    public void unloadUsers()
    {
        LineRenderer lineRenderer; 
        foreach (GameObject user in peopleRefs)
        {
            lineRenderer = user.GetComponent<LineRenderer>();
            lineRenderer.enabled = false;
            
        }
        peopleRefs.Clear();
    }
    
    private void castRays ()
    {
        puntuation = 0;
        
        foreach (GameObject user in peopleRefs)
        {
        
            if (Physics.Linecast(gameObject.transform.position, user.transform.position, LayerMask.GetMask("Obstacle")))
            {
                //Debug.DrawLine(uavBodyRef.transform.position, user.transform.position, Color.red);
                puntuation += 1.0f;
            }
            else
            {
                //Debug.DrawLine(uavBodyRef.transform.position, user.transform.position, Color.yellow);
                LineRenderer lineRenderer = user.GetComponent<LineRenderer>(); 
                lineRenderer.enabled = true;
                lineRenderer.SetPosition(0, uavBodyRef.transform.position);
                lineRenderer.SetPosition(1, user.transform.position);
                puntuation += 1.0f;
            }
            
        }
    }

    public float getPuntuation()
    {
        //castRays();
        return (float)peopleRefs.Count;
    }
    
}
