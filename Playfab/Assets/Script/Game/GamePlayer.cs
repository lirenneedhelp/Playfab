using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GamePlayer : MonoBehaviour
{
    //Variables
    public Vector2 upForce;
    private bool isDead;
    private bool canJump;
    private bool isJumping;

    //Components
    private Rigidbody2D rb2d;
    private Animator anim;

    //Audio Stuff
    [Header("Audio Stuff")]
    AudioSource audioSource;
    public AudioClip[] audioClips;

    public ParticleSystem PointParticleSystem;
    SpriteRenderer spriteRenderer;

    // Start is called before the first frame update
    void Start()
    {
        rb2d = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>();

        audioSource = GetComponent<AudioSource>();
        canJump = false;
        isJumping = false;
    }

    // Update is called once per frame
    void Update()
    {
        //If we are not dead, we can press
        if(!isDead)
        {
            //For Mouse, Keyboard, Touch
            if(Input.GetMouseButtonDown(0) ||
               Input.GetKeyDown(KeyCode.Space))
            {
                if (!canJump)
                    return;

                //Stop the object and push it by "upForce"
                rb2d.velocity = Vector2.zero;
                rb2d.AddForce(upForce, ForceMode2D.Impulse);
                // anim.SetTrigger("Flap");

                canJump = false;
                isJumping = true;

                anim.SetTrigger("Jump");
            }
        }

        if (isJumping)
        {
            if(rb2d.velocity.y < 0.0f)
            {
                anim.SetTrigger("Fall");
            }
        }
    }

    private void FixedUpdate()
    {
        Vector2 pos = transform.position;
        Vector2 velocity = rb2d.velocity;

        Vector2 rayOrigin = new Vector2(pos.x + 0.7f, pos.y - spriteRenderer.bounds.size.y / 2.5f);
        Vector2 rayDirection = Vector2.up;
        float rayDistance = velocity.y * Time.fixedDeltaTime;
        RaycastHit2D hit2D = Physics2D.Raycast(rayOrigin, rayDirection, rayDistance);

        if(hit2D.collider != null)
        {
            if(hit2D.collider.gameObject.tag == "Ground")
            {
                canJump = true;
                isJumping = false;
                anim.SetTrigger("Land");
            }
        }
    }

    //when an incoming collider makes contact with this object's collider (2D physics only).
    private void OnCollisionEnter2D(Collision2D collision)
    {
       
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.tag != "Obstacle")
            return;

        //Stop the object
        rb2d.velocity = Vector2.zero;
        isDead = true;
        anim.SetTrigger("Hit");

        //Set it to Game Over
        GameController.instance.GameOver();

        if (PointParticleSystem != null)
        {
            PointParticleSystem.transform.position = this.transform.position;
            PointParticleSystem.Play();
        }
    }

    public void PlaySound(string type)
    {
        /*AudioClip clip = audioClips[0];
        switch(type)
        {
            case "Point":
                clip = audioClips[0];
                break;
            case "Flap":
                clip = audioClips[1];
                break;
            case "Die":
                clip = audioClips[2];
                break;
            case "Hit":
            default:
                clip = audioClips[3];
                break;
        }
        audioSource.PlayOneShot(clip);*/
    }
}
