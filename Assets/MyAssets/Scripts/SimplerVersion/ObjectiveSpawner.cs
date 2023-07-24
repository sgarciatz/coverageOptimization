using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectiveSpawner : MonoBehaviour
{
    [SerializeField, Tooltip("Entity to spawn")]    GameObject entity1;
    [SerializeField, Tooltip("Entity to spawn")]    GameObject entity2;    
    [SerializeField, Tooltip("Maximun value of x")] public float maxX;
    [SerializeField, Tooltip("Minimun value of x")] public float minX;
    [SerializeField, Tooltip("Maximun value of z")] public float maxZ;
    [SerializeField, Tooltip("Minimun value of z")] public float minZ;            
    
    [SerializeField, Tooltip("Number of entities to spawn")] private int numEntities = 10; 
    [SerializeField, Tooltip("List of spawned entities")]    public List<(GameObject entityGameObjRef, float weight)> entities = new List<(GameObject entityGameObjRef, float weight)>();
    [SerializeField, Tooltip("Sum of the weights of all the entities")] public float weightsSum;
    
    public void Start()
    {
    GameObject floor = gameObject.transform.Find("floor_front_right").gameObject;
    maxX = floor.transform.position.x + 10.0f;
    maxZ = floor.transform.position.z + 10.0f;
    
    
    floor = gameObject.transform.Find("floor_back_left").gameObject;
    minX = floor.transform.position.x - 10.0f;
    minZ = floor.transform.position.z - 10.0f;
    
    spawn();
    weightsSum = getWeightsSum();
    }
    
    public void spawn() 
    {
        
        Vector3 entityPos = new Vector3(Random.Range(minX, maxX), 0.5f, Random.Range(minZ, maxZ));
        entities.Add((Instantiate(entity1, entityPos, Quaternion.identity), 0.75f));
        Vector3 newEntityPos;
        int i;
        for (i = 0; i < numEntities - 1; i++)
        {
            newEntityPos = new Vector3(Random.Range(entityPos.x + 5.0f, entityPos.x - 5.0f), 0.5f, Random.Range(entityPos.z + 5.0f, entityPos.z - 5.0f) );
            entities.Add((Instantiate(entity2, newEntityPos, Quaternion.identity), 0.25f));
            entityPos = newEntityPos;
        }
        
    }
    
    private float getWeightsSum ()
    {
        float sum = 0;
        
        foreach((GameObject entityGameObjRef, float weight) entity in entities)
        {
            sum += entity.weight;
        }
        
        return sum;
    }
    
    public void move()
    {
        entities[0].entityGameObjRef.transform.position = new Vector3(Random.Range(minX, maxX), 0.5f, Random.Range(minZ, maxZ));
        
        int i;
        for (i = 1; i < numEntities; i++)
        {
        entities[i].entityGameObjRef.transform.position = new Vector3(Random.Range(entities[i-1].entityGameObjRef.transform.position.x + 5.0f, entities[i-1].entityGameObjRef.transform.position.x - 5.0f),
                                                                      0.5f, 
                                                                      Random.Range(entities[i-1].entityGameObjRef.transform.position.z + 5.0f, entities[i-1].entityGameObjRef.transform.position.z - 5.0f));
        }
    }
}
