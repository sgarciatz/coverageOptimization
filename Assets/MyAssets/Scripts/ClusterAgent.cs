using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//ML-Agents imports
using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;

public class ClusterAgent : Agent
{
    [Tooltip("The speed of the UAV")] public float movementSpeed = 200f;

    [Tooltip("Cluster's GameObject reference")] public GameObject cluster;
    
    [Tooltip("The list with the UAVs of the cluster")] public GameObject[] uavList;
    
    [Tooltip("Terrain's reference")] public TerrainHeatmap terrainRef;
    
    
    [Tooltip("Radius of the coverage area")] public float coverageRadius = 25.0f;
    
    [Tooltip("Upper boundary ")] public float maxX= 40; 
    [Tooltip("Lower boundary ")] public float minX= -40; 
    
    [Tooltip("Left boundary ")] public float maxZ= 40; 
    [Tooltip("Right boundary ")] public float minZ= -40; 

    // Start is called before the first frame update
    void Start()
    {
        cluster = gameObject;
        // Initialize uavList
        uavList = new GameObject[3];
        uavList[0] = cluster.transform.Find("UAV_0").gameObject;
        uavList[1] = cluster.transform.Find("UAV_1").gameObject;
        uavList[2] = cluster.transform.Find("UAV_2").gameObject;
        resizeCoverageArea();
                         
        uavList[0].transform.position = new Vector3(Random.Range(maxX, minX), 20,  Random.Range(maxZ, minZ));
        uavList[1].transform.position = new Vector3(Random.Range(maxX, minX), 20,  Random.Range(maxZ, minZ));
        uavList[2].transform.position = new Vector3(Random.Range(maxX, minX), 20,  Random.Range(maxZ, minZ));
    
        //Get the reference to the terrain list
        terrainRef = GameObject.Find("Terrain").GetComponent<TerrainHeatmap>();
    }

    // Update is called once per frame
    void Update()
    {
        //Vector3 randomDirection = new Vector3(Random.Range(-1.0f, 1.0f), 0, Random.Range(-1.0f, 1.0f));
        //uavList[0].transform.position += randomDirection * Time.deltaTime * movementSpeed;
        
        //randomDirection = new Vector3(Random.Range(-1.0f, 1.0f), 0, Random.Range(-1.0f, 1.0f));
        //uavList[1].transform.position += randomDirection * Time.deltaTime * movementSpeed;

        //randomDirection = new Vector3(Random.Range(-1.0f, 1.0f), 0, Random.Range(-1.0f, 1.0f));
        //uavList[2].transform.position += randomDirection * Time.deltaTime * movementSpeed;           
    }
    
    private void resizeCoverageArea() {
        Vector3 scale = new Vector3(coverageRadius, 20.0f, coverageRadius);
        Vector3 position = new Vector3(-(coverageRadius/2.0f), -20.0f, -(coverageRadius/2.0f));
        
        uavList[0].transform.Find("Coverage").gameObject.transform.localPosition = position;
        uavList[0].transform.Find("Coverage").gameObject.transform.localScale = scale;       

        uavList[1].transform.Find("Coverage").gameObject.transform.localPosition = position;
        uavList[1].transform.Find("Coverage").gameObject.transform.localScale = scale;     
        
        uavList[2].transform.Find("Coverage").gameObject.transform.localPosition = position;
        uavList[2].transform.Find("Coverage").gameObject.transform.localScale = scale;          
    }
    
    
        /// <summary>
    /// Collect observations for the neural network
    /// </summary>
    /// <param name="sensor"></param>
    public override void CollectObservations(VectorSensor sensor) 
    {
        // UAVs positions normalized to scenario dimensions (Y component ignored) 6
        Vector2 uavPosition;
        float scenarioDimX = maxX - minX;
        float scenarioDimZ = maxZ - minZ;
        foreach (GameObject uav in uavList)
        {
            //Apply Min-Max Normalization 
            
            uavPosition = new Vector2(  ((uav.transform.position.x - minX) / (maxX - minX)) / scenarioDimX, 
                                        ((uav.transform.position.z - minZ) / (maxZ - minZ)) / scenarioDimZ);
            sensor.AddObservation(uavPosition);
        }
        
        // Positions and values of HeatMaps 8*8*3 = 192
        GameObject[,] cells = terrainRef.cellRefMatrix;
        int[,] heatmap = terrainRef.heatmap;
        
        int rows = cells.GetLength(0);
        int columns = cells.GetLength(1);
        int i,j;
        Vector2 cellPosition;
        float heatmapValue;
        for (i = 0; i < rows; i++)
        {
            for (j = 0; j < columns; j++)
            {
                //Apply Min-Max Normalization
                //cellPosition = new Vector2(((cells[i,j].transform.position.x - minX) / (maxX - minX)) / scenarioDimX,
                //                           ((cells[i,j].transform.position.z - minZ) / (maxZ - minZ)) / scenarioDimZ);
                //sensor.AddObservation(cellPosition); // Add positions
                
                heatmapValue = heatmap[i,j] / 5.0f;
                sensor.AddObservation(heatmapValue); // Add values
            }
        }
        
                
        // Current coverage percentage given the position of the drones and the cells 1
        float coveragePercentage = getCoveragePercentage();

        sensor.AddObservation(coveragePercentage);
           
    }
    
