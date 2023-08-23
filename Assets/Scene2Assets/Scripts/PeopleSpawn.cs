using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using System.Linq;
[RequireComponent(typeof(Scenario))]
public class PeopleSpawn : MonoBehaviour
{
    
    private Scenario scenario;

    [SerializeField] private GameObject personPrefab;
    
    [SerializeField] public int numberUsers;
    [SerializeField] public int numberClusters;
    private float _padding = 200.0f;
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
    private float _maxRadius = 100.0f;
    public float maxRadius 
    {
        get
        {
            return _maxRadius;
        }
        set
        {
            _maxRadius = value;
        }
    }
    
    private List<GameObject> _usersList = new List<GameObject>();

    public List<GameObject> usersList 
    {
        get
        {
            return new List<GameObject>(_usersList);
        }
    }
    
    /// <summary>
    /// Method <c>Start</c> is called before the first frame update. It initializes variables and spawns people.
    /// </summary>
    void Start()
    {
        scenario = GetComponent<Scenario>();
        spawnPeople();   

    }
    
    /// <summary>
    /// Method <c>spawnPeople</c> instantiates n gameObjects from <c>personPrefab</c> inside <c>numberClusters</c> random sized areas.
    /// </summary>
    private void spawnPeople() 
    {
        float clusterRadius;
        Vector3 clusterCentroid, personPos;
        int i, j;
        int[] peoplePerCluster = calcPeoplePerCluster(); 
        GameObject auxPerson;
        for (i = 0; i < numberClusters; i++)
        {
            clusterRadius = Random.Range(50.0f, _maxRadius);

            clusterCentroid = new Vector3( Random.Range(scenario.minX + clusterRadius + _padding, scenario.maxX - clusterRadius - _padding), 
                                                   0.0f,
                                                   Random.Range(scenario.minZ + clusterRadius +_padding, scenario.maxZ - clusterRadius - _padding));        
            
            
            for (j = 0; j < peoplePerCluster[i]; j++)
            {
                personPos = new Vector3( Random.Range(clusterCentroid.x - clusterRadius, clusterCentroid.x + clusterRadius),
                                         1.125f,
                                         Random.Range(clusterCentroid.z - clusterRadius, clusterCentroid.z + clusterRadius));
                auxPerson = Instantiate(personPrefab, personPos, Quaternion.identity);
                auxPerson.transform.parent = gameObject.transform;
                _usersList.Add(auxPerson);
            }
        }
    }
    
    
    /// <summary>
    /// Method <c>MovePeople</c> generates a new position for all users.
    /// </summary>    
    public void movePeople() 
    {
        float clusterRadius;
        Vector3 clusterCentroid, personPos;
        int i, j;
        int k = 0;
        int[] peoplePerCluster = calcPeoplePerCluster(); 
        
        for (i = 0; i < numberClusters; i++)
        {
            clusterRadius = Random.Range(50.0f, _maxRadius);

            clusterCentroid = new Vector3( Random.Range(scenario.minX + clusterRadius + _padding, scenario.maxX - clusterRadius - _padding), 
                                                   0.0f,
                                                   Random.Range(scenario.minZ + clusterRadius +_padding, scenario.maxZ - clusterRadius - _padding));          
            
            
            for (j = 0; j < peoplePerCluster[i]; j++)
            {                

                _usersList[k].transform.position = new Vector3( Random.Range(clusterCentroid.x - clusterRadius, clusterCentroid.x + clusterRadius),
                                         1.125f,
                                         Random.Range(clusterCentroid.z - clusterRadius, clusterCentroid.z + clusterRadius));
                                         
                k++;
            }
        }
    }
    
    /// <summary>
    /// Method <c>calcPeoplePerCluster</c> is a helper that randomly distributes <c>numberUsers</c> users into <c>numberClusters</c> .
    /// </summary>      
    private int[] calcPeoplePerCluster()
    {
        int[] peoplePerCluster = new int[numberClusters];
        int i, peopleLeftout;
        float[] weights = new float[numberClusters];
        float weightsSum;
        
        if (numberClusters == 1)
        {
            peoplePerCluster[0]= numberUsers;
            return peoplePerCluster;
        }        
        
        for (i=0; i < numberClusters; i++)
        {
            weights[i] = Random.Range(0.0f, 1.0f);
        }

             
        weightsSum = weights.Sum();
        
        for (i=0; i < numberClusters; i++)
        {
            peoplePerCluster[i] = (int)((weights[i] / weightsSum) * numberUsers);
        }
        
        // Check if all clusters sum up to 100.
        peopleLeftout = numberUsers - peoplePerCluster.Sum();
        if (peopleLeftout > 0)
        {
            peoplePerCluster[Random.Range(0, numberClusters)] += peopleLeftout;
        }
        // Assert that after checks, all culsters sum up to 100.
        Debug.Assert(peoplePerCluster.Sum() == numberUsers, $"The sum of clusters ({peoplePerCluster.Sum()}) is not equal to the total number of users ({numberUsers})!");
        return peoplePerCluster;
    } 

    
}
