using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Shaker: MonoBehaviour {
    public float timer = 0.0f;
    private float startTime;
    private Vector3 randomVector;
    [SerializeField] private float range;
    [SerializeField] private float maxRange = 2.5f;

    void Update() {
        if (timer > 0.0f) {
            CreateRandomVector();
            range =  Mathf.Lerp(range, 0.0f, 1.0f / startTime);
            transform.position = randomVector * range;
            timer -= Time.deltaTime;
        }
    }

    public void Shake(float time) {
        timer = time;
        startTime = time;
        range = maxRange;
        CreateRandomVector();
    }

    void CreateRandomVector() {
        randomVector = Random.insideUnitSphere;
        randomVector = new Vector3(randomVector.x, randomVector.y, transform.position.z);
    }
}