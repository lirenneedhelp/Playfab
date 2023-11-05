using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PickUp : MonoBehaviour
{
    public int score = 250;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        //If the player is colliding 
        if(collision.gameObject.tag == "Player")
        {
            GameController.instance.AddScore(score);
            Destroy(gameObject);
        }
    }
}
