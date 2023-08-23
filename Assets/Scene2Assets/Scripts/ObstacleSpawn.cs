using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Scenario))]
public class ObstacleSpawn : MonoBehaviour
{

    private Scenario scenario;
    
    
    [SerializeField] GameObject obstaclePrefab;
    [SerializeField] int numberObstacles;
    private float _padding = 150.0f;
    public float padding 
    {
        get
        {
            return _padding;
        }
        set
        {
            _padding = value;
        }
    }
    


    private List<GameObject> _obstaclesList = new List<GameObject>();

    public List<GameObject> obstaclesList 
    {
        get
        {
            return new List<GameObject>(_obstaclesList);
        }
    }
    
    /// <summary>
    /// Method <c>Start</c> is called before the first frame update. It initializes variables and spawns obstacles.
    /// </summary>
    void Start()
    {
        scenario = GetComponent<Scenario>();
        spawnObstacles();
    }

    /// <summary>
    /// Method <c>spawnObstacles</c> instantiates n gameObjects from obstaclePrefab inside a random sized area.
    /// </summary>
    private void spawnObstacles() 
    {
        int i, j;
        Vector3 obstaclePos;
        

        GameObject obstacleAux;
        for (i = 0; i < numberObstacles; i++)
        {
            obstaclePos = new Vector3( Random.Range(scenario.minX + _padding + 20.0f, scenario.maxX - _padding - 20.0f), 
                                       10.0f,
                                       Random.Range(scenario.minZ + _padding + 20.0f, scenario.maxZ - _padding - 20.0f));        
            
                
            obstacleAux = Instantiate(obstaclePrefab, obstaclePos, Quaternion.identity);
            obstacleAux.transform.parent = gameObject.transform;
            _obstaclesList.Add(obstacleAux);
        }
    }


    /// <summary>
    /// Method <c>spawnObstacles</c> generate new random positions inside a random sized area for all obstacles in <c>obstacleList</c>.
    /// </summary>    
    public void moveObstacles()
    {
        int i, j;
        

        foreach (GameObject obstacle in _obstaclesList)
        {
            obstacle.transform.position = new Vector3 (Random.Range(scenario.minX + _padding + 20.0f, scenario.maxX - _padding - 20.0f), 
                                                       10.0f,
                                                       Random.Range(scenario.minZ + _padding + 20.0f, scenario.maxZ - _padding - 20.0f));
        }
    }
    
}

