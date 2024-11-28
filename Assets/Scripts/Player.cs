using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/**
 * Organized this way:
    1) Player states
    2) Player movement
    3) Player collision
    4) Player status
    5) Triggers
    6) Rendering
 */

public class Player: MonoBehaviour {
    public int state = 0;

    // Player data
    public float jumpSpeed = 10.0f;
    public float doubleJumpSpeed = 7.0f;
    public float accelerationSpeed = 2.0f;
    public float brakeSpeed = 1.0f;
    public float horizontalSpeed = 6.0f;
    public float hitSpeed = 2.2f;
    public float maxFallSpeed = 7.0f;
    public float coyoteTime = 0.5f;
    public float timeBetweenBullets = 0.5f;
    public float blinkCycleSeconds = 0.07f;
    public float maxInvisibleTime = 1.0f;
    public float maxHitTime = 0.5f;
    public Vector2 wallJumpForce = new Vector2(7.0f, 7.0f);

    // Player movement
    private Rigidbody2D rigidBody;
    public bool isJumping = false;
    private bool onGround = true;
    private bool isJumpCut = false;
    private bool isFalling = false;
    private float wallJumpDirection;
    private float direction = 1.0f;
    private Vector2 motion;
    private bool canDoubleJump = false;
    private bool canShoot = true;
 
    // Collision
    [SerializeField] private Transform groundCheckPoint;
	[SerializeField] private Vector2 groundCheckSize = new Vector2(0.13f, 0.2f);
    [SerializeField] private LayerMask groundLayer;
    [SerializeField] private Transform frontWallCheckPoint;
    [SerializeField] private Transform backWallCheckPoint;
	[SerializeField] private Vector2 wallCheckSize = new Vector2(0.4f, 0.3f);

    // Timers
    private float invisibilityTimer;
    private float groundTimer;
    private float wallTimer;
    private float wallLeftTimer;
    private float wallRightTimer;
    private float launchTimer;
    [SerializeField] private float winTimer = 2.0f;

    // Player status
    public int health = 3;
    public int maxHealth = 3;
    public int projectileType = 1;
    private int blinkType = 0;
    public bool canBeHurt = true;
    public bool hasDoubleJump = true;
    public bool hasWallJump = true;
    public Projectile projectilePrefab;
    public int points;

    // Rendering
    private SpriteRenderer spriteRenderer;
    public Image[] hearts;
    public ParticleSystem deathParticleSystem;

