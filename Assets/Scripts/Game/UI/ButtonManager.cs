using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using UnityEngine;
using UnityEngine.SceneManagement;

public class ButtonManager : MonoBehaviour
{


    public void BackHome()
    {
        SceneManager.LoadScene("Menu");
    }

    public void FreePlay()
    {
        SceneManager.LoadScene("SIRscene");
    }

    public void BAdGovt()
    {
        SceneManager.LoadScene("BadGovt");
    }

    public void GoodGovt()
    {
        SceneManager.LoadScene("Goodgovt");
    }
    

}
