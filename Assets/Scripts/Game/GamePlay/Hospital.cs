//=============================
//Author: Zack Yang 
//Created Date: 09/24/2020 19:27
//=============================
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class Hospital : MonoBehaviour
{
    //Fields
    #region Fields of hospital capacity, hospital UI range
    /// <summary>
    /// hospital current capacity
    /// </summary>
    private int currentCapacity = 0;
    /// <summary>
    /// hospital UI range
    /// </summary>
    public Tilemap hospitalRange;
    #endregion

    public List<Vector3> pointsInHospital;
    
    //Methods
    #region Methods of Initialization
    public void Init()
    {
        
        Debug.Log("<color=white>Hospital.Init()</color>");
        currentCapacity = 0;
        EventCenter.GetInstance().AddEventListener<Person>("Add Patient", AddPatient);
        InitCityPts();
    }
    #endregion
    
    
    public Vector3 GetRandomPosition()
    {
        return pointsInHospital[Random.Range(0, pointsInHospital.Count)];
    }

    public void InitCityPts()
    {
        hospitalRange.CompressBounds();
        pointsInHospital = new List<Vector3>();
        
        foreach (Vector3Int position in hospitalRange.cellBounds.allPositionsWithin)
        {
            if (hospitalRange.HasTile(position))
            {
                Vector3 place = hospitalRange.CellToWorld(position); // convert this tile map grid coords to local space coords
                //Tile at "place"
                pointsInHospital.Add(place);
            }
        }
        
        
        Debug.Log("NUM PTS " + pointsInHospital.Count);
        
    }

    #region Methods of add patient when there are still enough capacity
    public void AddPatient(Person p)
    {

        hospitalRange = GameObject.FindWithTag("Quarantine").GetComponent<Tilemap>();
        
        
        if (currentCapacity < Virus.capacity)
        {
            EventCenter.GetInstance().EventTrigger<Person.E_Person_Status>("Minus Data", Person.E_Person_Status.Infected);
            EventCenter.GetInstance().EventTrigger<Person.E_Person_Status>("Add Data", Person.E_Person_Status.Hospital);
            Debug.Log("Admitting to hospital with tilemap " + hospitalRange.gameObject
            .name);
            p.Init(hospitalRange, null, this);
            currentCapacity++;
            
        }
    }
    #endregion
}
