using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Enemy: MonoBehaviour {
    public int state = 0;
    public int enemyType = 0;

    // Enemy data
    public float horizontalSpeed = 6.0f;
    public float frogMiniJumpSpeed;
    public float frogBigJumpSpeed;
    public float frogTimeOnGround;
    public Player player;
    [SerializeField] private GameObject plantProjectile;
    public float rabbitForce = 2.5f;

    // Enemy movement
    private Rigidbody2D rigidBody;
    [SerializeField] private float direction = 1.0f;
    
    // Enemy collision
	[SerializeField] private Vector2 groundCheckSize = new Vector2(0.13f, 0.2f);
    [SerializeField] private LayerMask groundLayer;
    [SerializeField] private Vector3 frontWallOffset;
    [SerializeField] private Vector3 backWallOffset;
	[SerializeField] private Vector2 wallCheckSize = new Vector2(0.4f, 0.3f);

    // Timers
    private float invisibilityTimer;

    // Enemy status
    public int health = 3;
    public int maxHealth = 3;
    private int blinkType = 0;
    public bool canBeHurt = true;
    public float blinkCycleSeconds = 0.07f;
    public float maxInvisibleTime = 0.6f;
    public float gemRng = 0.4f;

    // Rendering
    private SpriteRenderer spriteRenderer;
    [SerializeField] private GameObject destroyFx;
    [SerializeField] private Gem gem;
    private Animator animator;

    void Start() {
        rigidBody = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        animator = GetComponent<Animator>();

        switch (enemyType) {
            case 0: // Frog
                StartFrog();
                break;
            case 1: // Opossum
                // Stub
                break;
            case 2: // Plant
                // Stub
                break;
            case 3: // Slime
                // Stub
                break;
            case 4: // Bunny
                StartRabbit();
                break;
            default:
                break;
        }

        player.enemyCount += 1;
    }

    void Update() {
        switch (enemyType) {
            case 0: // Frog
                UpdateFrog();
                break;
            case 1: // Opossum
                UpdateOpossum();
                break;
            case 2: // Plant
                UpdatePlant();
                break;
            case 3: // Slime
                // Stub
                break;
            case 4:
                UpdateRabbit();
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

    void StartFrog() {
        float gravity = 9.81f;
        float timeMiniJump = 2.0f * frogMiniJumpSpeed / gravity;
        float timeBigJump = 2.0f * frogBigJumpSpeed / gravity;
        float period = timeMiniJump + timeBigJump + 2.0f * frogTimeOnGround;
        InvokeRepeating("MiniJump", 0.0f, period);
        InvokeRepeating("BigJump", frogTimeOnGround + timeMiniJump, period);
    }

    void UpdateFrog() {
        if (player.transform.position.x > transform.position.x) {
            direction = 1.0f;
        } else {
            direction = -1.0f;
        }
    }

    void MiniJump() {
        rigidBody.AddForce(Vector2.up * frogMiniJumpSpeed, ForceMode2D.Impulse);
    }
    
    void BigJump() {
        rigidBody.AddForce(Vector2.up * frogBigJumpSpeed, ForceMode2D.Impulse);
    }

    void UpdateOpossum() {
        if (direction == 1.0f) {
            rigidBody.velocity = new Vector3(horizontalSpeed, rigidBody.velocity.y, 0.0f);
        } else {
            rigidBody.velocity = new Vector3(-horizontalSpeed, rigidBody.velocity.y, 0.0f);
        }
    }

    void UpdatePlant() {
        if (player.transform.position.x > transform.position.x) {
            direction = 1.0f;
        } else {
            direction = -1.0f;
        }
    }

    void StartRabbit() {
        InvokeRepeating("RabbitJump", 0.0f, 1.5f);
        direction = Random.value < 0.5 ? -1.0f : 1.0f;
    }

    void UpdateRabbit() {
        if (rigidBody.velocity.y == 0.0f && Physics2D.OverlapBox(transform.position, groundCheckSize, 0, groundLayer)) {
            rigidBody.velocity = new Vector2(0.0f, 0.0f);
            animator.Play("bunny1");
        } else {
            if (rigidBody.velocity.y > 0.0f) {
                animator.Play("BunnyJump");
            } else {
                animator.Play("BunnyFall");
            }
        }
    }

    void RabbitJump() {
        if (direction == 1.0f) {
            direction = -1.0f;
        } else {
            direction = 1.0f;
        }

        if (rigidBody.velocity.y == 0.0f && Physics2D.OverlapBox(transform.position, groundCheckSize, 0, groundLayer)) {
            float forceX = rabbitForce * direction;
            float forceY = rabbitForce;
            rigidBody.AddForce(new Vector2(forceX, forceY), ForceMode2D.Impulse);
        }
    }

    // Animator events

    void OnShoot() {
        if (!plantProjectile) {
            return;
        }
        GameObject proj = Instantiate(plantProjectile, transform.position, Quaternion.identity);
        proj.SendMessage("SetPlayer", player);
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
        if (gem && Random.value < gemRng) {
            Instantiate(gem, transform.position, Quaternion.identity);
        }
        player.OnEnemyDestroy();
        Destroy(this.gameObject);
    }

    void UpdateSprite() {
        if (direction == 1.0f) {
            spriteRenderer.flipX = false;
        } else {
            spriteRenderer.flipX = true;
        }

        if (blinkType == 0) {
            spriteRenderer.enabled = true;
            spriteRenderer.color = Color.white;
        } else {
            spriteRenderer.enabled = false;
            spriteRenderer.color = Color.red;
        }
    }

    void OnTriggerEnter2D(Collider2D other) {
        if (other.gameObject.CompareTag("UpdateDirection")) {
            direction = direction == 1.0f ? -1.0f : 1.0f;
        }
    }

    private void OnDrawGizmosSelected() {
		Gizmos.color = Color.green;
		Gizmos.DrawWireCube(transform.position, groundCheckSize);
		Gizmos.color = Color.blue;
        Gizmos.DrawWireCube(transform.position + frontWallOffset, wallCheckSize);
		Gizmos.DrawWireCube(transform.position + backWallOffset, wallCheckSize);
	}
}