using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using CodeMonkey.Utils;
using static Window_Graph;
using static Person;


public class SimulationGraph : MonoBehaviour
{
    private RectTransform graphContainer;
    public Sprite dotSprite;
    

    private Window_Graph _windowGraph;
    
    private int numStartingXpoints = 10;

    [SerializeField] private Dictionary<string, List<int>> valueList;
    [SerializeField] private Dictionary<string, Color> colorList;
    private IGraphVisual lineGraphVisual;



    
    
    private void Awake()
    {
        _windowGraph = gameObject.AddComponent<Window_Graph>();
        graphContainer = transform.Find("graphContainer").GetComponent<RectTransform>();
        Init();
        EventCenter.GetInstance().AddEventListener("Game Stop", Init);
    }


    private void Init()
    {
        initLists();
        lineGraphVisual = new LineGraphVisual(graphContainer, dotSprite, colorList, colorList);
        _windowGraph.ShowGraph(valueList, lineGraphVisual);
    }


    private void initLists()
    {
        valueList = new Dictionary<string, List<int>>();
        valueList.Add(GameStrings.infectedKey, Enumerable.Range(0, numStartingXpoints).Select(i => -5).ToList());
        valueList.Add(GameStrings.removedKey, Enumerable.Range(0, numStartingXpoints).Select(i => -5).ToList());
        valueList.Add(GameStrings.susceptibleKey, Enumerable.Range(0, numStartingXpoints).Select(i => -5).ToList());
//        valueList.Add(GameStrings.quarKey, Enumerable.Range(0, numStartingXpoints).Select(i => -5).ToList());
//        valueList.Add(GameStrings.IncKey, Enumerable.Range(0, numStartingXpoints).Select(i => -5).ToList());
        colorList = new Dictionary<string,Color>();
        colorList.Add(GameStrings.infectedKey, GameColors.Infected);
        colorList.Add(GameStrings.removedKey, GameColors.Recovered);
        colorList.Add(GameStrings.susceptibleKey, GameColors.Healthy);
//        colorList.Add(GameStrings.quarKey, GameColors.Hospital);
//        colorList.Add(GameStrings.IncKey, GameColors.Incubation);
        
    }

    public void UpdateGraph(int idx, Dictionary<string, int> newVal)
    {
        Debug.Log("Update idx " + idx);
        int len = valueList.First().Value.Count;
        if (len-1 > idx)
        {
            _windowGraph.UpdateValue(idx, newVal);
        }
        else
        {
            
            foreach (var item in valueList)
            {
                List<int> extension = Enumerable.Range(0, numStartingXpoints).Select(x =>-5).ToList();
                valueList[item.Key].AddRange(extension);
            }
            _windowGraph.ShowGraph(valueList, lineGraphVisual);
            _windowGraph.UpdateValue(idx, newVal);
        }
        
    }

}
