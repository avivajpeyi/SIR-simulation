using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class SliderLabel : MonoBehaviour
{
     private string formatText = "{0:0.#}";

    private TMP_Text tmproText;
    private Slider slider;

    private void Start()
    {
        tmproText = GetComponentInChildren<TMP_InputField>().transform.Find("Text")
        .GetComponent<TMP_Text>();
        slider = GetComponentInChildren<Slider>();
        slider.onValueChanged.AddListener(HandleValueChanged);
        HandleValueChanged(slider.value);
    }

    private void HandleValueChanged(float value)
    {
        Debug.Log(this.name + " val changed to " + value);
        tmproText.text = string.Format(formatText, value);
    }
}
