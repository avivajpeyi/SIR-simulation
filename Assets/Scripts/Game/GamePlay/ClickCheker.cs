using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ClickCheker : MonoBehaviour
{
    
    private bool _gameStart = false;
    
    // Start is called before the first frame update
    void Start()
    {
        EventCenter.GetInstance()
            .AddEventListener("Game Start", () => { _gameStart = true; });
        EventCenter.GetInstance().AddEventListener("Game Stop", () =>
        {
            _gameStart = false;
        });
    }

    // Update is called once per frame
    void Update()
    {
        if (_gameStart)
        {
            if (Input.GetMouseButtonDown(0)) {
                Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
                Vector2 mousePos2D = new Vector2(mousePos.x, mousePos.y);
            
                RaycastHit2D hit = Physics2D.Raycast(mousePos2D, Vector2.zero);
                if (hit.collider != null) {
                    Debug.Log(hit.collider.gameObject.name);
                    GameObject go = hit.collider.gameObject;
                    if (go.CompareTag("Person"))
                    {
                        Debug.Log("CLicked on person");
                        Person p = go.GetComponent<Person>();
                        if (p.isInfected())
                            EventCenter.GetInstance().EventTrigger<Person>("Add Patient", p);
                    }
                }
                }
        }

    }
}
