using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Star: MonoBehaviour {
    public GameObject destroyPrefab;
    private bool active = false;
    private SpriteRenderer spriteRenderer;
    public float rhythm = 1.0f;
    public float height = 0.7f;
    private Vector3 startPosition;

    void Start() {
        spriteRenderer = GetComponent<SpriteRenderer>();
        startPosition = transform.position;
    }

    public void Activate() {
        active = true;
    }

    void Update() {
        if (active) {
            spriteRenderer.color = Color.white;
            transform.position = startPosition + Vector3.up * height * Mathf.Cos(rhythm * Time.time);
        } else {
            Color c = Color.blue;
            c.a = 0.42f;
            spriteRenderer.color = c;
        }
    }

    void OnPlayerContact(Player player) {
        if (active) {
            player.Heal(10);
            player.starCount += 1;
            player.CutJumpSpeed();
            Instantiate(destroyPrefab, transform.position, Quaternion.identity);
            Destroy(this.gameObject);
            player.state = 4;
        }
    }
}