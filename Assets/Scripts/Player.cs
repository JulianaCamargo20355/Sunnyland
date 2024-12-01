using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

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
    public float maxFallSpeed = 7.0f;
    public float coyoteTime = 0.5f;
    public float timeBetweenBullets = 0.5f;
    public float blinkCycleSeconds = 0.07f;
    public float maxInvisibleTime = 1.0f;
    public float maxHitTime = 0.5f;
    public Vector2 wallJumpForce = new Vector2(7.0f, 7.0f);
    public Vector2 knockbackForce = new Vector2(7.0f, 7.0f);
    public float defaultGravity = 2.25f;
    public float waterGravity = 0.38f;
    public float waterGravityFactor = 0.5f;
    public float currentFactor = 1.0f;

    // Player movement
    private Rigidbody2D rigidBody;
    public bool isJumping = false;
    public bool onGround = true;
    private bool isJumpCut = false;
    private bool isFalling = false;
    private float wallJumpDirection;
    private float direction = 1.0f;
    private Vector2 motion;
    private bool canDoubleJump = false;
    private bool canShoot = true;
    private bool onWater = false;
    private float currentJumpSpeed;
    private float currentDoubleJumpSpeed;

    // Abilities
    public int projectileType = 1;
    public bool hasCrouch = true;
    public bool hasDoubleJump = false;
    public bool hasWallJump = false;
    public int abilityToGain = 0;
 
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
    private float crouchTimer;

    // Player status
    public int health = 32;
    public int maxHealth = 32;
    private int blinkType = 0;
    public bool canBeHurt = true;
    public Projectile projectilePrefab;
    public int fruitCount;
    public int gemCount;
    public int starCount;
    public int enemyCount;
    public int fruitsRemaining;
    public int enemiesDestroyed;
    public Star star;
    public bool higherInvisibilityTime = false;
    public int damageFactor = 1;

    // Rendering
    private SpriteRenderer spriteRenderer;
    private Animator animator;

    void Start() {
        rigidBody = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        animator = GetComponent<Animator>();
        if (abilityToGain > 0) {
            projectileType = abilityToGain - 2;
            if (projectileType < 0) {
                projectileType = 0;
            }
            if (projectileType > 3) {
                projectileType = 3;
            }
            if (abilityToGain > 1) {
                hasDoubleJump = true;
            }
            if (abilityToGain > 0) {
                hasWallJump = true;
            }
            if (abilityToGain == 6) {
                higherInvisibilityTime = true;
            }
        } else {
            hasDoubleJump = false;
            hasWallJump = false;
        }
        UIEnergyBars.Instance.SetValue(UIEnergyBars.EnergyBars.PlayerHealth, health / (float) maxHealth);
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
            case 5:
                CrouchUpdate();
                break;
            default:
                state = 4;
                break;
        }

        if (Input.GetKeyDown("q")) {
            Hurt(1);
        }

        if (Input.GetKeyDown("w")) {
            //onWater = true;
            state = 4;
        }

        if (Input.GetKeyDown("k")) {
            Knockback(1.0f);
        }

        if (Input.GetKeyDown("g")) {
            damageFactor = 2;
        }

        if (onWater) {
            rigidBody.gravityScale = waterGravity;
            currentJumpSpeed = jumpSpeed * waterGravityFactor;
            currentDoubleJumpSpeed = doubleJumpSpeed * waterGravityFactor;
            currentFactor = waterGravityFactor;
        } else {
            rigidBody.gravityScale = defaultGravity;
            currentJumpSpeed = jumpSpeed;
            currentDoubleJumpSpeed = doubleJumpSpeed;
            currentFactor = 1.0f;
        }

        if (isJumping && rigidBody.velocity.y < 0.0f) {
            isFalling = true;
            isJumping = false;
            isJumpCut = false;
        }

        UpdateTimers();
        UpdateSprite();

        if (enemyCount <= 0 && fruitsRemaining <= 0) {
            if (star) {
                star.Activate();
            }
        }
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
        if (crouchTimer > 0.0f) {
            crouchTimer -= Time.deltaTime;
            if (crouchTimer <= 0.0f) {
                crouchTimer = 0.0f;
            }
        }
        if (state != 2 && invisibilityTimer > 0.0f) {
            invisibilityTimer -= Time.deltaTime;
        }
        if (invisibilityTimer <= 0.0f) {
            // Refresh status 
            invisibilityTimer = 0;
            blinkType = 0;
        }
    }

    void StopAllTimers() {
        launchTimer = 0.0f;
        invisibilityTimer = 0.0f;
        wallTimer = 0.0f;
        wallLeftTimer = 0.0f;
        wallRightTimer = 0.0f;
        groundTimer = 0.0f;
        crouchTimer = 0.0f;
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
            Jump(currentJumpSpeed);
            state = 1;
            onGround = false;
            groundTimer = 0.0f;
        }

        if (!onGround) {
            state = 1;
        }

        if (Mathf.Abs(rigidBody.velocity.x) > 0.5f) {
            animator.Play("run-Animation");

            // Variable running speed timing
            animator.SetFloat("RunAnimationSpeed", (Mathf.Abs(rigidBody.velocity.x) + 0.5f) / horizontalSpeed);
        } else {
            if (invisibilityTimer > 0) {
                animator.Play("Dizzy");
            } else {
                animator.Play("foxidle");
            }
        }
    }

    void MidairUpdate() {
        HandleInput();
        CheckGround();
        CheckWallJump();

        if (groundTimer > 0.0f) {
            // Coyote jump
            if (!isJumping && Input.GetKeyDown("z")) {
                Jump(currentJumpSpeed);
                groundTimer = 0.0f;
            }
        } else {
            if (!isJumpCut && Input.GetKeyUp("z") && rigidBody.velocity.y > 0.0f) {
                isJumpCut = true;
                CutJumpSpeed();
            }

            if (hasDoubleJump) {
                if (!CanWallJump() && canDoubleJump && Input.GetKeyDown("z")) {
                    Jump(currentDoubleJumpSpeed);
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
            }

            if (rigidBody.velocity.y < 0.0f) {
                isFalling = true;
                isJumping = false;
                isJumpCut = false;
            }

            rigidBody.velocity = new Vector2(rigidBody.velocity.x, Mathf.Max(rigidBody.velocity.y, -maxFallSpeed));
        }

        if (onGround) {
            state = 0;
            isJumping = false;
            isFalling = false;
        }

        if (rigidBody.velocity.y < 0.0f) {
            animator.Play("Fall");
        } else {
            animator.Play("Jump");
        }
    }

    void WinUpdate() {
        canBeHurt = false;
        rigidBody.velocity = new Vector2(0.0f, rigidBody.velocity.y);
        if (onGround && rigidBody.velocity.y <= 0.0f && winTimer > 0.0f) {
            winTimer -= Time.deltaTime;
            if (winTimer <= 0.0f) {
                // Next scene
                winTimer = 2.0f;
                canBeHurt = true;
                SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
            }
            animator.Play("Victory");
        }
        if (!onGround) {
            if (rigidBody.velocity.y < 0.0f) {
                animator.Play("Fall");
            } else {
                animator.Play("Jump");
            }
        }
        CheckGround();
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
        float maxTimer = maxInvisibleTime;
        if (higherInvisibilityTime) {
            maxTimer = 2.0f * maxInvisibleTime;    
        };
        if (invisibilityTimer > maxTimer / 2.0f) {
            invisibilityTimer -= Time.deltaTime;
            animator.Play("Hurt");
            CancelSpeed(1.0f);
        } else {
            CheckGround();
            if (onGround) {
                state = 0;
            } else {
                state = 1;
            }
        }
    }

    void CrouchUpdate() {
        HandleInput();
        CheckGround();

        isJumping = false;
        isJumpCut = false;
        isFalling = false;
        canDoubleJump = true;
        groundTimer = coyoteTime;
        crouchTimer = coyoteTime;

        if (!isJumping && Input.GetKeyDown("z")) {
            Jump(currentJumpSpeed);
            state = 1;
            onGround = false;
            groundTimer = 0.0f;
            crouchTimer = 0.0f;
        }

        if (!onGround) {
            state = 1;
        }

        animator.Play("Crouch_Animation");
    }

    public void Jump(float velocity) {
        float force = velocity - rigidBody.velocity.y;
        if (crouchTimer > 0.0f) {
            force += 1.5f;
        }
        rigidBody.AddForce(Vector2.up * force, ForceMode2D.Impulse);
        isJumping = true;
        isFalling = false;
        isJumpCut = false;
        state = 1;
        onGround = false;
    }

    public void WallJump(float dir) {
        Vector2 force = new Vector2(wallJumpForce.x, wallJumpForce.y * currentFactor);
		force.x *= dir;
		if (Mathf.Sign(rigidBody.velocity.x) != Mathf.Sign(force.x)) {
			force.x -= rigidBody.velocity.x;
        }
		if (rigidBody.velocity.y < 0) {
			force.y -= rigidBody.velocity.y;
        }
		rigidBody.AddForce(force, ForceMode2D.Impulse);
        isJumping = false;
        isJumpCut = false;
        isFalling = false;
        state = 3;
    }

    public void Knockback(float factor) {
        Vector2 force = new Vector2(knockbackForce.x * factor, knockbackForce.y * factor * currentFactor);
		force.x *= -direction;
		if (Mathf.Sign(rigidBody.velocity.x) != Mathf.Sign(force.x)) {
			force.x -= rigidBody.velocity.x;
        }
		if (rigidBody.velocity.y < 0) {
			force.y -= rigidBody.velocity.y;
        }
        if (rigidBody.velocity.y > 0) {
			force.y -= rigidBody.velocity.y;
        }
		rigidBody.AddForce(force, ForceMode2D.Impulse);
        state = 2;
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

    void CancelSpeed(float lerpAmount) {
        float targetSpeed = Mathf.Lerp(rigidBody.velocity.x, 0.0f, lerpAmount);
        float speedDif = targetSpeed - rigidBody.velocity.x;
		float movement = speedDif * brakeSpeed;
		rigidBody.AddForce(movement * Vector2.right, ForceMode2D.Force);
    }

    public void CutJumpSpeed() {
        if (rigidBody.velocity.y > 0.0f) {
            float force = rigidBody.velocity.y * 0.5f;
            rigidBody.AddForce(Vector2.down * force, ForceMode2D.Impulse);
            isJumpCut = true;
        }
    }

    void HandleInput() {
        motion.x = Input.GetAxisRaw("Horizontal");

        if (motion.x > 0.0f) {
            direction = 1.0f;
        }

        if (motion.x < 0.0f) {
            direction = -1.0f;
        }
        
        if (state != 5) {
            Run(1);
        }

        if (canShoot) {
            if (Input.GetKey("x")) { 
                if (launchTimer <= 0.0f) {
                    launchTimer = timeBetweenBullets;     
                    if (higherInvisibilityTime) {
                        launchTimer /= 2.0f;
                    }       
                    LaunchProjectile();
                }
            }
        }

        if (motion.x == 0.0f && Input.GetKey("down") && onGround && state == 0) {
            state = 5;
        }

        if (!Input.GetKey("down") && onGround && state == 5) {
            state = 0;
        }
    }

    // Collision

    void CheckGround() {
        bool againstGround = !isJumping || rigidBody.velocity.y <= 0.0f;
        if (againstGround && Physics2D.OverlapBox(groundCheckPoint.position, groundCheckSize, 0, groundLayer)) {
            onGround = true;
            isJumping = false;
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

    public void Hurt(int damageAmount, bool stronger = false) {
        if (higherInvisibilityTime) {
            stronger = true;
            damageAmount /= 2;
        }
        if (!canBeHurt) {
            return;
        }
        if (invisibilityTimer > 0.0f) {
            return;        
        }
        health -= damageAmount / damageFactor;
        UIEnergyBars.Instance.SetValue(UIEnergyBars.EnergyBars.PlayerHealth, health / (float) maxHealth);
        if (health <= 0) {
            Kill();
            UIEnergyBars.Instance.SetValue(UIEnergyBars.EnergyBars.PlayerHealth, 0.0f);
            return;
        }
        if (!stronger) {
            Knockback(0.5f);
        } else {
            Knockback(2.0f);
        }
        SetInvisible();
    }

    public void Kill() {
        health = 0;
        this.gameObject.SetActive(false); 
    }

    void LaunchProjectile() {
        Projectile first = null;
        Projectile second = null;
        Projectile third = null;
        switch (this.projectileType) {
            case 0:
                first = Instantiate(projectilePrefab, transform.position, Quaternion.identity);
                first.SetBulletType(projectileType);
                first.bulletDirection = direction;
                break;   
            case 1: 
                first = Instantiate(projectilePrefab, transform.position, Quaternion.identity);
                first.SetBulletType(projectileType);
                first.SetFloatDirection(1.0f);
                first.bulletDirection = direction;

                second = Instantiate(projectilePrefab, transform.position, Quaternion.identity);
                second.SetBulletType(projectileType);
                second.SetFloatDirection(-1.0f);
                second.bulletDirection = direction; 
                break;   
            case 2:
                first = Instantiate(projectilePrefab, transform.position, Quaternion.identity);
                first.SetFloatDirection(1.0f);
                first.SetBulletType(projectileType);
                first.bulletDirection = direction;

                second = Instantiate(projectilePrefab, transform.position, Quaternion.identity);
                second.SetFloatDirection(-1.0f);
                second.SetBulletType(projectileType);
                second.bulletDirection = direction;
                break;
            case 3:
                first = Instantiate(projectilePrefab, transform.position, Quaternion.identity);
                first.SetFloatDirection(1.0f);
                first.SetBulletType(projectileType);
                first.bulletDirection = direction;

                second = Instantiate(projectilePrefab, transform.position, Quaternion.identity);
                second.SetFloatDirection(0.0f);
                second.SetBulletType(projectileType);
                second.bulletDirection = direction;

                third = Instantiate(projectilePrefab, transform.position, Quaternion.identity);
                third.SetFloatDirection(-1.0f);
                third.SetBulletType(projectileType);
                third.bulletDirection = direction;
                break;
            default:
                break;  
        }
    }

    public void SetInvisible() {
        if (invisibilityTimer > 0.0f) {
            invisibilityTimer = maxInvisibleTime;
            if (higherInvisibilityTime) {
                invisibilityTimer = 2.0f * maxInvisibleTime;    
            }
            return;
        }
        invisibilityTimer = maxInvisibleTime;
        if (higherInvisibilityTime) {
            invisibilityTimer = 2.0f * maxInvisibleTime;    
        }
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
            Hurt(4);
        }

        if (other.gameObject.CompareTag("Enemy")) {
            Hurt(4);
        }

        if (other.gameObject.CompareTag("InstantDeath")) {
            Kill();
        }

        if (other.gameObject.CompareTag("Gem")) {
            other.gameObject.SendMessage("OnPlayerContact", this);
        }

        if (other.gameObject.CompareTag("Fruit")) {
            other.gameObject.SendMessage("OnPlayerContact", this);
        }

        if (other.gameObject.CompareTag("Star")) {
            other.gameObject.SendMessage("OnPlayerContact", this);
        }
    }

    void OnCollisionEnter2D(Collision2D other) {
        if (other.gameObject.CompareTag("Hazard")) {
            Hurt(4);
        }

        if (other.gameObject.CompareTag("Enemy")) {
            Hurt(4);
        }

        if (other.gameObject.CompareTag("InstantDeath")) {
            Kill();
        }

        if (other.gameObject.CompareTag("Fruit")) {
            other.gameObject.SendMessage("OnPlayerContact", this);
        }

        if (other.gameObject.CompareTag("Gem")) {
            other.gameObject.SendMessage("OnPlayerContact", this);
        }

        if (other.gameObject.CompareTag("Star")) {
            other.gameObject.SendMessage("OnPlayerContact", this);
        }
    }

    public void Upgrade(int type) {
        this.projectileType = type;
        SetInvisible();
    }

    public void OnFruitCollect(int type) {
        fruitCount += 1;
        fruitsRemaining -= 1;
        if (fruitsRemaining == 0) {
            switch (abilityToGain) {
                case 0:
                    hasWallJump = true;
                    hasDoubleJump = false;
                    break;
                case 1:
                    hasWallJump = true;
                    hasDoubleJump = true;
                    break;
                case 2:
                    hasWallJump = true;
                    hasDoubleJump = true;
                    projectileType = 1;
                    break;
                case 3:
                    hasWallJump = true;
                    hasDoubleJump = true;
                    projectileType = 2;
                    break;
                case 4:
                    hasWallJump = true;
                    hasDoubleJump = true;
                    projectileType = 3;
                    break;
                case 5:
                    higherInvisibilityTime = true;
                    hasWallJump = true;
                    hasDoubleJump = true;
                    break;
                default:
                    break;
            }
            abilityToGain += 1;
        }
    }

    public void OnGemCollect(int type) {
        gemCount += 1;
        Heal(4);
    }

    public void OnEnemyDestroy() {
        enemyCount -= 1;
        enemiesDestroyed += 1;
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
