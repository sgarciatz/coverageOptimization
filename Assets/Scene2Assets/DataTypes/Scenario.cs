using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Scenario : MonoBehaviour
{
    private Vector3 _minPoint;
    private Vector3 _maxPoint;
    
    
    public Vector3 minPoint
    {
        get
        {
            return new Vector3(_minPoint.x, _minPoint.y, _minPoint.z);
        }
    }
    public float minX
    {
        get
        {
            return _minPoint.x;
        }
    }
    public float minY
    {
        get
        {
            return _minPoint.y;
        }
    }
    public float minZ
    {
        get
        {
            return _minPoint.z;
        }
    }
    
    public Vector3 maxPoint
    {
        get
        {
            return new Vector3(_maxPoint.x, _maxPoint.y, _maxPoint.z);
        }
    }
    public float maxX
    {
        get
        {
            return _maxPoint.x;
        }
    }
    public float maxY
    {
        get
        {
            return _maxPoint.y;
        }
    }
    public float maxZ
    {
        get
        {
            return _maxPoint.z;
        }
    }   
    public Vector3 centerPoint
    {
        get
        {
            return _maxPoint - _minPoint;
        }
    }
    
    /// <summary>
    /// Method <c>Start</c> is called before the first frame update. It initializes variables.
    /// </summary>
    void Start()
    {
        float minHeight = 100.0f; 
        float maxHeight = 0.0f;
        Vector3 globalPos = gameObject.transform.Find("Terrain").position;
        float localScale = gameObject.transform.Find("Terrain").localScale.x * 10;
        _minPoint = new Vector3(globalPos.x - localScale/2, globalPos.y - maxHeight, globalPos.z - localScale/2);
        _maxPoint = new Vector3(globalPos.x + localScale/2, globalPos.y + minHeight, globalPos.z + localScale/2);
        Debug.Log($"Scenario boundaries: min = ({_minPoint}), max = ({_maxPoint})");
    }

    public void UpdateScenarioSize()
    {
        float minHeight = 100.0f; 
        float maxHeight = 0.0f;
        Vector3 globalPos = gameObject.transform.Find("Terrain").position;
        float localScale = gameObject.transform.Find("Terrain").localScale.x * 10;
        _minPoint = new Vector3(globalPos.x - localScale/2, globalPos.y - maxHeight, globalPos.z - localScale/2);
        _maxPoint = new Vector3(globalPos.x + localScale/2, globalPos.y + minHeight, globalPos.z + localScale/2);
        Debug.Log($"Scenario boundaries: min = ({_minPoint}), max = ({_maxPoint})");    
    }
    /// <summary>
    /// Method <c>normalizeVector</c> min max scales a vector relatively to the scenario position and its scale.
    /// </summary>    
    public Vector3 normalizeVector (Vector3 vectorToNormalize)
    {
        Vector3 normalizedVector = new Vector3((vectorToNormalize.x - minX) / (maxX - minX),
                                       (vectorToNormalize.y - minY) / (maxY - minY),
                                       (vectorToNormalize.z - minZ) / (maxZ - minZ));
       
        Debug.Assert(normalizedVector.x <= 1.0f && normalizedVector.y <= 1.0f && normalizedVector.z <= 1.0f &&
                     normalizedVector.x >= 0.0f && normalizedVector.y >= 0.0f && normalizedVector.z >= 0.0f,
                     $"Normalization of v=({vectorToNormalize}) not working! => normalized v = ({normalizedVector})");    
        
        return normalizedVector;
    }
    
  
    /// <summary>
    /// Method <c>isVectorOutsideScenario</c> checks if vector is outside the scenario boundaries.
    /// </summary>    
    public bool isVectorOutsideScenario (Vector3 vector)
    {
        if (vector.x < minX || vector.x > maxX ||
            vector.y < minY || vector.y > maxY ||
            vector.z < minZ || vector.z > maxZ)
        {
            return true;
        }
        else
        {
            return false;
        }
    }
    
    public Vector3 getCenterPoint()
    {
        Vector3 centerPoint = Vector3.Lerp(_minPoint, _maxPoint, 0.5f);
        
        return new Vector3(centerPoint.x, 0.0f, centerPoint.z);
    }
}
