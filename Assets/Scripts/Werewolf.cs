using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Werewolf: MonoBehaviour {
    public int state = 0;

    public float horizontalSpeed = 6.0f;
    public Vector2 aggressiveForce = new Vector2(10.0f, 4.0f);
    private Rigidbody2D rigidBody;
    private float invisibilityTimer;
    public int health = 15;
    private int blinkType = 0;
    public float blinkCycleSeconds = 0.07f;
    public float maxInvisibleTime = 1.0f;
    private float timer;
    private SpriteRenderer spriteRenderer;
    [SerializeField] private GameObject destroyFx;
    public float maxStandingTime = 1.5f;
    public float maxRunningTime = 0.5f;
    public float maxAttackingTime = 0.5f;
    public float generalTimer;

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
        rigidBody.velocity = new Vector3(0.0f, rigidBody.velocity.y, 0.0f);
        timer += Time.deltaTime;
        if (timer > maxStandingTime) {
            state = 1;
            timer = 0.0f;
        }
    }

    void RunUpdate() {
        rigidBody.velocity = new Vector3(-horizontalSpeed, rigidBody.velocity.y, 0.0f);
        timer += Time.deltaTime;
        if (timer > maxRunningTime) {
            state = 2;
            timer = 0.0f;
        }
    }

    void AttackUpdate() {
        rigidBody.velocity = new Vector3(0.0f, rigidBody.velocity.y, 0.0f);
        timer += Time.deltaTime;
        if (timer > maxAttackingTime) {
            rigidBody.AddForce(aggressiveForce, ForceMode2D.Impulse);
            state = 3;
            timer = 0.0f;
        }
    }

    void JumpUpdate() {
        timer = 0.0f;
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