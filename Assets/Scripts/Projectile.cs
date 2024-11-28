using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/**
 * Projectile types
 0 = Default 
 1 = Dream
 2 = Plasma
 */

public class Projectile : MonoBehaviour {
    public Rigidbody2D rigidBody;
    public float bulletSpeed;
    public float plasmaSpeed;
    public Color defaultColor;
    public Color dreamColor;
    public Color plasmaColor;
    public int bulletType;
    public float direction;
    public float bulletDirection;
    public float dreamHeight;
    public float defaultDamageAmount = 1;
    public float dreamDamageAmount = 2;
    public float plasmaDamageAmount = 3;
    private float damageAmount = 1;
    private SpriteRenderer spriteRenderer;
    private float timer;

    void Start() {
        spriteRenderer = GetComponent<SpriteRenderer>();
        rigidBody = GetComponent<Rigidbody2D>(); 
        Destroy(this.gameObject, 20);
    }

    void Update() {   
        timer += Time.deltaTime;      
        switch (bulletType) {
            case 0:
                DefaultUpdate();
                break;
            case 1:
                DreamUpdate();
                break;
            case 2:
                PlasmaUpdate();
                break;
            default:
                break;
        }
    }

    void DefaultUpdate() {
        rigidBody.velocity = new Vector2(bulletDirection * bulletSpeed, 0.0f);   
        spriteRenderer.color = defaultColor;  
    }

    void DreamUpdate() {
        rigidBody.velocity = new Vector2(bulletDirection * bulletSpeed, 0.0f);   
        spriteRenderer.color = dreamColor;   
    }

    void PlasmaUpdate() {
        rigidBody.velocity = new Vector2(bulletDirection * plasmaSpeed, direction * dreamHeight * Mathf.Cos(20 * timer));
        spriteRenderer.color = plasmaColor;   
    }

    public void SetDefault() {
        bulletType = 0;
        SetDamage(defaultDamageAmount);
    }

    public void SetDream() {
        bulletType = 1;
        SetDamage(dreamDamageAmount);
    }

    public void SetPlasma(float dir) {
        direction = dir;
        bulletType = 2;
        SetDamage(plasmaDamageAmount);
    }

    public void SetType(int type) {
        bulletType = type;
    }

    public void SetDamage(float damage) {
        damageAmount = damage;
    }

    void OnTriggerEnter2D(Collider2D other) {
        if (other.gameObject.CompareTag("Enemy")) {
            other.gameObject.SendMessage("Hurt", damageAmount);
            Destroy(this.gameObject);
        }
    }
}
