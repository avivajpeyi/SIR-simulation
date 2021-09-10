//=============================
//Author: Zack Yang 
//Created Date: 09/24/2020 14:33
//=============================

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using TMPro;
using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEngine.UI;
using Debug = UnityEngine.Debug;
using Random = UnityEngine.Random;

public class Person : MonoBehaviour
{
    //enums
    #region enum of person's health status
    /// <summary>
    /// all health status of person
    /// </summary>
    public enum E_Person_Status
    {
        Healthy=0,
        Infected=1,
        Recovered=2,
        Incubation=3,
        Hospital=4,
        Null
    }
    #endregion


    //fields
    #region Fields of person's space, status and image
    /// <summary>
    /// the region for people to move within
    /// </summary>
    private Tilemap _space;
    /// <summary>
    /// the health status of person
    /// </summary>
    [SerializeField]  private E_Person_Status _status = E_Person_Status.Null;
    /// <summary>
    /// the image of person
    /// </summary>
    [SerializeField] private List<SpriteRenderer> _sprite;
    /// <summary>
    /// the city object
    /// </summary>
    private City _city;
    private Hospital _hospital;
    /// <summary>
    /// if the game has started
    /// </summary>
    private bool _gameStart = false;

    /// <summary>
    /// Visual for infection circle
    /// </summary>
    private GameObject _infectionCircleCurve;
    

    
    #endregion

    #region fields of person's moving direction
    /// <summary>
    /// the moving speed of the person
    /// </summary>
    private float _speed = 0;

    #endregion

    #region Fields of person's infection logic
    /// <summary>
    /// the number of frame before each infection check
    /// </summary>
    private int _checkInfectionFrame = 30;

    /// <summary>
    /// current frame number
    /// </summary>
    private int _currentInfectionFrame = 0;
    /// <summary>
    /// the number of frame before a infected person get into hospital
    /// </summary>
    private float _waitTimeToHospital = 0;
    #endregion


    private bool isDead = false;
    public bool canDie = true;

    #region Fields of person's incubation period
    private float _waitTimeToInfected = 0f;
    #endregion
    
    #region Fields of person's recovery period
    private float _waitTimeToRecovered = 0f;
    #endregion
    
    
    int zPos = 0;


    private RandomWander rw;

