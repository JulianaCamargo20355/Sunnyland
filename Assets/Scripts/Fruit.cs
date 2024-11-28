using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Fruit: MonoBehaviour {
    public GameObject destroyPrefab;
    public int powerUpType;
    public bool healPlayer = true;

    void OnPlayerContact(Player player) {
        if (healPlayer) {
            player.Heal(1);
        }
        if (powerUpType != 0) {
            player.Upgrade(powerUpType);
        }
        if (destroyPrefab) {
            Instantiate(destroyPrefab, transform.position, Quaternion.identity);
        }
        Destroy(this.gameObject);
    }
}