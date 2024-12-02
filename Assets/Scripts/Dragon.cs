using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Dragon : MonoBehaviour {
    public int state = 1;

    // Boss data
    public int health = 32;
    public int maxHealth = 32;
    public float horizontalSpeed = 5.0f;
    public float accelerationRate = 3.0f;
    public float blinkCycleSeconds = 0.07f;
    public float maxInvisibleTime = 0.7f;
    public Vector2 aggressiveForce = new Vector2(10.0f, 4.0f);
    public float areaWidth = 6.0f;
    public float fireRate = 2f;
    public float shootRate = 0.45f;
    public float changeRate = 0.7f;
    private int shootCount = 0;
    public float hopForce = 0.5f;

    // Timers
    private float invisibilityTimer;
    private float fireTimer;
    private float timer;

    // Boss status
    public float direction = 1.0f;
    private int blinkType = 0;
    [SerializeField] private Player player;
    [SerializeField] private GameObject destroyFx;
    [SerializeField] private GameObject shaker;
    [SerializeField] private Vector3 startPosition;
    [SerializeField] private Vector3 targetPosition;

    // Boss components
    private Rigidbody2D rigidBody;
    private SpriteRenderer spriteRenderer;
    public Animator animator;
    public GameObject fireballPrefab;
    public Transform firePoint;

    void Start() {
        rigidBody = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        animator = GetComponent<Animator>();
        UIEnergyBars.Instance.SetVisibility(UIEnergyBars.EnergyBars.BossHealth, true);

        startPosition = transform.position;
        targetPosition = GetNextPosition();
        fireTimer = fireRate;

        accelerationRate = ((1.0f / Time.fixedDeltaTime) * accelerationRate) / horizontalSpeed;
    }

    void Update() {
        switch (state) {
            case 0:
                IdleState();
                break;
            case 1:
                RunState();
                break;
            case 2:
                FireState();
                break;
            default:
                break;
        }

        timer -= Time.deltaTime;
        if (timer <= 0.0f) {
            timer = changeRate * Random.value;
            state = Random.Range(0, 3);
            if (state == 2) {
                timer = 7.5f * changeRate * Random.value;
            }
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

    void IdleState() {
        animator.Play("dragon");
        Run(1.0f, 0.0f);
        fireTimer = fireRate;
    }

    void FireState() {
        animator.Play("DragonAttack");
        fireTimer -= Time.deltaTime;
        if (transform.position.y <= 0.0f) {
            rigidBody.AddForce(Vector2.up * hopForce, ForceMode2D.Impulse);
        }
        if (fireTimer <= 0.0f) {
            fireTimer = shootRate;
            GameObject fireball = Instantiate(fireballPrefab, firePoint.position, Quaternion.identity);
            fireball.SendMessage("SetPlayer", player);
            if (health < maxHealth / 4.0f) {
                fireTimer = 0.4f;
                fireball.GetComponent<SpriteRenderer>().color = Color.cyan;
            }
            shaker.SendMessage("Shake", 1.2f);
            shootCount += 1;
            if (shootCount > 7 && health >= maxHealth / 4.0f || shootCount > 12 && health < maxHealth / 4.0) {
                shootCount = 0;
                state = 1;
            }
        }
        Run(1.0f, 0.0f);
    }

    void RunState() {
        if (direction == 1.0f) {
            animator.Play("DragonWalk2");
        } else {
            animator.Play("DragonWalk");
        }

        if (transform.position.x < targetPosition.x) {
            direction = 1.0f;
        } else {
            direction = -1.0f;
        }

        Run(1.0f, 1.0f);

        if (Mathf.Abs(transform.position.x - targetPosition.x) < 0.1f) {
            targetPosition = GetNextPosition();
        }
    }

    void Run(float lerpAmount, float factor) {
        float targetSpeed = factor * direction * horizontalSpeed;
        targetSpeed = Mathf.Lerp(rigidBody.velocity.x, targetSpeed, lerpAmount);
        float speedDif = targetSpeed - rigidBody.velocity.x;
		float movement = speedDif * accelerationRate;
        Debug.Log(movement);
		rigidBody.AddForce(movement * Vector2.right, ForceMode2D.Force);
    }

    Vector3 GetNextPosition() {
        Vector3 nextPosition = startPosition;
        float displacement = Random.Range(-areaWidth, areaWidth);
        if (player.transform.position.x > transform.position.x) {
            direction = 1.0f;
            displacement = areaWidth;
        }
        nextPosition.x += displacement;
        return nextPosition;
    }

    public void Hurt(int v) {
        if (invisibilityTimer > 0.0f) {
            return;        
        }
        health -= v;
        if (UIEnergyBars.Instance) {
            UIEnergyBars.Instance.SetValue(UIEnergyBars.EnergyBars.BossHealth, health / (float) maxHealth);
        }
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
        if (UIEnergyBars.Instance) {
            UIEnergyBars.Instance.SetValue(UIEnergyBars.EnergyBars.BossHealth, 0.0f);
        }
        Instantiate(destroyFx, transform.position, Quaternion.identity);
        Destroy(this.gameObject);
        player.CutJumpSpeed();
        player.state = 4;
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
}
