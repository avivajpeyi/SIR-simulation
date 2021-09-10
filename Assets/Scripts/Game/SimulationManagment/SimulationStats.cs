using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SimulationStats : MonoBehaviour
{
    //Fields

    #region Fields of counts 

    [SerializeField] private int healthy;
    [SerializeField] private int incubation;
    [SerializeField] private int infected;
    [SerializeField] private int removed;
    [SerializeField] private int daysGone;
    private int quarintine;
    
    #endregion

    #region Fields of achives 

    [SerializeField] private List<int> healthyArchive;
    [SerializeField] private List<int> incubationArchive;
    [SerializeField] private List<int> infectedArchive;
    [SerializeField] private List<int> removedArchive;
    [SerializeField] private List<int> daysArchive;
    private List<int> quarintineArchive;
    
    #endregion

    #region Fields of hospital capacity data that will be shown on the panel

    private int capacityNum;
    private int currentCapacity;

    #endregion

    /// <summary>
    /// if the game has started
    /// </summary>
    private bool _gameStart = false;

    private float _timeElapsed = 0.0f;

    private SimulationGraph _graph;

    private bool runningGameVersion;
    

    //Methods

    #region Methods of nomobehaviours

    void Start()
    {
        Debug.Log("<color=white>SimulationStats.Start()</color>");
        EventCenter.GetInstance().AddEventListener<int>("Init Data", Init);
        runningGameVersion = GetComponent<SimulationRunner>().runningGameVersion;
    }


    public float getGoat()
    {
        return GetStatPercent(removed + infected);
    }
    
    public float getHuman()
    {
        return GetStatPercent(incubation + healthy + quarintine); 
    }


    #endregion

    #region Methods of initialization when game start

    void Init(int num)
    {
        Debug.Log("<color=white>SimulationStats.Init()</color>");
        _graph = FindObjectOfType<SimulationGraph>();
        
        daysGone = 0;
        capacityNum = num;
        currentCapacity = num;
        healthy = (Virus.population - Virus.originalInfected);
        incubation = 0;
        quarintine = 0;
        infected = Virus.originalInfected;
        removed = 0;
        _timeElapsed = 0.0f;
        

        quarintineArchive= new List<int>();
        healthyArchive = new List<int>();
        incubationArchive = new List<int>();
        infectedArchive = new List<int>();
        removedArchive = new List<int>();
        daysArchive = new List<int>();
        UpdateArchives();


        EventCenter.GetInstance()
            .AddEventListener("Game Start", () => { _gameStart = true; });
        EventCenter.GetInstance()
            .AddEventListener("Time Up", () => { _gameStart = false; });
        
        EventCenter.GetInstance().AddEventListener("Game Stop", Clear);
        EventCenter.GetInstance()
            .AddEventListener<Person.E_Person_Status>("Minus Data", MinusData);
        EventCenter.GetInstance()
            .AddEventListener<Person.E_Person_Status>("Add Data", AddData);
    }

    #endregion

    #region Methods to call when receive manipulate data event

    void MinusData(Person.E_Person_Status status)
    {
        switch (status)
        {
            case Person.E_Person_Status.Healthy:
                healthy = healthy - 1;
                break;

            case Person.E_Person_Status.Incubation:
                incubation = incubation - 1;
                break;

            case Person.E_Person_Status.Infected:
                infected = infected - 1;
                break;

            case Person.E_Person_Status.Hospital:
                quarintine = quarintine -1;
                currentCapacity++;
                break;

            case Person.E_Person_Status.Recovered:
                removed = removed - 1;
                break;
        }
    }

    void AddData(Person.E_Person_Status status)
    {
        switch (status)
        {
            case Person.E_Person_Status.Healthy:
                healthy = healthy + 1;
                break;

            case Person.E_Person_Status.Incubation:
                incubation = incubation + 1;
                break;

            case Person.E_Person_Status.Infected:
                infected = infected + 1;
                break;

            case Person.E_Person_Status.Hospital:
                quarintine = quarintine +1;
                currentCapacity--;
                break;

            case Person.E_Person_Status.Recovered:
                removed = removed + 1;
                break;
        }
    }

    #endregion

    #region Methods to call to clear data when game over

    void Clear()
    {
        EventCenter.GetInstance()
            .RemoveEventListener<Person.E_Person_Status>("Minus Data", MinusData);
        EventCenter.GetInstance()
            .RemoveEventListener<Person.E_Person_Status>("Add Data", AddData);
        EventCenter.GetInstance().RemoveEventListener("Game Stop", Clear);
        healthy = 0;
        incubation = 0;
        quarintine = 0;
        infected = 0;
        daysGone = 0;
        _gameStart = false;
    }

    #endregion

    #region Method called every frame

    void Update()
    {
        if (_gameStart)
        {
            _timeElapsed += Time.deltaTime;
            int new_day = (int) _timeElapsed;
            if (new_day > daysGone)
            {
                UpdateArchives();
                UpdateGraph();
            }

            daysGone = (int) _timeElapsed;

            if (daysGone >= 50)
            {
                if (runningGameVersion)
                    EventCenter.GetInstance().EventTrigger("Time Up");
                
            }

        }
    }


    void UpdateArchives()
    {
        healthyArchive.Add(GetStatPercent(healthy));
        incubationArchive.Add(GetStatPercent(incubation));
        quarintineArchive.Add(GetStatPercent(quarintine));
        infectedArchive.Add(GetStatPercent(infected));
        removedArchive.Add(GetStatPercent(removed));
        daysArchive.Add(daysGone);
    }

    void UpdateGraph()
    {
        if (_graph != null)
        {
            var dat = new Dictionary<string, int>
            {
                {GameStrings.infectedKey, infectedArchive[daysGone]},
                {GameStrings.removedKey, removedArchive[daysGone]},
                {GameStrings.susceptibleKey,healthyArchive[daysGone]},
//                {GameStrings.quarKey,quarintineArchive[daysGone]},
//                {GameStrings.IncKey,incubationArchive[daysGone]}
            };
            _graph.UpdateGraph(daysGone, dat);    
        }

        
    }

    #endregion

    int GetStatPercent(int stat)
    {
        float percentFactor = 100.0f / (float) Virus.population;
        return Mathf.RoundToInt(stat * percentFactor);
    }
}