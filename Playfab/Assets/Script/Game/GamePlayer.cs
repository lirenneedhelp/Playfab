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


    float flight_duration = 0f;

    float flightCD;

    float flightTimer;

    bool isFlight = true;

    bool Flight = false;

    bool doubleJump = false;

    int remainingDoubleJumps = 2;

    float doubleJumpCD;

    float storeDJCD;

    public float baseCooldown = 5.0f; // Initial cooldown time
    public float cooldownScaleFactor = 0.9f; // Scale factor for cooldown reduction per level

    bool startCD = false;

    // Start is called before the first frame update
    void Start()
    {
        rb2d = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>();

        audioSource = GetComponent<AudioSource>();
        canJump = false;
        isJumping = false;
        doubleJump = DataCarrier.Instance.skills[1].level > 0;
        storeDJCD = baseCooldown * Mathf.Pow(cooldownScaleFactor, DataCarrier.Instance.skills[1].level);
        doubleJumpCD = storeDJCD;

        Flight = DataCarrier.Instance.skills[2].level > 0;
        flightCD = 10f;
        flightTimer = flightCD;
        flight_duration = DataCarrier.Instance.skills[2].level;
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
                rb2d.AddForce(upForce * (1 + DataCarrier.Instance.skills[0].level * 0.1f), ForceMode2D.Impulse);
                // anim.SetTrigger("Flap");
                if (doubleJump && !startCD)
                {
                    remainingDoubleJumps--;

                    if (remainingDoubleJumps <= 0)
                    {
                        canJump = false;
                        isJumping = true;
                        startCD = true;
                    }
                }
                else
                {
                    canJump = false;
                    isJumping = true;
                }
                anim.SetTrigger("Jump");
            }
            else if (Input.GetKey(KeyCode.K))
            {
                if (Flight) // Checks if the skill is unlocked
                {
                    if (isFlight) // Checks if the skill is in cooldown
                    {
                        if (flight_duration > 0)
                        {
                            flight_duration -= Time.deltaTime;
                            if (rb2d.transform.position.y < 15f)
                                rb2d.velocity = new Vector2(rb2d.velocity.x, rb2d.velocity.y + 10 * Time.deltaTime);
                        }
                        else
                        {
                            flight_duration = DataCarrier.Instance.skills[2].level;
                            isFlight = false;
                        }
                    }
                }
            }

            if (startCD)
            {
                if (doubleJumpCD > 0)
                {
                    doubleJumpCD -= Time.deltaTime;
                }
                else
                {
                    doubleJumpCD = storeDJCD;
                    startCD = false;
                }
            }

            if (!isFlight)
            {
                if (flightTimer > 0)
                {
                    flightTimer -= Time.deltaTime;
                }
                else
                {
                    flightTimer = flightCD;
                    isFlight = true;
                }
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
                remainingDoubleJumps = 2;
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
