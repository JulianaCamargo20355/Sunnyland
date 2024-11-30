using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Fruit: MonoBehaviour {
    public int powerUpType;
    public bool healPlayer = false;
    public Player player;

    void Start() {
        player.fruitsRemaining += 1;
    }

    void OnPlayerContact(Player player) {
        player.OnFruitCollect(powerUpType);
        if (healPlayer) {
            player.Heal(1);
        }
        if (powerUpType != 0) {
            player.Upgrade(powerUpType);
        }
        Destroy(this.gameObject);
    }
}