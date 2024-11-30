using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Eagle: MonoBehaviour {
    public float startPosition;
    public float range;
    public float fallSpeed = 12f;
    public float rate = 8.0f;

    void Start() {
        startPosition = transform.position.x;
        range = 2.25f + Random.value;
        Destroy(this.gameObject, 20);
    }

    void Update() {
        float x = startPosition + range * Mathf.Cos(Time.time * rate);
        float y = transform.position.y - fallSpeed * Time.deltaTime;
        transform.position = new Vector3(x, y, transform.position.z);
    }
}