//=============================
//Author: Zack Yang 
//Created Date: 09/24/2020 14:32
//=============================

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEngine.UI;
using Random = UnityEngine.Random;


public class City : MonoBehaviour
{
    //Fileds
    #region Field of animation image
    /// <summary>
    /// the loading animation image when game start
    /// </summary>
    public Image loadingAnimation;
    #endregion

    #region Field of city's range, people and currentInfected
    /// <summary>
    /// the range of city
    /// </summary>
    public Tilemap cityRange;

    public List<Vector3> pointsInCity;
    /// <summary>
    /// the list of all current alive people
    /// </summary>
    public List<Person> people = new List<Person>();
    /// <summary>
    /// amount of current infected people. Only use for initialzation.
    /// </summary>
    private int currentInfected = 0;
    #endregion

    [SerializeField] private Hospital hospital; 
    

    public void OnDrawGizmos()
    {
        Gizmos.color = new Color(0, 50,50,0.1f);
        Gizmos.DrawCube(
            cityRange.transform.position,
            cityRange.localBounds.extents/2
            );
    }


    //Methods
    #region Methods of initialization

    public void Start()
    {
        if(loadingAnimation != null)
            loadingAnimation.gameObject.SetActive(false);
    }


    public Vector3 GetRandomPosition()
    {
        return pointsInCity[Random.Range(0, pointsInCity.Count)];
    }

    public void InitCityPts()
    {
        cityRange.CompressBounds();
        pointsInCity = new List<Vector3>();

        foreach (Vector3Int position in cityRange.cellBounds.allPositionsWithin)
        {
            if (cityRange.HasTile(position))
            {
                Vector3 place = cityRange.CellToWorld(position); // convert this tile map grid coords to local space coords
                pointsInCity.Add(place);
            }
        }

    }

    public void Init()
    {
        Debug.Log("<color=white>City.Init()</color>");
        Clear();
        EventCenter.GetInstance().AddEventListener<int>("Add People", AddPerson);
        EventCenter.GetInstance().AddEventListener("Clear People", Clear);
        InitCityPts();
        hospital = FindObjectOfType<Hospital>();
    }
    #endregion

    #region Methods of add people to city when game start

    /// <summary>
    /// add all people to city
    /// </summary>
    /// <param name="num"></param>
    public void AddPerson(int num)
    {
        currentInfected = 0;
        if (this !=null )
            StartCoroutine(BeginCreate(num));
    }

    /// <summary>
    /// add all people to city coroutine
    /// </summary>
    /// <param name="num"></param>
    /// <returns></returns>
    private IEnumerator BeginCreate(int num)
    {
        float waitTime = 1f;
        float t = Time.deltaTime;
        if(loadingAnimation != null)
            loadingAnimation.gameObject.SetActive(true);

        for (int i = 0; i < num; i++)
        {

            string prefabStr = "Prefabs/Person";

            PoolMgr.GetInstance().GetObj(prefabStr, (obj) =>
            {
                Person p = obj.GetComponent<Person>();
                people.Add(p);
                p.Init(cityRange, this, hospital);

                if (currentInfected < Virus.originalInfected)
                {
                    p.ChangeStatus(Person.E_Person_Status.Infected);
                    currentInfected++;
                }
            });

            if (i % 100 == 0)
                yield return 0;
        }

        Debug.Log("BeginCreate took " + (t-Time.deltaTime) +"s. Waiting "+ waitTime +"s before calling 'Game Start'");
        yield return new WaitForSeconds(waitTime);
        loadingAnimation.gameObject.SetActive(false);
        EventCenter.GetInstance().EventTrigger("Game Start");
    }
    #endregion

    #region Methods of delete all people from city when game ends
    /// <summary>
    /// clear people from city
    /// </summary>
    public void Clear()
    {
        StartCoroutine(beginClear());
    }

    /// <summary>
    /// clear people from city coroutine
    /// </summary>
    /// <returns></returns>
    private IEnumerator beginClear()
    {
        for (int i = 0; i < people.Count; i++)
        {
            people[i].ChangeStatus(Person.E_Person_Status.Null);
            PoolMgr.GetInstance().PushObj("Prefabs/Person", people[i].gameObject);
            if (i % 100 == 0)
                yield return 0;
        }
        people.Clear();
    }
    #endregion
}
