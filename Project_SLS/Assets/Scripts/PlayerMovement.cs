using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    CharacterController controller;
    Transform playerModel;
    [SerializeField] float speed;
    [SerializeField] float gravity;
    [SerializeField] float jumpHeight;
    [SerializeField] Vector3 currentVelocity;

    [SerializeField] Transform groundCheck;
    float groundDistance = 0.4f;

    [SerializeField] LayerMask groundMask;
    [SerializeField] bool isGrounded;


    //SLOW TIME TWEEN VARIABLES
    float time;
    float change;
    float startValue;
    float targetValue;
    [SerializeField] float tweenDuration;

    //SLOW TIME CHECKS
    bool slowTime;
    bool resetSlowTime;
    bool reset;

    //RUNNING
    bool isRunning;

    //ROCKET SLIDE
    bool isSliding;
    float rsTime;
    Vector3 rsChange;
    Vector3 rsStartPosition;
    Vector3 rsTargetPosition;
    [SerializeField] float rsTweenDuration;
    [SerializeField] float rsSpeed;


    //JUMPING
    Rigidbody rb;
    [SerializeField][Range(1, 20)] float jumpVelocity = 300f;
    [SerializeField] float fallMultiplier = 2.5f;
    [SerializeField] float lowJumpMultiplier = 2f;

    // Start is called before the first frame update
    void Start()
    {
        controller = GetComponent<CharacterController>();
        playerModel = GetComponent<Transform>();
        rb = GetComponent<Rigidbody>();
    }

    // Update is called once per frame
    void Update()
    {
        Jump();
        BasicMovement();
        SlowDownTime();
        RocketSliding();
    }

    void BasicMovement()
    {
        //check if player is on ground by casting an insible sphere around a point and seeing if it collides with anything
        isGrounded = Physics.CheckSphere(groundCheck.position, groundDistance, groundMask);

        //if (isGrounded && currentVelocity.y < 0f) currentVelocity.y = -2f;

        float x = Input.GetAxis("Horizontal");
        float z = Input.GetAxis("Vertical");

        //takes movement relattive to the player and mutiply it by the current input (1 or -1)
        Vector3 movement = transform.right * x + transform.forward * z;

        //multiplying by time.deltaTime makes it framerate (update) independent
        controller.Move(movement * speed * Time.deltaTime);

        if (Input.GetKeyDown(KeyCode.LeftShift)) isRunning = true;
        if (Input.GetKeyUp(KeyCode.LeftShift)) isRunning = false;

        //add gravity pull to the y axis   
        //currentVelocity.y += gravity * Time.deltaTime;
        //controller.Move(currentVelocity * Time.deltaTime);
    }

    private void Jump()
    {
        if (Input.GetKeyDown(KeyCode.Space)) rb.velocity = Vector3.up * jumpVelocity;

        if(rb.velocity.y < 0) rb.velocity += Vector3.up * Physics.gravity.y * (fallMultiplier - 1) * Time.deltaTime;
        else if (rb.velocity.y > 0 && Input.GetKey(KeyCode.Space)) rb.velocity += Vector3.up * Physics.gravity.y * (lowJumpMultiplier - 1) * Time.deltaTime; 
    }

    void SlowDownTime()
    {
        //slow down time, develop lerp algo
        if (Input.GetMouseButtonDown(1)) { slowTime = true; resetSlowTime = false; }
        if (Input.GetMouseButtonUp(1)) { resetSlowTime = true; slowTime = false; }

        if (slowTime)
        {
            if (!reset)
            {
                time = 0f;
                reset = true;
            }

            startValue = 1f;
            targetValue = 0.25f;
            change = targetValue - startValue;

            if (time <= tweenDuration)
            {
                time += Time.deltaTime;
                Time.timeScale = TweenManager.EaseOutQuad(time, startValue, change, tweenDuration);
            }
        }

        if (resetSlowTime)
        {
            if (reset)
            {
                time = 0f;
                reset = false;
            }

            startValue = 0.25f;
            targetValue = 1;
            change = targetValue - startValue;

            if (time <= tweenDuration/2)
            {
                time += Time.deltaTime;
                Time.timeScale = TweenManager.EaseInQuad(time, startValue, change, tweenDuration/2);
            }
        }
    }

    void RocketSliding()
    {
        if (isRunning)
        {
            speed = 20f;

            if (Input.GetKeyDown(KeyCode.LeftControl))
            {
                isSliding = true;
                rsStartPosition = transform.position;
                rsTargetPosition = new Vector3(transform.position.x, transform.position.y, transform.position.z * rsSpeed);
            }
        }

        if (Input.GetKeyUp(KeyCode.LeftControl)) isSliding = false;

        if (!isRunning)
        {
            speed = 12f;
        }

        if(isSliding)
        {
            playerModel.localScale = new Vector3(1, .5f, 1);
            //controller.Move(currentVelocity * 2 * Time.deltaTime);

            rsChange = rsTargetPosition - rsStartPosition;

            if (rsTime <= rsTweenDuration)
            {
                rsTime += Time.deltaTime;
                transform.position = new Vector3(transform.position.x, transform.position.y, TweenManager.EaseOutQuad(rsTime, rsStartPosition.z, rsChange.z, rsTweenDuration));
            }

            if (rsTime >= rsTweenDuration)
            {
                rsTime = 0f;
                isSliding = false;
            }
        }

        if(!isSliding)
        {
            playerModel.localScale = new Vector3(1, 1, 1);
        }
    }

}
