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
    
    [SerializeField, Tooltip("Movement speed of the UAV")]       private float movementSpeed = 10.0f;
    [SerializeField, Tooltip("Coverage radius of the UAV")]      private float coverageRadius;

    // References to other scripts
    [SerializeField, Tooltip("ObjectiveSpawner of the TrainingSpace")] private ObjectiveSpawner objectiveSpawnerRef;
    
    
    // Training related fields
    [SerializeField, Tooltip("Current Episode step coverage accumulator")] private float stepCoverageAcc; 
    [SerializeField, Tooltip("Current Episode movement magnidude accumulator")] private float stepMovementAcc;
    [SerializeField, Tooltip("Alpha value, it specifies the importance of the movement and the coverage during the training phase")] private float alpha = 0.85f;
    
    
    
    
    //[SerializeField, Tooltip("Magnitude Threshold, acts like a low pass filter.")] private float threshold = 0.2f;
    public void Start() 
    {
        coverageRadius = gameObject.transform.Find("Coverage").gameObject.transform.localScale.x / 2.0f;
    }
    
    // On each episode, reset stepCoverageAcc, stepMovementAcc and generate new positions for users    
    public override void OnEpisodeBegin() 
    {
        objectiveSpawnerRef.move();
        stepCoverageAcc = 0.0f; 
        stepMovementAcc = 0.0f;
    }
    
    // The observations are the positions of the agents and targets
    public override void CollectObservations(VectorSensor sensor) 
    {
        
        Vector2 uavPosition = normalizePosition(new Vector2(gameObject.transform.position.x, gameObject.transform.position.z));        
        sensor.AddObservation(uavPosition);
        
        Vector2 entityPos;

        foreach ((GameObject entityGameObjRef, float weight) entity in objectiveSpawnerRef.entities)
        {
            entityPos = normalizePosition(new Vector2(entity.entityGameObjRef.transform.position.x, entity.entityGameObjRef.transform.position.z));
            sensor.AddObservation(entityPos);
            sensor.AddObservation(entity.weight);
        }        

    }
    
    
    // The actions specifies the movement of the UAV
    // After the movement is carried out, the boundaries must be
    // checked and the coverage has to be calculated.    
    public override void OnActionReceived(ActionBuffers actions) 
    {
        //Move the drone
        // First, calc the magnitude of the movement
        float magnitude = (actions.ContinuousActions[2] + 1.0f) / (2.0f);
        
        //Apply threshold to magnitude 
        //magnitude = magnitude < threshold? 0.0f : magnitude;
        
        Vector3 direction = Vector3.Normalize(new Vector3(actions.ContinuousActions[0], 0 , actions.ContinuousActions[1]));
        Vector3 movement = direction * magnitude * Time.deltaTime * movementSpeed;
        
        gameObject.transform.position += movement;
        
        
        //Check if it is outside the boundaries of the map
        if (IsDroneOutsideBoundaries() == true) 
        {
            float coverage           = stepCoverageAcc / MaxStep;
            float distance           = stepMovementAcc / StepCount;
            float outOfBoundsPenalty = -1.0f;// * ( 1.0f - ((float)StepCount / (float)MaxStep)); 
            float reward             = outOfBoundsPenalty + (alpha * coverage) + ((1-alpha) * (1 - distance));
            Debug.LogWarning($"(Ep: {CompletedEpisodes}) UAV has exited the Training Area (step: {StepCount}), setting negative reward {reward} based on: \n\t outOfBoundsPenalty: {outOfBoundsPenalty} \tcoverage: {coverage} \tdistance: {distance} ");
            SetReward(reward);
            EndEpisode();
            gameObject.transform.position = new Vector3(0.0f, 0.0f, 0.0f);
            return;
        }

        float currentStepCoverage   = getCoverage() / objectiveSpawnerRef.weightsSum;
        stepCoverageAcc += currentStepCoverage; 
        stepMovementAcc += magnitude;
        
        if (currentStepCoverage > 0.0f)
        {
            AddReward(currentStepCoverage/(float)MaxStep);
        }
        else
        {
            AddReward(-1.0f/(float)MaxStep);
        }
        
        if(StepCount % 500 == 0) Debug.Log($"(Ep: {CompletedEpisodes}, Step: {StepCount}) Relative Accumulated Coverage = {stepCoverageAcc / StepCount}%  \t\t  Relative Cumulative Travel Distance =  {stepMovementAcc / (StepCount)}% ");  
                
        //Final Step reward
        if(StepCount == MaxStep)
        {
            float coverage = stepCoverageAcc / MaxStep;
            float distance = stepMovementAcc / MaxStep;
            float reward = (alpha * coverage) + ((1-alpha) * (1 - distance));
            SetReward(reward);
            Debug.Log($"Episode {CompletedEpisodes} Completed, REWARD -> {reward}");
        }

    }
          
    private bool IsDroneOutsideBoundaries()
    {
        if  (gameObject.transform.position.x > objectiveSpawnerRef.maxX + 10.0f ||
             gameObject.transform.position.x < objectiveSpawnerRef.minX - 10.0f ||
             gameObject.transform.position.z > objectiveSpawnerRef.maxZ + 10.0f ||
             gameObject.transform.position.z < objectiveSpawnerRef.minZ - 10.0f)
        {
            return true;
        } 
        else
        {
            return false;
            
        }
    }
    
    private float getCoverage()
    {
        List<(GameObject entityGameObjRef, float weight)> targets = objectiveSpawnerRef.entities;
        Vector2 uavPosition   = new Vector2(gameObject.transform.position.x, gameObject.transform.position.z);
        Vector2 targetPosition;
        float distance;
        float acc = 0.0f;

        foreach ( (GameObject entityGameObjRef, float weight) target in targets)
        {
            targetPosition = new Vector2(target.entityGameObjRef.transform.position.x, target.entityGameObjRef.transform.position.z);
            
            distance = Vector2.Distance(uavPosition, targetPosition);
            if( distance < coverageRadius) 
            {
                acc += target.weight;
            }
        }
        
        return acc;
    }
    
    
    private Vector2 normalizePosition(Vector2 pos)
    {       
        float normalizedX = ((pos.x - objectiveSpawnerRef.minX) / (objectiveSpawnerRef.maxX - objectiveSpawnerRef.minX));
        float normalizedZ = ((pos.y - objectiveSpawnerRef.minZ) / (objectiveSpawnerRef.maxZ - objectiveSpawnerRef.minZ));
        
        return  new Vector2(normalizedX, normalizedZ);
    }
}
