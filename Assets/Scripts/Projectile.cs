using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Projectile : MonoBehaviour {
    public Rigidbody2D rigidBody;
    public float bulletSpeed;
    public Color[] colors;
    public int bulletType;
    public float direction;
    public float bulletDirection;
    public float dreamHeight = 7.5f;
    private float damageAmount = 1;
    [SerializeField] private float launchSpeed = 4.5f;
    private SpriteRenderer spriteRenderer;
    private float timer;
    private bool colorSet = false;

    void Start() {
        spriteRenderer = GetComponent<SpriteRenderer>();
        rigidBody = GetComponent<Rigidbody2D>(); 
        Destroy(this.gameObject, 20);
        if (bulletType != 0 && !colorSet) {
            spriteRenderer.color = colors[bulletType];
            colorSet = true;
        }
    }

    void Update() {   
        timer += Time.deltaTime;      
        switch (bulletType) {
            case 0:
                rigidBody.velocity = new Vector2(bulletDirection * bulletSpeed, 0.0f);   
                break;
            case 1:
                rigidBody.velocity = new Vector2(bulletDirection * (bulletSpeed + 0.5f), direction * launchSpeed);   
                break;
            case 2:
                rigidBody.velocity = new Vector2(bulletDirection * (bulletSpeed + 1.0f), direction * dreamHeight * Mathf.Cos(20 * timer));
                break;
            case 3:
                rigidBody.velocity = new Vector2(bulletDirection * (bulletSpeed + 1.0f), direction * dreamHeight * Mathf.Cos(20 * timer));
                break;
            default:
                break;
        }
        launchSpeed = Mathf.Lerp(launchSpeed, 0.0f, 4.5f * Time.deltaTime);
        if (!colorSet) {
            spriteRenderer.color = colors[bulletType];
            colorSet = true;
        }
        SetDamage(1.0f + 1.0f * bulletType);
    }

    public void SetBulletType(int type) {
        bulletType = type;
    }

    public void SetDamage(float damage) {
        damageAmount = damage;
    }

    public void SetFloatDirection(float dir) {
        direction = dir;
    }

    void OnTriggerEnter2D(Collider2D other) {
        if (other.gameObject.CompareTag("Enemy")) {
            other.gameObject.SendMessage("Hurt", damageAmount);
            Destroy(this.gameObject);
        }
    }
}
