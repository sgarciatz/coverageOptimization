using UnityEngine;
using Unity.MLAgents;

public class EnvironmentManager : MonoBehaviour
{
    private float spawnPadding;
    private PeopleSpawn pSpawn;
    private ObstacleSpawn oSpawn;
    void Start()
    {
        pSpawn = GetComponent<PeopleSpawn>();
        oSpawn = GetComponent<ObstacleSpawn>();
        
        float firstPadding = Academy.Instance.EnvironmentParameters.GetWithDefault("spawnPadding", 150.0f);
        pSpawn.padding = firstPadding;
        oSpawn.padding = firstPadding;
        Academy.Instance.EnvironmentParameters.RegisterCallback("spawnPadding", padding =>
        {
            Debug.Log("Updating spawns padding!");
            pSpawn.padding = padding;
            oSpawn.padding = padding;
        });

        
    } 
}
