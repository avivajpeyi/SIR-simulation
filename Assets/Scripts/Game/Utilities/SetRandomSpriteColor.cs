using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SetRandomSpriteColor : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        SpriteRenderer sp = GetComponent<SpriteRenderer>();
        Color col = new Color(
            Random.Range(0f, 1f),
            Random.Range(0f, 1f),
            Random.Range(0f, 1f)
        );
        sp.color = col;
    }
}