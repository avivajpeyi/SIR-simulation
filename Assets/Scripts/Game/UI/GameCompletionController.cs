using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class GameCompletionController : MonoBehaviour
{

    [SerializeField] private TMP_Text statusTxt;
    [SerializeField] private TMP_Text numHumanTxt;
    [SerializeField] private TMP_Text numGoatTxt;
    private SimulationStats _stats;
    
    // Start is called before the first frame update
    void Start()
    {
        _stats = FindObjectOfType<SimulationStats>();
        EventCenter.GetInstance()
            .AddEventListener("Time Up", DisplayStats);
        
        statusTxt.text = "";
        numGoatTxt.text = "";
        numHumanTxt.text = "";

    }


    void DisplayStats()
    {
        float goatPerc = _stats.getGoat();
        float huPerc = _stats.getHuman();

        if (goatPerc >= huPerc)
        {
            statusTxt.text = "Goats have taken over!";
        }
        else
        {
            statusTxt.text = "Crisis averted";
        }
        
        numHumanTxt.text = string.Format("Human\n{0:0.#}%", huPerc);
        numGoatTxt.text = string.Format("Goat\n{0:0.#}%", goatPerc);
    }
    
    

}
