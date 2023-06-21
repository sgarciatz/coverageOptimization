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
    
    [SerializeField, Tooltip("List of spawned entities")] public List<(GameObject entityGameObjRef, float weight)> entities = new List<(GameObject entityGameObjRef, float weight)>();

    
    public void Start()
    {
    GameObject floor = gameObject.transform.Find("floor_front_right").gameObject;
    maxX = floor.transform.position.x + 5.0f;
    maxZ = floor.transform.position.z + 5.0f;
    
    
    floor = gameObject.transform.Find("floor_back_left").gameObject;
    minX = floor.transform.position.x - 5.0f;
    minZ = floor.transform.position.z - 5.0f;
    
    spawn();
    }
    
    public void spawn() 
    {
        Vector3 entityPos = new Vector3(Random.Range(minX, maxX), 0.5f, Random.Range(minZ, maxZ));
        (GameObject entityGameObjRef, float weight) newEntity = (Instantiate(entity1, entityPos, Quaternion.identity), 0.75f);
        entities.Add(newEntity);
        
        Vector3 secondEntityPos = new Vector3(Random.Range(entityPos.x + 5.0f, entityPos.x - 5.0f), 0.5f, Random.Range(entityPos.z + 5.0f, entityPos.z - 5.0f) );
        //Vector3 secondEntityPos = new Vector3(Random.Range(minX, maxX), 0.5f, Random.Range(minZ, maxZ));
        (GameObject entityGameObjRef, float weight) secondNewEntity = (Instantiate(entity2, secondEntityPos, Quaternion.identity), 0.25f);
        entities.Add(secondNewEntity);
    }
    
    public void move()
    {
        entities[0].entityGameObjRef.transform.position = new Vector3(Random.Range(minX, maxX), 0.5f, Random.Range(minZ, maxZ));
        
        
        entities[1].entityGameObjRef.transform.position = new Vector3(Random.Range(entities[0].entityGameObjRef.transform.position.x + 5.0f, entities[0].entityGameObjRef.transform.position.x - 5.0f),
                                                          0.5f, 
                                                          Random.Range(entities[0].entityGameObjRef.transform.position.z + 5.0f, entities[0].entityGameObjRef.transform.position.z - 5.0f));
        
        //entities[1].entityGameObjRef.transform.position = new Vector3(Random.Range(minX, maxX), 0.5f, Random.Range(minZ, maxZ));
    }
}
