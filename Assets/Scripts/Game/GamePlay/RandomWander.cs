using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class RandomWander : MonoBehaviour
{
    private Rigidbody2D rb;
    [SerializeField] public float _speed = 50;

    public bool mainScript = false;
    
    private int _changeDirFrame;
    private int _currentFrame;
    
    // Start is called before the first frame update
    void Start()
    {
        
        rb = GetComponent<Rigidbody2D>();
        _changeDirFrame = Random.Range(100, 200);
        _currentFrame = _changeDirFrame + 1;

    }

    public void ZeroMovement()
    {
        if (rb != null)
        {
            rb.velocity = Vector2.zero;
            Destroy(rb );
        }

        
        Destroy(this);
    }


    public void Update()
    {
        if (mainScript)
        {
            Move();
        }
    }


    public void Move()
    {
        _currentFrame++;
        if (_currentFrame > _changeDirFrame)
        {
            rb.velocity = Vector2.zero;
            
            if (Random.Range(0f, 1f) < Virus.intentionToMove)
            {    
                Vector2 dir = new Vector2(Random.Range(-50, 50), Random.Range(-50, 50));
                dir.Normalize();
                rb.AddForce(dir * _speed);
            }

            _currentFrame = 0;
        }
        
        FlipSprite();
    }

    
    
    private void FlipSprite()
    {
     
        Vector3 s = this.transform.localScale;
        if (rb.velocity.x < 0)
        {
            

            this.transform.localScale = new Vector3(
                Math.Abs(s.x),
                s.y,
                s.z
            );
        }
        else
        {
            
            this.transform.localScale = new Vector3(
                -Math.Abs(s.x),
                s.y,
                s.z
            );


        }
        
    }
    
}
