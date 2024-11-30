using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Gem: MonoBehaviour {
    public int type = 0;

    void OnPlayerContact(Player player) {
        player.OnGemCollect(type);
        Destroy(this.gameObject);
    }
}