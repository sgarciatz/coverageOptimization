using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//ML-Agents imports
using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;

// Write to file
using System.IO;


[RequireComponent(typeof(Scenario))]
[RequireComponent(typeof(PeopleSpawn))]
[RequireComponent(typeof(ObstacleSpawn))]
public class ControllerAgent : Agent
{
    [SerializeField] float  maxSpeed = 16.67f;
    private float           maxXZDistance;
    private float           episodeCoverageAcc;
    [SerializeField] float  alpha = 0.85f;
    private float           episodeMovementAcc;
    private Scenario        scenario;
    private PeopleSpawn     peopleSpawn;
    private ObstacleSpawn   obstacleSpawn;
    
    private List<LineOfSightManager> uavList;
    
    private List<string> data = new List<string>();
    // Start is called before the first frame update
    void Start()
    {
        episodeCoverageAcc  = 0.0f;
        episodeMovementAcc  = 0.0f;        
        scenario            = GetComponent<Scenario>();
        peopleSpawn         = GetComponent<PeopleSpawn>();
        obstacleSpawn       = GetComponent<ObstacleSpawn>();
        uavList             = new List<LineOfSightManager>(GetComponentsInChildren<LineOfSightManager>());
        maxXZDistance       = uavList[0].uavCoverageRef.transform.localScale.x/2;
        
    }


    public override void OnEpisodeBegin() 
    {

        episodeCoverageAcc = 0.0f;
        episodeMovementAcc = 0.0f;
        peopleSpawn.movePeople();
        obstacleSpawn.moveObstacles();
        assignUsersToUAVs();
    }
    
    
    public override void CollectObservations(VectorSensor sensor) 
    {
        Vector3 auxPos;
        // Add the coordenates of all UAVs, ignore y component
        foreach (LineOfSightManager uav in uavList)
        {
            auxPos = scenario.normalizeVector(uav.uavBodyRef.transform.position);
            
            sensor.AddObservation(new Vector2(auxPos.x, auxPos.z));
        }
        //Add the coordenates of all users, ignore y component 
        foreach (GameObject person in peopleSpawn.usersList)
        {
            auxPos = scenario.normalizeVector(person.transform.position);
            sensor.AddObservation(new Vector2(auxPos.x, auxPos.z));
        }
    }
     
     
    public override void OnActionReceived(ActionBuffers actions) 
    {   
        //Apply actions
        List<Vector3> uavMovements = new List<Vector3>();
        Vector3 direction, movement;
        int i;
        float stepReward;
        float magnitude;
        float stepMovement = 0.0f;
        for (i = 0; i < uavList.Count; i++)
        {
            direction = Vector3.Normalize(new Vector3 (actions.ContinuousActions[i*3], 0, actions.ContinuousActions[i*3+1]));
            magnitude = (actions.ContinuousActions[i*3+2] + 1.0f) / 2.0f;
            stepMovement += magnitude;
            movement = direction * magnitude * Time.deltaTime * maxSpeed;
            uavList[i].uavBodyRef.transform.parent.position += movement;
        }
        stepMovement = stepMovement / uavList.Count;
        episodeMovementAcc += stepMovement;
        // Evaluate actions
        if (IsDroneOutsideBoundaries()) // If UAV is outside environment, finish episode and apply penalty
        {
            float outOfBoundsPenalty = -1.0f;
            float distance           = episodeMovementAcc / StepCount;
            float coverage           = episodeCoverageAcc / MaxStep;
            // float episodeReward      = outOfBoundsPenalty + coverage;
            float episodeReward      = outOfBoundsPenalty + alpha * coverage + (1.0f - alpha) * (1.0f - distance);
            SetReward(episodeReward);
            foreach (LineOfSightManager uav in uavList)
            {
                uav.uavBodyRef.transform.parent.position = scenario.getCenterPoint();
            }
            EndEpisode();
            return;
        }

        assignUsersToUAVs();
        float currentStepCoverage = 0.0f;

        foreach (LineOfSightManager uav in uavList)
        {
            currentStepCoverage += uav.getPuntuation();
        }
        // Calc. the ratio between the users covered with respect to the total users.
        currentStepCoverage = currentStepCoverage/peopleSpawn.numberUsers;
        // Accumulate the step rewards to generate the episode reward.
        episodeCoverageAcc += currentStepCoverage;
        //Save data to list 
        //saveData(currentStepCoverage, episodeCoverageAcc, episodeCoverageAcc/StepCount, stepMovement, episodeMovementAcc, episodeMovementAcc/StepCount);        
        
        if (currentStepCoverage > 0.0f)
        {

            stepReward = currentStepCoverage/(float)MaxStep;
            SetReward(stepReward);
        }
        else
        {
            SetReward(-1.0f/(float)MaxStep);
        }

        if (StepCount > 0 && StepCount % 500 == 0 && gameObject.name == "Environment") // Every 500 iterations, show accumulated reward until that moment 
        {
            Debug.Log($"(Ep. {CompletedEpisodes}, Step: {StepCount}) Coverage/Steps: {episodeCoverageAcc / StepCount}");
        }
        
        if (StepCount == MaxStep) // If end of episode...
        {	
            //writeToFile("/home/santiago/Documents/Trabajo/data_001.csv");
            float distance           = episodeMovementAcc / MaxStep;
            float coverage           = episodeCoverageAcc / MaxStep;
            //float episodeReward      = coverage;
            float episodeReward      = alpha * coverage + (1.0f - alpha) * (1.0f - distance);
            SetReward(episodeReward);
            if (gameObject.name == "Environment")
            {
                Debug.Log($"EPISODE {CompletedEpisodes} REWARD(Coverage/Steps) -> {episodeReward}");
            }
        }

    }
    
