using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Ogre: MonoBehaviour {
    public int state = 0;

    public float horizontalSpeed = 8.5f;
    public float accelerationRate = 2.0f;
    private Rigidbody2D rigidBody;
    private float invisibilityTimer;
    public int health = 20;
    private int blinkType = 0;
    public float blinkCycleSeconds = 0.07f;
    public float maxInvisibleTime = 1.0f;
    public float direction = -1.0f;
    private SpriteRenderer spriteRenderer;
    [SerializeField] private GameObject destroyFx;
    [SerializeField] private GameObject shaker;
    public Animator animator;
    public Player player;

    void Start() {
        rigidBody = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        animator = GetComponent<Animator>();
    }

    void Update() {
        switch (state) {
            case 0:
                StandUpdate();
                break;
            case 1:
                RunUpdate();
                break;
            case 2:
                AttackUpdate();
                break;
            default:
                break;
        }

        if (player.transform.position.x > transform.position.x) {
            direction = 1.0f;
        } else {
            direction = -1.0f;
        }

        float animationTime = animator.GetCurrentAnimatorStateInfo(0).normalizedTime;
        animationTime = animationTime % 1;

        if (state != 2 && animationTime >= 13.0f / 15.0f) {
            state = 2; // Attack
            shaker.SendMessage("Shake", 1.2f);
            if (player.onGround) {
                player.Hurt(1, true);
            }
        } else if (state != 1 && animationTime >= 8.0f / 25.0f) {
            state = 1;
        } else if (state != 0) {
            state = 0;
        }

        if (invisibilityTimer > 0.0f) {
            invisibilityTimer -= Time.deltaTime;
            if (invisibilityTimer <= 0.0f) {
                invisibilityTimer = 0.0f;
                blinkType = 0;
            }
        }
        UpdateSprite();
    }

    void Run(float lerpAmount, float factor) {
        float targetSpeed = factor * direction * horizontalSpeed;
        targetSpeed = Mathf.Lerp(rigidBody.velocity.x, targetSpeed, lerpAmount);
        float speedDif = targetSpeed - rigidBody.velocity.x;
		float movement = speedDif * accelerationRate;
		rigidBody.AddForce(movement * Vector2.right, ForceMode2D.Force);
    }

    void StandUpdate() {
        Run(1.0f, 0.0f);
    }

    void RunUpdate() {
        Run(1.0f, 1.0f);
    }

    void AttackUpdate() {
        Run(1.0f, 0.0f);
    }

    public void Hurt(int v) {
        if (health < 0) {
            Kill();
            return;
        }
        if (invisibilityTimer > 0.0f) {
            return;        
        }
        health -= v;
        SetInvisible();
    }

    public void SetInvisible() {
        invisibilityTimer = maxInvisibleTime;
        blinkType = 1;
        Invoke("UpdateBlinkStatus", blinkCycleSeconds);
    }

    void UpdateBlinkStatus() {
        if (invisibilityTimer > 0) {
            // Hurting  
            if (blinkType == 1) {
                blinkType = 0;
            } else {
                blinkType = 1;
            }
            Invoke("UpdateBlinkStatus", blinkCycleSeconds);
        } else {
            blinkType = 0;
        }
    }

    public void Kill() {
        health = 0;
        Instantiate(destroyFx, transform.position, Quaternion.identity);
        Destroy(this.gameObject);
    }

    void UpdateSprite() {
        if (direction == 1.0f) {
            spriteRenderer.flipX = true;
        } else {
            spriteRenderer.flipX = false;
        }

        if (blinkType == 0) {
            spriteRenderer.enabled = true;
            spriteRenderer.color = Color.white;
        } else {
            spriteRenderer.enabled = false;
            spriteRenderer.color = Color.red;
        }
    }

    void OnCollisionEnter2D(Collision2D other) {
        if (other.gameObject.CompareTag("Ground")) {
            state = 0;
        }
    }
}