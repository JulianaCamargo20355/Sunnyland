using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TrollAttacker: MonoBehaviour {
    public Player player;
    public Eagle eagle;

    public void AttackPlayer() {
        for (int i = 0; i < 10; ++i) {
            if (Random.value < 0.3f) {
                Instantiate(eagle, transform.position + Vector3.right * i * 3f, Quaternion.identity);
            }
        }
    }
}