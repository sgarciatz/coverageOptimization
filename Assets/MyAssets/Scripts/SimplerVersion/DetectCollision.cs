using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DetectCollision : MonoBehaviour
{
    [SerializeField, Tooltip("Reference to the agent Script")] AgentUAV agentScript;
    [SerializeField, Tooltip("References to the active collided entities ")] public List<GameObject> collidedEntities;
    
    
    public void Start()
    {      
        agentScript = GetComponentInParent<AgentUAV>(true);
        
        collidedEntities = new List<GameObject>();
        
    }
    public void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            collidedEntities.Add(other.gameObject);
            agentScript.collisionDetected();           
        }

    }
    
    public void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            collidedEntities.Remove(other.gameObject);
            //agentScript.collisionStopped();
        }
    }
}
