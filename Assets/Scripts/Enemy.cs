using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Enemy: MonoBehaviour {
    public int state = 0;
    public int enemyType = 0;

    // Enemy data
    public float horizontalSpeed = 6.0f;

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
    public float maxInvisibleTime = 1.0f;

    // Rendering
    private SpriteRenderer spriteRenderer;
    [SerializeField] private GameObject destroyFx;

    void Start() {
        rigidBody = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    void Update() {
        if (invisibilityTimer > 0.0f) {
            invisibilityTimer -= Time.deltaTime;
            if (invisibilityTimer <= 0.0f) {
                invisibilityTimer = 0.0f;
                blinkType = 0;
            }
        }
        UpdateSprite();
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
		Gizmos.DrawWireCube(transform.position, groundCheckSize);
		Gizmos.color = Color.blue;
        Gizmos.DrawWireCube(transform.position + frontWallOffset, wallCheckSize);
		Gizmos.DrawWireCube(transform.position + backWallOffset, wallCheckSize);
	}
}