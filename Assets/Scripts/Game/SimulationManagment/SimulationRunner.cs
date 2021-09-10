using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.SocialPlatforms;

[RequireComponent(typeof(City))]
[RequireComponent(typeof(Hospital))]
public class SimulationRunner : MonoBehaviour
{
    //Fields

    #region Virus spreading stats

    /// <summary>
    /// Number of People in city
    /// </summary>
    public int population = 100;

    /// <summary>
    /// Day 0 patients
    /// </summary>
    public int originalInfected = 1;

    /// <summary>
    /// Days where virus is incubating in patient (after infection) 
    /// </summary>
    public int incubationPeriod = 1;

    /// <summary>
    /// Days until infected person recovers/dies (is removed) 
    /// </summary>
    public int recoveryPeriod = 24;

    /// <summary>
    /// Rate at which infection is spread  
    /// </summary>
    [Range(0, 100)] public int infectionRate = 100;

    /// <summary>
    /// Infection radius
    /// </summary>
    public float infectionRadius = 20.0f;

    /// <summary>
    /// Quarantine capacity
    /// </summary>
    public int capacity = 0;

    /// <summary>
    /// Intention to move 
    /// </summary>
    public int intentionToMove = 100;

    /// <summary>
    /// Days before govt quarantines infected
    /// </summary>
    public int responseTime = 1000;


    public float avgSpeed = 3;


    public bool spawnGoats = false;
    
    #endregion


    //Fields

    #region Fields of city, hospital

    /// <summary>
    /// city component of the game panel
    /// </summary>
    private City city;

    /// <summary>
    /// hospital component of the game panel
    /// </summary>
    private Hospital hospital;


    public bool runningGameVersion = false; 

    // Start is called before the first frame update

    #endregion

    //Methods

    #region Methods of monobehaviors

    void Start()
    {

        Debug.Log("<color=white>SimulationRunner.Start()</color>");
        city = GetComponent<City>();
        hospital = GetComponent<Hospital>();
        city.Init();
        hospital.Init();
        if (runningGameVersion)
        {
            StartCoroutine(BootStart());
        }
    }


    IEnumerator BootStart()
    {
        float waitTime = 0.1f;
        Debug.Log("Waiting "+ waitTime +"s before calling BeginSimulation");
        yield return new WaitForSeconds(waitTime);
        BeginSimulation();
        
        EventCenter.GetInstance().AddEventListener("Time Up", TimeUp);
    }

    void TimeUp()
    {
        Debug.Log("GAME OVER");
        
    }

    void Update()
    {
        if (runningGameVersion)
        {
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                Debug.Log("Quit Simulation");
                PoolMgr.GetInstance().Clear();
                SceneManager.LoadScene("Menu");
            }
            else if (Input.GetKeyDown(KeyCode.R))
            {
                Debug.Log("Reset Simulation");
                EndSimulation();
                SceneManager.LoadScene(SceneManager.GetActiveScene().name);
            }
        }
        
    }

    #endregion


    #region Methods of start/stop

    void BeginSimulation()
    {
        Debug.Log("<color=yellow>->BEGIN SIM<-</color>");
        SetDefaults();
        EventCenter.GetInstance().EventTrigger<int>("Add People", Virus.population);
        EventCenter.GetInstance().EventTrigger<int>("Init Data", Virus.capacity);
    }

    void EndSimulation()
    {
        EventCenter.GetInstance().EventTrigger("Clear People");
        EventCenter.GetInstance().EventTrigger("Game Stop");
    }


    #endregion

    #region Methods of game setting initialization

    void SetDefaults()
    {
        Virus.population = population;
        Virus.originalInfected = originalInfected;
        Virus.incubationPeriod = incubationPeriod;
        Virus.infectionRate = infectionRate;
        Virus.infectionRadius = infectionRadius;
        Virus.capacity = capacity;
        Debug.Log("Set intention to move " + intentionToMove);
        Virus.intentionToMove = intentionToMove;
        Debug.Log("nw intention to move " + Virus.intentionToMove);
        Virus.responseTime = responseTime;
        Virus.recoveryPeriod = recoveryPeriod;
        Virus.meanMovementSpeed = avgSpeed;
        Virus.SpawnGoats = spawnGoats;
    }

    #endregion
}