    void Start() {
        rigidBody = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    void Update() {
        // State machine
        switch (state) {
            case 0:
                StandUpdate();
                break;
            case 1:
                MidairUpdate();
                break;
            case 2:
                HurtUpdate();
                break;
            case 3:
                WallJumpUpdate();
                break;
            case 4:
                WinUpdate();
                break;
            default:
                state = 4;
                break;
        }

        if (Input.GetKeyDown("q")) {
            state = 2;
        }

        UpdateTimers();
        UpdateSprite();
    }

    void UpdateTimers() {
        if (launchTimer > 0.0f) {
            launchTimer -= Time.deltaTime;
            if (launchTimer <= 0.0f) {
                launchTimer = 0.0f;
            }
        }
        if (groundTimer > 0.0f) {
            groundTimer -= Time.deltaTime;
            if (groundTimer <= 0.0f) {
                groundTimer = 0.0f;
            }
        }
        if (wallTimer > 0.0f) {
            wallTimer -= Time.deltaTime;
            if (wallTimer <= 0.0f) {
                wallTimer = 0.0f;
            }
        }
        if (wallLeftTimer > 0.0f) {
            wallLeftTimer -= Time.deltaTime;
            if (wallLeftTimer <= 0.0f) {
                wallLeftTimer = 0.0f;
            }
        }
        if (wallRightTimer > 0.0f) {
            wallRightTimer -= Time.deltaTime;
            if (wallRightTimer <= 0.0f) {
                wallRightTimer = 0.0f;
            }
        }
        if (state != 2 && invisibilityTimer > 0.0f) {
            invisibilityTimer -= Time.deltaTime;
            if (invisibilityTimer <= 0.0f) {
                // Refresh status 
                invisibilityTimer = 0;
                blinkType = 0;
            }
        }
    }

    void StopAllTimers() {
        launchTimer = 0.0f;
        invisibilityTimer = 0.0f;
        wallTimer = 0.0f;
        wallLeftTimer = 0.0f;
        wallRightTimer = 0.0f;
        groundTimer = 0.0f;
    }

    void StandUpdate() {
        HandleInput();
        CheckGround();

        isJumping = false;
        isJumpCut = false;
        isFalling = false;
        canDoubleJump = true;
        groundTimer = coyoteTime;

        if (!isJumping && Input.GetKeyDown("z")) {
            Jump(jumpSpeed);
            state = 1;
            onGround = false;
            groundTimer = 0.0f;
        }

        if (!onGround) {
            state = 1;
        }
    }

    void MidairUpdate() {
        HandleInput();
        CheckGround();
        CheckWallJump();

        if (groundTimer > 0.0f) {
            // Coyote jump
            if (!isJumping && Input.GetKeyDown("z")) {
                Jump(jumpSpeed);
                groundTimer = 0.0f;
            }
        } else {
            if (!isJumpCut && Input.GetKeyUp("z") && rigidBody.velocity.y > 0.0f) {
                isJumpCut = true;
                CutJumpSpeed();
            }

            if (hasDoubleJump) {
                if (!CanWallJump() && canDoubleJump && Input.GetKeyDown("z")) {
                    Jump(doubleJumpSpeed);
                    canDoubleJump = false;
                }
            }

            if (Input.GetKeyDown("z") && CanWallJump()) {
                if (wallRightTimer > 0.0f) {
                    wallJumpDirection = -1.0f;
                }
                
                if (wallLeftTimer > 0.0f) {
                    wallJumpDirection = 1.0f;
                }

                direction = wallJumpDirection;
                WallJump(wallJumpDirection);
                isJumping = false;
                isJumpCut = false;
                isFalling = false;
                state = 3;
            }

            if (rigidBody.velocity.y < 0.0f) {
                isFalling = true;
                isJumping = false;
                isJumpCut = false;
            }
        }

        if (onGround) {
            state = 0;
            isJumping = false;
            isFalling = false;
        }
    }

    void WinUpdate() {
        rigidBody.velocity = new Vector2(0.0f, rigidBody.velocity.y);
        if (winTimer > 0.0f) {
            winTimer -= Time.deltaTime;
            if (winTimer <= 0.0f) {
                // Next scene
            }
        }
    }

    void WallJumpUpdate() {
        if (rigidBody.velocity.y < 0.0f) {
            CheckGround();
            if (onGround) {
                state = 0;
                isFalling = false;
                isJumping = false;
            } else {
                state = 1;
                isFalling = true;
                isJumping = false;
            }
        }
    }

    void HurtUpdate() {
        if (direction == 1.0f) {
            rigidBody.velocity = new Vector3(-hitSpeed, rigidBody.velocity.y, 0.0f);
        } else {
            rigidBody.velocity = new Vector3(hitSpeed, rigidBody.velocity.y, 0.0f);
        }

        if (invisibilityTimer > 0.0f) {
            invisibilityTimer -= Time.deltaTime;
            if (invisibilityTimer <= 0.0f) {
                // Refresh status 
                invisibilityTimer = 0;
                blinkType = 0;
                CheckGround();
                if (onGround) {
                    state = 0;
                } else {
                    state = 1;
                }
            }
        }
    }

    void Jump(float velocity) {
        float force = velocity - rigidBody.velocity.y;
        rigidBody.AddForce(Vector2.up * force, ForceMode2D.Impulse);
        isJumping = true;
        isFalling = false;
        isJumpCut = false;
        state = 1;
        onGround = false;
    }

    void WallJump(float dir) {
        Vector2 force = new Vector2(wallJumpForce.x, wallJumpForce.y);
		force.x *= dir;
		if (Mathf.Sign(rigidBody.velocity.x) != Mathf.Sign(force.x)) {
			force.x -= rigidBody.velocity.x;
        }
		if (rigidBody.velocity.y < 0) {
			force.y -= rigidBody.velocity.y;
        }
		rigidBody.AddForce(force, ForceMode2D.Impulse);
    }

    bool CanWallJump() {
        if (!hasWallJump) {
            return false;
        }

        if (wallTimer > 0.0f && groundTimer <= 0.0f) {
            if (state == 3) {
                if (wallJumpDirection == 1.0f && wallRightTimer > 0.0f) {
                    return true;
                } else if (wallJumpDirection == -1.0f && wallLeftTimer > 0.0f) {
                    return true;
                }
                return false;
            }
            return true;
        }
        return false;
    }

    void Run(float lerpAmount) {
        float targetSpeed = motion.x * horizontalSpeed;
        targetSpeed = Mathf.Lerp(rigidBody.velocity.x, targetSpeed, lerpAmount);
        
        float accelRate;
        if (Mathf.Abs(targetSpeed) > 0.01f) {
            accelRate = accelerationSpeed;
        } else {
            accelRate = brakeSpeed;
        }

        float speedDif = targetSpeed - rigidBody.velocity.x;
		float movement = speedDif * accelRate;
		rigidBody.AddForce(movement * Vector2.right, ForceMode2D.Force);
    }

    void CutJumpSpeed() {
        float force = rigidBody.velocity.y * 0.5f;
        rigidBody.AddForce(Vector2.down * force, ForceMode2D.Impulse);
        isJumpCut = true;
    }

    void HandleInput() {
        motion.x = Input.GetAxisRaw("Horizontal");

        if (motion.x > 0.0f) {
            direction = 1.0f;
        }

        if (motion.x < 0.0f) {
            direction = -1.0f;
        }
        
        Run(1);

        if (canShoot) {
            if (Input.GetKey("x")) { 
                if (launchTimer <= 0.0f) {
                    launchTimer = timeBetweenBullets;            
                    LaunchProjectile();
                }
            }
        }
    }

    // Collision

    void CheckGround() {
        if (!isJumping && Physics2D.OverlapBox(groundCheckPoint.position, groundCheckSize, 0, groundLayer)) {
            onGround = true;
        }

        if (isJumping) {
            onGround = false;
        }

        if (!onGround && rigidBody.velocity.y < 0.0f) {
            isFalling = true;
            isJumping = false;
            isJumpCut = false;
            onGround = false;
        }
    }

    void CheckWallJump() {
        if (Physics2D.OverlapBox(frontWallCheckPoint.position, wallCheckSize, 0, groundLayer)) {
            wallRightTimer = coyoteTime;
        }

        if (Physics2D.OverlapBox(backWallCheckPoint.position, wallCheckSize, 0, groundLayer)) {
            wallLeftTimer = coyoteTime;
        }

        wallTimer = Mathf.Max(wallLeftTimer, wallRightTimer);
    }

    // Status

    public void Heal(int amount) {
        health += amount;
        if (health > maxHealth) {
            health = maxHealth;
        }
        //SetInvisible();
    }

    public void Hurt(int damageAmount) {
        if (invisibilityTimer > 0.0f) {
            return;        
        }
        health -= damageAmount;
        if (health <= 0) {
            Kill();
            return;
        }
        SetInvisible();
    }

    public void Kill() {
        health = 0;
        deathParticleSystem.gameObject.SetActive(true);
        deathParticleSystem.transform.position = new Vector3(transform.position.x, transform.position.y, -0.5f);
        deathParticleSystem.Play();
        this.gameObject.SetActive(false);
        for (int i = 0; i < hearts.Length; ++i) {
            hearts[i].enabled = false;            
        }   
    }

    void LaunchProjectile() {
        Projectile first = null;
        Projectile second = null;
        switch (this.projectileType) {
            case 0: // Default
                first = Instantiate(projectilePrefab, transform.position, Quaternion.identity);
                first.SetDefault();
                first.bulletDirection = direction;
                break;   
            case 1: // Dream 
                first = Instantiate(projectilePrefab, transform.position + Vector3.up * 0.5f, Quaternion.identity);
                second = Instantiate(projectilePrefab, transform.position + Vector3.down * 0.5f, Quaternion.identity);
                first.SetDream();
                first.bulletDirection = direction;
                second.SetDream(); 
                second.bulletDirection = direction; 
                break;   
            case 2: // Plasma
                first = Instantiate(projectilePrefab, transform.position, Quaternion.identity);
                second = Instantiate(projectilePrefab, transform.position, Quaternion.identity);
                first.SetPlasma(1.0f); // Inverted direction
                first.bulletDirection = direction;
                second.SetPlasma(-1.0f);
                second.bulletDirection = direction;
                break;
            default:
                break;  
        }
    }

    public void SetInvisible() {
        if (invisibilityTimer > 0.0f) {
            invisibilityTimer = maxInvisibleTime;
            return;
        }
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
        }
    }

    // Triggers

    void OnTriggerEnter2D(Collider2D other) {
        if (other.gameObject.CompareTag("Hazard")) {
            Hurt(1);
        }

        if (other.gameObject.CompareTag("Enemy")) {
            Hurt(1);
        }

        if (other.gameObject.CompareTag("InstantDeath")) {
            Kill();
        }

        if (other.gameObject.CompareTag("Fruit")) {
            other.gameObject.SendMessage("OnPlayerContact", this);
        }

        if (other.gameObject.CompareTag("Star")) {
            other.gameObject.SendMessage("OnPlayerContact", this);
            state = 4; // Win
        }
    }

    public void Upgrade(int type) {
        this.projectileType = type;
        SetInvisible();
    }

    // Rendering

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

    private void OnDrawGizmosSelected() {
		Gizmos.color = Color.green;
		Gizmos.DrawWireCube(groundCheckPoint.position, groundCheckSize);
		Gizmos.color = Color.blue;
        Gizmos.DrawWireCube(frontWallCheckPoint.position, wallCheckSize);
		Gizmos.DrawWireCube(backWallCheckPoint.position, wallCheckSize);
	}
}
