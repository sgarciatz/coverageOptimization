using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//ML-Agents imports
using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;

public class AgentUAV : Agent
{
    
    [SerializeField, Tooltip("Movement speed of the UAV")]       private float movementSpeed = 1000f;
    [SerializeField, Tooltip("Coverage radius of the UAV")]      private float coverageRadius;
    [SerializeField, Tooltip("Reference to The Training Space")] private GameObject trainingSpace;
    [SerializeField, Tooltip("ObjectiveSpawner of the TrainingSpace")] private ObjectiveSpawner objectiveSpawnerRef;
    [SerializeField, Tooltip("DetectCollision script reference")] private DetectCollision detectCollisionRef; 
    
    [SerializeField, Tooltip("Current Episode max coverage")] private float maxCoverage; 
    public void Start()
    {
        coverageRadius = gameObject.transform.Find("Coverage").gameObject.transform.localScale.x / 2.0f;
        
        objectiveSpawnerRef = trainingSpace.GetComponent<ObjectiveSpawner>();
        detectCollisionRef = GetComponentInChildren<DetectCollision>(true);
    }
    
    public override void CollectObservations(VectorSensor sensor) 
    {
        
        Vector2 uavPosition = normalizePosition(new Vector2(gameObject.transform.position.x, gameObject.transform.position.z));        
        sensor.AddObservation(uavPosition);
        
        Vector2 entityPos;
        
        foreach ((GameObject entityGameObjRef, float weight) entity in objectiveSpawnerRef.entities)
        {
            entityPos = normalizePosition(new Vector2(entity.entityGameObjRef.transform.position.x, entity.entityGameObjRef.transform.position.z));
            sensor.AddObservation(entityPos);
        }        

    }
    
    public override void OnActionReceived(ActionBuffers actions) 
    {
        Vector3 movement = new Vector3(actions.ContinuousActions[0], 0 , actions.ContinuousActions[1]);
        gameObject.transform.position += movement ;

        float reward = 0;  
        if (StepCount == MaxStep || calcCoverage() > 0.5f) //If end
        {
            float coverage = calcCoverage();
            
            reward = coverage == 0.0f ? -1.0f : coverage/maxCoverage;
            reward -= StepCount*0.0001f *movement.magnitude;
            SetReward(reward);
            
            if(movement.magnitude == 0f)
            {
                SetReward(1f); 
                EndEpisode();   
            } 

            Debug.Log($"Cov: {calcCoverage()} -- Reward: {reward} -- Step: {StepCount} -- Mov: {movement.magnitude}");
        }
        else
        {
            SetReward(-1f*0.00001f*StepCount);
        }
        //if (movement.magnitude > 0.05f) reward -= 0.1f;

        //SetReward(reward);
    }
    

    
    public override void OnEpisodeBegin() 
    {
        gameObject.transform.position = new Vector3(0.0f, 0.0f, 0.0f);
        objectiveSpawnerRef.move();
        maxCoverage = 1; //calcMaxCoverage();
        detectCollisionRef.collidedEntities.Clear();
    }
    
    
    //Check for events
    public void FixedUpdate()
    {
        if  (gameObject.transform.position.x > objectiveSpawnerRef.maxX ||
             gameObject.transform.position.x < objectiveSpawnerRef.minX ||
             gameObject.transform.position.z > objectiveSpawnerRef.maxZ ||
             gameObject.transform.position.z < objectiveSpawnerRef.minZ)
        {
            Debug.Log($"The UAV has exited the Training Area, adding negative reward (Ep: {CompletedEpisodes})");
            SetReward(-1.0f);
            EndEpisode();
        }
    }
    
    
    //
    public void collisionDetected()
    {

        
        //Calcule reward
        float coverage = calcCoverage();
        
        float reward = coverage/maxCoverage;


        //Debug.Log($"(Ep: {CompletedEpisodes}) A player has entered in the coverage area of the UAV, reward = {reward} ({coverage}/{maxCoverage}");
        
        // if (coverage == maxCoverage)
        // {
        //     SetReward(1.0f);
        //     EndEpisode();
        // }
    }
    
    
    public void collisionStopped()
    {
    

        //Calcule reward
        float coverage = calcCoverage();
        
        float reward = coverage/maxCoverage;
        //SetReward(reward);
        Debug.Log($"(Ep: {CompletedEpisodes}) A player has exited the coverage area of the UAV, reward = {reward} ({coverage}/{maxCoverage} ");
    }
    
    private Vector2 normalizePosition(Vector2 pos)
    {
        float dimX = objectiveSpawnerRef.maxX - objectiveSpawnerRef.minX;
        float dimZ = objectiveSpawnerRef.maxZ - objectiveSpawnerRef.minZ;
        
        float normalizedX = ((pos.x - objectiveSpawnerRef.minX) / (objectiveSpawnerRef.maxX - objectiveSpawnerRef.minX)) / dimX;
        float normalizedZ = ((pos.y - objectiveSpawnerRef.minZ) / (objectiveSpawnerRef.maxZ - objectiveSpawnerRef.minZ)) / dimZ;
        
        return  new Vector2(normalizedX, normalizedZ);
    }


    private float calcMaxCoverage()
    {
        List<(GameObject entityGameObjRef, float weight)> targets = objectiveSpawnerRef.entities;
        Vector2 posTarget1, posTarget2;
        float best = 0.0f;
        float acc;
        foreach ((GameObject entityGameObjRef, float weight) target1 in targets)
        {
            acc = target1.weight;
            foreach ((GameObject entityGameObjRef, float weight) target2 in targets.Except(new List<(GameObject entityGameObjRef, float weight)>{target1}))
            {
                posTarget1 = new Vector2(target1.entityGameObjRef.transform.position.x, target1.entityGameObjRef.transform.position.z);
                posTarget2 = new Vector2(target2.entityGameObjRef.transform.position.x, target2.entityGameObjRef.transform.position.z);
                
                if (Mathf.Abs(Vector2.Distance(posTarget1, posTarget2)) < coverageRadius * 2)
                {
                    acc += target2.weight;
                }
            }
            
            if ( acc > best) 
            {
                best = acc;
            }
        }
        Debug.Log($"Best coverage obtainable: {best}");
        return best;
    }
    
    private float calcCoverage()
    {        
        List<(GameObject entityGameObjRef, float weight)> targets = objectiveSpawnerRef.entities;
        
        int index = -1;
        float acc = 0;
        foreach((GameObject entityGameObjRef, float weight) entity in targets)
        {
            Vector2 personPos = new Vector2(entity.entityGameObjRef.transform.position.x, entity.entityGameObjRef.transform.position.z);
            Vector2 uavPos = new Vector2(gameObject.transform.position.x, gameObject.transform.position.z);
            if(Vector2.Distance(personPos, uavPos) < coverageRadius)
            {
                acc += entity.weight;
            }
        }
        
        return acc;
    }
}
