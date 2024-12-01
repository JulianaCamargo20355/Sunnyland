using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Fruit: MonoBehaviour {
    public int powerUpType;
    public Player player;

    void Start() {
        player.fruitsRemaining += 1;
    }

    void OnPlayerContact(Player player) {
        player.OnFruitCollect(powerUpType);
    }
}