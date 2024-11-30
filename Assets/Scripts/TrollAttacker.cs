using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TrollAttacker: MonoBehaviour {
    public Player player;
    public Eagle eagle;

    public void AttackPlayer() {
        for (int i = 0; i < 9; ++i) {
            if (Random.value < 0.15f) {
                Instantiate(eagle, transform.position + Vector3.right * i * 4f, Quaternion.identity);
            }
        }
    }
}