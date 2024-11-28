using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ItemFeedback: MonoBehaviour {
    public float delay = 0.0f;
    
    void Start() {
        Destroy(gameObject, GetComponent<Animator>().GetCurrentAnimatorStateInfo(0).length + delay); 
    }
}