    public bool IsDroneOutsideBoundaries()
    {
        foreach (LineOfSightManager uav in uavList)
        {
            if (scenario.isVectorOutsideScenario(uav.uavBodyRef.transform.position))
            {
                //Debug.Log(uav.uavBodyRef.transform.position);
                return true;
            }
        }
        return false;
    }

    public void assignUsersToUAVs()
    {
        float bestDistance, currentDistance;
        int bestUAV;
        bool hasLoS, currentHasLoS;
        LineOfSightManager losManager;

        List<List<GameObject>> peopleToUAVMultlist = new List<List<GameObject>>(uavList.Count);
        int i;
        for (i = 0; i < uavList.Count; i++)
        {
            peopleToUAVMultlist.Add(new List<GameObject>());
        }
        

 
 
        foreach (GameObject person in peopleSpawn.usersList)
        {
            bestDistance = 99999.0f;
            hasLoS = false;
            bestUAV = -1;
            i = 0;
            do
            {              
                currentDistance = (new Vector2(person.transform.position.x, person.transform.position.z) - new Vector2(uavList[i].uavBodyRef.transform.position.x, uavList[i].uavBodyRef.transform.position.z)).magnitude;
                currentHasLoS = false;
                //Debug.Log($" Person: {person.transform.position}, UAV: {uavList[i].uavBodyRef.transform.position}, Distance: {currentDistance}");
                if (currentDistance <= maxXZDistance)
                {
                    
                     if (!Physics.Linecast(person.transform.position, uavList[i].uavBodyRef.transform.position, LayerMask.GetMask("Obstacle")))
                     {
                        currentHasLoS = true;
                     }
                     
                     if (bestUAV == -1)
                     {
                        bestUAV = i;
                        hasLoS = true;
                        bestDistance = currentDistance;
                     }
                     else
                     {
                        if (!hasLoS && currentHasLoS)
                        {
                            bestUAV = i;
                            hasLoS = true;
                            bestDistance = currentDistance;
                        }
                        else
                        {
                            if ( currentHasLoS && bestDistance > currentDistance)
                            {
                                bestUAV = i;
                                hasLoS = true;
                                bestDistance = currentDistance;
                            }
                        }
                     }
                }
                
                i++;
            } while (i < uavList.Count);
            
            if (bestUAV > -1)
            {
                peopleToUAVMultlist[bestUAV].Add(person);
            }
            
        }
        
        for (i = 0; i < uavList.Count; i++)
        {
            uavList[i].unloadUsers();
            uavList[i].loadUsers(peopleToUAVMultlist[i]);
        }
    }
    
    public void saveData(float coverage, float coverageAcc, float averagedCoverageAcc, float movement, float movementAcc, float averagedMovementAcc)
    {
        data.Add($"{coverage},{coverageAcc},{averagedCoverageAcc},{movement},{movementAcc},{averagedMovementAcc}");


    }
    public void writeToFile(string filePath)
    {
        StreamWriter writer = new StreamWriter(filePath);
        foreach (var row in data)
        {
            writer.WriteLine(row);
        }
    }
}
