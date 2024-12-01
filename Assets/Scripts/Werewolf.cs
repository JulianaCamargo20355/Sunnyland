using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Werewolf: MonoBehaviour {
    public int state = 0;

    // Boss data
    public int health = 32;
    public int maxHealth = 32;
    public float horizontalSpeed = 25.0f;
    public float accelerationRate = 7.0f;
    public float blinkCycleSeconds = 0.07f;
    public float maxInvisibleTime = 0.7f;
    public Vector2 aggressiveForce = new Vector2(10.0f, 4.0f);
    
    // Timers
    private float invisibilityTimer;
    private float timer;
    public float generalTimer;

    // Boss status
    public float direction = -1.0f;
    private int blinkType = 0;
    [SerializeField] private Player player;
    [SerializeField] private GameObject destroyFx;
    [SerializeField] private GameObject shaker;

    // Boss components
    private Rigidbody2D rigidBody;
    private SpriteRenderer spriteRenderer;
    public Animator animator;

    void Start() {
        rigidBody = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        UIEnergyBars.Instance.SetVisibility(UIEnergyBars.EnergyBars.BossHealth, true);
        accelerationRate = ((1.0f / Time.fixedDeltaTime) * accelerationRate) / horizontalSpeed;
    }

    void Update() {
        generalTimer += Time.deltaTime;
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
            case 3:
                JumpUpdate();
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

    // Animator events

    void OnJump() {
        if (state != 3) {
            Jump();
            state = 3;
            timer = 0.0f;
        }
    }

    void OnAttack() {
        if (state != 2) {
            state = 2; // Attack
            timer = 0.0f;
            shaker.SendMessage("Shake", 1.2f);
        }
    }

    void OnRun() {
        if (state != 1) {
            state = 1;
            timer = 0.0f;
        }
    }

    void OnAnimationFinish() {
        if (state != 0) {
            state = 0;
        }
    }

    void StandUpdate() {
        timer += Time.deltaTime;
        Run(1.0f, 0.0f);
    }

    void RunUpdate() {
        Run(1.0f, 1.0f);
        timer += Time.deltaTime;
    }

    void AttackUpdate() {
        rigidBody.velocity = new Vector3(0.0f, rigidBody.velocity.y, 0.0f);
        timer += Time.deltaTime;
        Run(1.0f, 0.0f);
    }

    void JumpUpdate() {
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

    void OnCollisionEnter2D(Collision2D other) {
        if (other.gameObject.CompareTag("Player")) {
            Hurt(1);
            Jump();
        }
    }
}