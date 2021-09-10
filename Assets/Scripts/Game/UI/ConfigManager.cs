using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class ConfigManager : MonoBehaviour
{
    //Fields
    #region Fields of UI inputField components for setting panel
    [SerializeField] private Slider population;
    [SerializeField] private Slider infectionRate;
    [SerializeField] private Slider originalInfected;
    [SerializeField] private Slider infectionRadius;
    [SerializeField] private Slider intentionToMove;
    [SerializeField] private Slider meanMovementSpeed;
    
    [SerializeField] private Toggle spawnGoats;
    
    [SerializeField] private TMP_InputField incubationPeriod;
    [SerializeField] private TMP_InputField capacity;
    [SerializeField] private TMP_InputField responseTime;
    [SerializeField] private TMP_InputField recoveryPeriod;
    #endregion

    #region Fields of UI Button components for setting panel
    private Button     beginBtn;
    private Button     stopBtn;
    #endregion

    //Methods
    #region Methods of monobehaviors
    void Start()
    {
        //get all the fields

        string b = "ConfigsPanel/Scroll View/Viewport/Content/";
        population = transform.Find(b+"PopulationPanel").GetComponentInChildren<Slider>();
        originalInfected = transform.Find(b+"InitialInfectedPanel").GetComponentInChildren<Slider>();
        infectionRate = transform.Find(b+"InfectionRateInputPanel").GetComponentInChildren<Slider>();
        infectionRadius = transform.Find(b+"InfectionRadiusPanel").GetComponentInChildren<Slider>();
        intentionToMove = transform.Find(b+"IntentionToMovePanel").GetComponentInChildren<Slider>();
        meanMovementSpeed = transform.Find(b+"MovementSpeedPanel").GetComponentInChildren<Slider>();

        spawnGoats = transform.Find(b + "GoatsPanel").GetComponentInChildren<Toggle>();
        
        capacity = transform.Find(b+"#QuarantineInputPanel").GetComponentInChildren<TMP_InputField>();
        responseTime = transform.Find(b+"DurationTillQuarantinePanel").GetComponentInChildren<TMP_InputField>();
        recoveryPeriod = transform.Find(b+"InfectionDurationInputPanel").GetComponentInChildren<TMP_InputField>();
        incubationPeriod = transform.Find(b+"IncubationPeriodInputPanel").GetComponentInChildren<TMP_InputField>();
        
        beginBtn =  transform.Find("StartButton").GetComponent<Button>();
        stopBtn =  transform.Find("StopButton").GetComponent<Button>();

            
            

        //initialize values
        stopBtn.gameObject.SetActive(false);


        beginBtn.onClick.AddListener(beginBtnClick);
//        stopBtn.onClick.AddListener(stopBtnClick);
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            Debug.Log("Quit Simulation");
            PoolMgr.GetInstance().Clear();
            SceneManager.LoadScene("Menu");
        }
    }
    #endregion

    #region Methods of btn click events
    void beginBtnClick()
    {
        //reset data
        ResetInfo();
        EventCenter.GetInstance().EventTrigger<int>("Add People", Virus.population);

        beginBtn.gameObject.SetActive(false);
        stopBtn.gameObject.SetActive(true);
        EventCenter.GetInstance().EventTrigger<int>("Init Data", Virus.capacity);
    }

    void stopBtnClick()
    {
        EventCenter.GetInstance().EventTrigger("Clear People");
        EventCenter.GetInstance().EventTrigger("Game Stop");
        stopBtn.gameObject.SetActive(false);
        beginBtn.gameObject.SetActive(true);
        
        // SceneManager.LoadScene("SIRscene");
    }
    #endregion

    #region Methods of game setting initialization
    void ResetInfo()
    {
        Debug.Log("RESET CONGID");
        float originalInfectPercent = originalInfected.value;
        int popVal = (int) population.value;
        int ogInfection = (int)((originalInfectPercent / 100.0) * popVal);

        Virus.population = popVal;
        Virus.originalInfected = ogInfection ;
        Virus.infectionRate = infectionRate.value;
        Virus.infectionRadius = infectionRadius.value;
        Virus.intentionToMove = intentionToMove.value;
        Virus.meanMovementSpeed = meanMovementSpeed.value;

        Virus.SpawnGoats = spawnGoats.isOn;
        
        Debug.Log("responseTime.text " + responseTime.text);
        Virus.responseTime = int.Parse(responseTime.text);
        Virus.capacity = int.Parse(capacity.text);
        Virus.recoveryPeriod = int.Parse(recoveryPeriod.text);;
        Virus.incubationPeriod = int.Parse(incubationPeriod.text);;
        
    }
    #endregion
}