    private float getCoveragePercentage()
    {
        GameObject[,] cellsCoords = terrainRef.cellRefMatrix;
        
        int[,] heatmap = terrainRef.heatmap;
        
        int heatmapSum = terrainRef.getSumHeatmap();
        int heatmapAcc = 0;
        
        
        int nUAV = uavList.Length;
        int rows = cellsCoords.GetLength(0);
        int columns = cellsCoords.GetLength(1);
        
        Vector2 uav0Pos = new Vector2(uavList[0].transform.position.x, uavList[0].transform.position.z); 
        Vector2 uav1Pos = new Vector2(uavList[1].transform.position.x, uavList[1].transform.position.z); 
        Vector2 uav2Pos = new Vector2(uavList[2].transform.position.x, uavList[2].transform.position.z); 
        
        int i,j;
        float distanceToUAV0, distanceToUAV1, distanceToUAV2;
        Vector2 auxCellCoords;
        
        for (i = 0; i < rows; i++)
        {
            for (j = 0; j < columns; j++)
            {
                // If the distance between an UAV and a cell is lower than x, then add h to heatmapAcc
                
                auxCellCoords = new Vector2(cellsCoords[i,j].transform.position.x, cellsCoords[i,j].transform.position.z);
                
                distanceToUAV0 = Vector2.Distance(auxCellCoords, uav0Pos); 
                distanceToUAV1 = Vector2.Distance(auxCellCoords, uav1Pos); 
                distanceToUAV2 = Vector2.Distance(auxCellCoords, uav2Pos);

                if (distanceToUAV0 <= 12.0f || distanceToUAV1 <= 12.0f || distanceToUAV2 <= 12.0f)
                {
                    heatmapAcc += heatmap[i,j];
                }
            }
        }
        
        if (StepCount % 1000 == 0) 
        {
            Debug.Log($"Coverage of step {StepCount}: {heatmapAcc} de {heatmapSum} ({((float)heatmapAcc)/heatmapSum}%)");
        }
        return ((float) heatmapAcc) / heatmapSum;  
    }
    
    
    
    
    public override void OnActionReceived(ActionBuffers actions) 
    {
        float scale = Time.deltaTime * movementSpeed;
        Debug.Log($"Coords UAV1: ({actions[0]},{actions[1]}), Coords UAV2: ({actions[2]},{actions[3]}), Coords UAV3: ({actions[4]},{actions[5]})");
        Vector3 uav0Movement = new Vector3(actions.ContinuousActions[0], 0,  actions.ContinuousActions[1]); 
        uavList[0].transform.position += new Vector3(uav0Movement.x, 0, uav0Movement.z) * scale;
        
        Vector3 uav1Movement = new Vector3(actions.ContinuousActions[2], 0, actions.ContinuousActions[3]); 
        uavList[1].transform.position += new Vector3(uav1Movement.x, 0, uav1Movement.z) * scale;
        
        Vector3 uav2Movement = new Vector3(actions.ContinuousActions[4], 0, actions.ContinuousActions[5]); 
        uavList[2].transform.position += new Vector3(uav2Movement.x, 0, uav2Movement.z) * scale;        
    }
    
    
    /// <summary>
    /// Reset the position of the UAVs and generate new heatmap.
    /// </summary>
    public override void OnEpisodeBegin() 
    {
        // Generate new Heatmap
        terrainRef.generateNewHeatmap();
        
        
        // Reset position drone 0
        uavList[0].transform.position = new Vector3(Random.Range(maxX-10.0f, minX-10.0f), 20,  Random.Range(maxZ-10.0f, minZ-10.0f));
        
        // Reset position drone 1
        do
        {
        uavList[1].transform.position = new Vector3(Random.Range(maxX-10.0f, minX-10.0f), 20,  Random.Range(maxZ-10.0f, minZ-10.0f));
        } while (Vector3.Distance(uavList[0].transform.position, uavList[1].transform.position) < 5.0f);
        
        // Reset position drone 2
        do
        {
        uavList[2].transform.position = new Vector3(Random.Range(maxX-10.0f, minX-10.0f), 20,  Random.Range(maxZ-10.0f, minZ-10.0f));
        } while (Vector3.Distance(uavList[0].transform.position, uavList[2].transform.position) < 5.0f || 
                 Vector3.Distance(uavList[1].transform.position, uavList[2].transform.position) < 5.0f);
    }
 
 
    public void FixedUpdate ()
    {

        if (StepCount == MaxStep-1)
        {
            float coverage = getCoveragePercentage();
            coverage = Mathf.Clamp(coverage * 4.0f, 0.0f, 1.0f);
            
            Debug.Log($"F(x) of Episode {CompletedEpisodes}: {coverage * 100.0f}%");
            SetReward(coverage);
            EndEpisode();
            return;
        } 
        
        Vector2 uavPosAux; 
        foreach (GameObject uav in uavList) 
        {
            uavPosAux = new Vector2(uav.transform.position.x, uav.transform.position.z);
            if (uavPosAux.x > maxX || uavPosAux.x < minX || uavPosAux.y > maxZ || uavPosAux.y < minZ) 
            {
                SetReward(-1.0f);
                EndEpisode();
            }
        }
    }   
}

