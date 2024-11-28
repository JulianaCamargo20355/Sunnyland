using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Star: MonoBehaviour {
    public GameObject destroyPrefab;

    void OnPlayerContact(Player player) {
        player.Heal(10);
        Instantiate(destroyPrefab, transform.position, Quaternion.identity);
        Destroy(this.gameObject);
    }
}