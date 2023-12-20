using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovement : MonoBehaviour
{
    [SerializeField] float runSpeed = 10f;
    [SerializeField] float jumpSpeed = 5f;
    [SerializeField] float climbSpeed = 4f;
    [SerializeField] float startingGravity = 5f;
    [SerializeField] float climbingGravity = 0f;
    [SerializeField] Vector2 deathKick = new Vector2(10f, 10f);
    [SerializeField] GameObject bullet;
    [SerializeField] Transform gun;

    bool isAlive = true;

    Vector2 moveInput;
    Rigidbody2D myRigidbody;
    Animator myAnimator;
    CapsuleCollider2D playerCollider;
    BoxCollider2D feetCollider;
    void Start()
    {
        myRigidbody = GetComponent<Rigidbody2D>();
        myAnimator = GetComponent<Animator>();
        playerCollider = GetComponent<CapsuleCollider2D>();
        feetCollider = GetComponent<BoxCollider2D>();
        myRigidbody.gravityScale = startingGravity;
    }

    void Update()
    {
        if (!isAlive)
        {
            return;
        }
        Run();
        FlipSprite();
        ClimbLadder();
        Die();
    }

    void OnMove(InputValue value)
    {
        if (!isAlive)
        {
            return;
        }
        moveInput = value.Get<Vector2>();
        Debug.Log(moveInput);
    }

    void OnFire(InputValue value)
    {
        if (!isAlive)
        {
            return;
        }
        Instantiate(bullet, gun.position, transform.rotation);
    }

    void OnJump(InputValue value)
    {
        if (!isAlive)
        {
            return;
        }
        if (!feetCollider.IsTouchingLayers(LayerMask.GetMask("Ground")))
        {
            return;   
        }
        if (value.isPressed)
        {
            myRigidbody.velocity += new Vector2(0f, jumpSpeed);
        }

    }

    void Run()
    {
        Vector2 playerVelocity = new Vector2(moveInput.x * runSpeed, myRigidbody.velocity.y);
        myRigidbody.velocity = playerVelocity;

        bool playerHasHorizontalSpeed = Mathf.Abs(myRigidbody.velocity.x) > Mathf.Epsilon;

        myAnimator.SetBool("IsRunning", playerHasHorizontalSpeed);
    }

    void FlipSprite()
    {
        bool playerHasHorizontalSpeed = Mathf.Abs(myRigidbody.velocity.x) > Mathf.Epsilon;

        if (playerHasHorizontalSpeed)
        {
            transform.localScale = new Vector2(Mathf.Sign(myRigidbody.velocity.x), 1f);
        }
    }

    void ClimbLadder()
    {
        myAnimator.SetBool("IsClimbing", false);

        if (!feetCollider.IsTouchingLayers(LayerMask.GetMask("Climbing")))
        {
            myRigidbody.gravityScale = startingGravity;
            return;
        }

        myRigidbody.gravityScale = climbingGravity;
        Vector2 climbVelocity = new Vector2(myRigidbody.velocity.x, moveInput.y * climbSpeed);
        myRigidbody.velocity = climbVelocity;

        bool playerHasVerticalSpeed = Mathf.Abs(myRigidbody.velocity.y) > Mathf.Epsilon;

        myAnimator.SetBool("IsClimbing", playerHasVerticalSpeed && feetCollider.IsTouchingLayers(LayerMask.GetMask("Climbing")));
    }

    void Die()
    {
        if (playerCollider.IsTouchingLayers(LayerMask.GetMask("Enemies", "Hazards")))
        {
            isAlive = false;
            myAnimator.SetTrigger("Dying");
            myRigidbody.velocity = deathKick;
            playerCollider.enabled = false;
            feetCollider.enabled = false;
            FindObjectOfType<GameSession>().ProcessPlayerDeath();
        }
    }
}