    //methods
    #region methods of person's initialization
    /// <summary>
    /// Initialize the properties of person when it is created
    /// </summary>
    /// <param name="space"></param>
    public void Init(Tilemap space, City city = null, Hospital hospital= null)
    {

        rw = GetComponent<RandomWander>();
        
        

        _speed = Random.Range(
            Mathf.Clamp( Virus.meanMovementSpeed-5, 0, Virus.meanMovementSpeed-5f),  
            Mathf.Clamp( Virus.meanMovementSpeed+5, 0, Virus.meanMovementSpeed+5f)
            ) * 25.0f;
        if (rw!=null)
            rw._speed = _speed;

        if (city == null)
            city = FindObjectOfType<City>();
        if (hospital == null)
            hospital = FindObjectOfType<Hospital>();
        
        _city = city;
        _hospital = hospital;
        

        _space = space;
        
        if (_space == null)
            Debug.Log(name + "has not been given a tilemap");

        if (_space.gameObject.CompareTag("Floor"))
        {
            ChangeStatus(E_Person_Status.Healthy);
            gameObject.transform.position = _city.GetRandomPosition();   
        }
        
        else if (_space.gameObject.CompareTag("Quarantine"))
        {
            ChangeStatus(E_Person_Status.Hospital);    
            gameObject.transform.position = _hospital.GetRandomPosition();
            Debug.Log("PLACED IN HOSPITAL");
            if (rw != null)
            {rw.ZeroMovement();
            }

            
            isDead = true;
        }

        
        else
        {
            throw new Exception("Person init on invalid tilemap: " +space.tag);
        }

        _infectionCircleCurve = transform.Find("InfectionCircle").gameObject;
        
        
        // FIXME: DOESNT WORK FOR HOSPITAL
        
        
        

        
        EventCenter.GetInstance().AddEventListener("Game Start", () =>
        {
            _gameStart = true;
        });

        EventCenter.GetInstance().AddEventListener("Game Stop", () =>
        {
            _gameStart = false;
        });
        
        EventCenter.GetInstance().AddEventListener("Time Up", () =>
        {
            _gameStart = false;
            this.rw.ZeroMovement();
        });
        
    }
    #endregion
    


    
    
    
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(0, 50,0, 0.1f);
//        Gizmos.DrawSphere(transform.position, Virus.infectionRadius);
    }


    #region methods to change person's health status

    void SetSpriteCol(Color c)
    {
        if (canDie)
        {
            foreach (var s in _sprite)
            {
                s.color = c;
            }    
        }

        
    }


    /// <summary>
    /// change the health status of person
    /// </summary>
    /// <param name="status"></param>
    public void ChangeStatus(E_Person_Status status)
    {
        _status = status;
        switch (status)
        {
            case E_Person_Status.Healthy:
                SetSpriteCol(GameColors.Healthy);
                break;
            case E_Person_Status.Incubation:
                _waitTimeToInfected = 0;
                SetSpriteCol(GameColors.Incubation);
                break;
            case E_Person_Status.Infected:
                _waitTimeToHospital = 0;
                _waitTimeToRecovered = 0;
                SetSpriteCol(GameColors.Infected);
                _infectionCircleCurve.SetActive(true);
                break;
            case E_Person_Status.Hospital:
                SetSpriteCol(GameColors.Hospital);
                break;
            case E_Person_Status.Recovered:
                SetSpriteCol(GameColors.Recovered);
                break;
        }
    }
    #endregion





    #region methods of how person infect others
    void CheckInfection()
    {
        if (_status == E_Person_Status.Infected || _status == E_Person_Status.Incubation)
        {
            _currentInfectionFrame ++;
            if (_currentInfectionFrame > _checkInfectionFrame && _city != null)
            {
                StartCoroutine(CheckInfectionCoroutine());
                _currentInfectionFrame = 0;
            }
        }
    }

    /// <summary>
    /// Check distance to others near me, and infect them 
    /// </summary>
    /// <returns></returns>
    IEnumerator CheckInfectionCoroutine()
    {
        float infectionRate = Virus.infectionRate * Virus.intentionToMove;
        for(int i = 0; _city != null && i < _city.people.Count; i++)
        {
            if (_city.people[i]._status == E_Person_Status.Healthy && Vector3.Distance
            (gameObject.transform.localPosition, _city.people[i].gameObject.transform
            .localPosition) < Virus.infectionRadius && Random.Range(0f, 1f) < infectionRate)
            {
                EventCenter.GetInstance().EventTrigger<E_Person_Status>("Minus Data", _city.people[i]._status);
                if (Virus.incubationPeriod > 0)
                    _city.people[i].ChangeStatus(E_Person_Status.Incubation);
               
                else
                    _city.people[i].ChangeStatus(E_Person_Status.Infected);
        
                    
                EventCenter.GetInstance().EventTrigger<E_Person_Status>("Add Data", _city.people[i]._status);
            }

            if (i % 100 == 0)
                yield return 0;
        }
    }
    #endregion

    #region methods of person's check in and check from to a hospital
    /// <summary>
    /// if infected and wait for more than the minimum time to be witnessed by hospital,
    /// publish an event
    /// </summary>
    void CheckHospital()
    {
        if (_status == E_Person_Status.Infected)
        {
            _waitTimeToHospital += Time.deltaTime;

            if (_waitTimeToHospital > Virus.responseTime)
                EventCenter.GetInstance().EventTrigger<Person>("Add Patient", this);
        }
    }
    #endregion
    
    
    

    #region methods of how person change from incubated to infected
    /// <summary>
    /// check if a person pass his incubation period
    /// </summary>
    void IncubationToInfected()
    {
        if (_status == E_Person_Status.Incubation)
        {
            _waitTimeToInfected += Time.deltaTime;
            if (_waitTimeToInfected > Virus.incubationPeriod)
            {
                _infectionCircleCurve.SetActive(true);
                EventCenter.GetInstance().EventTrigger<Person.E_Person_Status>("Minus Data", Person.E_Person_Status.Incubation);
                EventCenter.GetInstance().EventTrigger<Person.E_Person_Status>("Add Data", Person.E_Person_Status.Infected);
                ChangeStatus(E_Person_Status.Infected);
            }
        }
    }
    #endregion

    
    
    #region methods of how person chages from infected to recovered
    /// <summary>
    /// check if a person pass his incubation period
    /// </summary>
    void InfectedToRecovered()
    {
        if (_status == E_Person_Status.Infected)
        {
            _waitTimeToRecovered += Time.deltaTime;
            if (_waitTimeToRecovered > Virus.recoveryPeriod)
            {
                _infectionCircleCurve.SetActive(false);
                EventCenter.GetInstance().EventTrigger<Person.E_Person_Status>("Minus Data", Person.E_Person_Status.Infected);
                EventCenter.GetInstance().EventTrigger<Person.E_Person_Status>("Add Data", Person.E_Person_Status.Recovered);
                ChangeStatus(E_Person_Status.Recovered);
                DieAnim();
                transform.Find("Shadow").gameObject.SetActive(false);
            }
        }
    }
    #endregion
    
    
    #region methods of how person visuals





    void DieAnim()
    {
        if (canDie)
        {
            float rot;
            if (Random.Range(0.0f, 1.0f) > 0.5f)
            {
                rot = 90;
            }
            else
            {
                rot = -90;
            }
            Quaternion newRot = Quaternion.Euler(0,0,rot);
            transform.rotation = newRot;
            rw.ZeroMovement();
            isDead = true;
            GetComponent<Animator>().enabled = false;


            if (Virus.SpawnGoats)
            {
                string prefabStr = "Prefabs/Goat";
                GameObject instance = Instantiate(Resources.Load(prefabStr, typeof
                (GameObject))) as GameObject;
                instance.transform.position = transform.position;
                instance.GetComponent<RandomWander>()._speed = _speed;
                instance.transform.parent = transform;
            }
        }
    }



    #endregion


    public bool isInfected()
    {
        return _status == E_Person_Status.Infected;
    }


    #region Methods of monobehaviors
    void Update()
    {
        if (_gameStart)
        {
            if (!isDead)
            {
                rw.Move();
                CheckInfection();
                CheckHospital();
                IncubationToInfected();
                InfectedToRecovered();
            }

        }
    }
    #endregion
}
