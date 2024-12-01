using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BossProjectile : MonoBehaviour {
    public float bulletMomentum;
    private Vector3 playerPos;
    private Player player;
    private Rigidbody2D rigidBody;

    void Start() {
        rigidBody = GetComponent<Rigidbody2D>();
        Destroy(this.gameObject, 20);
        playerPos = new Vector3(player.transform.position.x, player.transform.position.y, 0.0f);
        Vector3 aim = (playerPos - transform.position).normalized;
        rigidBody.AddForce(aim * bulletMomentum, ForceMode2D.Impulse);
        Quaternion rotation = Quaternion.LookRotation(
            playerPos - transform.position,
            transform.TransformDirection(Vector3.up)
        );
        transform.rotation = new Quaternion(0.0f, 0.0f, rotation.z , rotation.w);
    }

    public void SetPlayer(Player playerObject) {
        player = playerObject;
    }
}