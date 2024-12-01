using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Ogre: MonoBehaviour {
    public int state = 0;

    public int health = 32;
    public int maxHealth = 32;
    public float horizontalSpeed = 8.5f;
    public float accelerationRate = 2.0f;
    public float blinkCycleSeconds = 0.07f;
    public float maxInvisibleTime = 1.0f;
    public Vector2 aggressiveForce = new Vector2(-30.0f, 15.0f);

    public float direction = -1.0f;
    private float invisibilityTimer;
    private int blinkType = 0;
    [SerializeField] private Player player;
    [SerializeField] private GameObject destroyFx;
    [SerializeField] private GameObject shaker;
    
    private SpriteRenderer spriteRenderer;
    public Animator animator;
    private Rigidbody2D rigidBody;

    void Start() {
        rigidBody = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        animator = GetComponent<Animator>();
        UIEnergyBars.Instance.SetVisibility(UIEnergyBars.EnergyBars.BossHealth, true);
        accelerationRate = ((1.0f / Time.fixedDeltaTime) * accelerationRate) / horizontalSpeed;
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

    void Jump() {
        Vector2 force = new Vector2(aggressiveForce.x, aggressiveForce.y);
        force.x *= direction; // Towards player
		if (Mathf.Sign(rigidBody.velocity.x) != Mathf.Sign(force.x)) {
			force.x -= rigidBody.velocity.x;
        }
		if (rigidBody.velocity.y < 0) {
			force.y -= rigidBody.velocity.y;
        }
		rigidBody.AddForce(force, ForceMode2D.Impulse);
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
        if (invisibilityTimer > 0.0f) {
            return;        
        }
        health -= v;
        UIEnergyBars.Instance.SetValue(UIEnergyBars.EnergyBars.BossHealth, health / (float) maxHealth);
        if (health <= 0) {
            Kill();
            return;
        }
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
        UIEnergyBars.Instance.SetValue(UIEnergyBars.EnergyBars.BossHealth, 0.0f);
        Instantiate(destroyFx, transform.position, Quaternion.identity);
        Destroy(this.gameObject);
        player.CutJumpSpeed();
        player.state = 4;
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

    // Animator events

    void OnWalk() {
        state = 1;
    }

    void OnAttack() {
        if (state != 2) {
            state = 2; // Attack
            shaker.SendMessage("Shake", 1.2f);
            if (player.onGround) {
                player.Hurt(1, true);
            }
        }
    }

    void OnAnimationFinish() {
        if (state != 0) {
            state = 0;
        }
    }

    void OnCollisionEnter2D(Collision2D other) {
        if (other.gameObject.CompareTag("Player")) {
            Hurt(4);
            Jump();
        }
    }
}