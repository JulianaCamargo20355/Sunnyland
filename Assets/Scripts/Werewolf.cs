using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Werewolf: MonoBehaviour {
    public int state = 0;

    public Player player;
    public float horizontalSpeed = 6.0f;
    public Vector2 aggressiveForce = new Vector2(10.0f, 4.0f);
    public float accelerationRate = 1.5f;
    private Rigidbody2D rigidBody;
    private float invisibilityTimer;
    public int health = 15;
    private int blinkType = 0;
    public float blinkCycleSeconds = 0.07f;
    public float maxInvisibleTime = 1.0f;
    public float direction = -1.0f;
    private float timer;
    private SpriteRenderer spriteRenderer;
    [SerializeField] private GameObject destroyFx;
    [SerializeField] private GameObject shaker;
    public float generalTimer;
    public Animator animator;

    void Start() {
        rigidBody = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    void Update() {
        generalTimer += Time.deltaTime;
        switch (state) {
            case 0: // Frog
                StandUpdate();
                break;
            case 1: // Opossum
                RunUpdate();
                break;
            case 2:
                AttackUpdate();
                break;
            case 3:
                JumpUpdate();
                break;
            default:
                break;
        }

        float animationTime = animator.GetCurrentAnimatorStateInfo(0).normalizedTime;
        animationTime = animationTime % 1;

        if (state != 3 && animationTime >= 24.0f / 25.0f) {
            Jump();
            state = 3;
            timer = 0.0f;
        } else if (state != 2 && animationTime >= 18.0f / 25.0f) {
            state = 2; // Attack
            timer = 0.0f;
            shaker.SendMessage("Shake", 1.2f);
        } else if (state != 1 && animationTime >= 12.0f / 25.0f) {
            state = 1;
            timer = 0.0f;
        } else if (animationTime >= 6.0f / 25.0f) {
            state = 0;
        }

        if (player.transform.position.x > transform.position.x) {
            direction = 1.0f;
        } else {
            direction = -1.0f;
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

    void StandUpdate() {
        timer += Time.deltaTime;
        Run(1.0f, 0.0f);
    }

    void RunUpdate() {
        Run(1.0f, 1.0f);
        timer += Time.deltaTime;
        float animationTime = animator.GetCurrentAnimatorStateInfo(0).normalizedTime;
        animationTime = animationTime % 1;
    }

    void AttackUpdate() {
        rigidBody.velocity = new Vector3(0.0f, rigidBody.velocity.y, 0.0f);
        timer += Time.deltaTime;
        float animationTime = animator.GetCurrentAnimatorStateInfo(0).normalizedTime;
        animationTime = animationTime % 1;
        Run(1.0f, 0.0f);
    }

    void JumpUpdate() {
        float animationTime = animator.GetCurrentAnimatorStateInfo(0).normalizedTime;
        animationTime = animationTime % 1;
        Run(1.0f, 0.0f);
    }

    void Run(float lerpAmount, float factor) {
        float targetSpeed = factor * direction * horizontalSpeed;
        targetSpeed = Mathf.Lerp(rigidBody.velocity.x, targetSpeed, lerpAmount);
        float speedDif = targetSpeed - rigidBody.velocity.x;
		float movement = speedDif * accelerationRate;
		rigidBody.AddForce(movement * Vector2.right, ForceMode2D.Force);
    }

    void Jump() {
        Vector2 force = new Vector2(aggressiveForce.x, aggressiveForce.y);
        force.x *= -direction;
		if (Mathf.Sign(rigidBody.velocity.x) != Mathf.Sign(force.x)) {
			force.x -= rigidBody.velocity.x;
        }
		if (rigidBody.velocity.y < 0) {
			force.y -= rigidBody.velocity.y;
        }
		rigidBody.AddForce(force, ForceMode2D.Impulse);
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
}