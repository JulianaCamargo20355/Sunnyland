using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Dragon : MonoBehaviour
{
   public float moveSpeed = 2f;
    public float areaWidth = 3f;
    public float areaHeight = 3f;
    public GameObject fireballPrefab;
    public Transform firePoint;
    public float fireRate = 2f;

    private Vector3 startPosition;
    private Vector3 targetPosition;
    private float fireTimer;

    void Start()
    {
        startPosition = transform.position;
        targetPosition = GetNextPosition();
        fireTimer = fireRate;
    }

    void Update()
    {
        MoveEnemy();
        HandleFire();
    }

    void MoveEnemy()
    {
        transform.position = Vector3.MoveTowards(transform.position, targetPosition, moveSpeed * Time.deltaTime);

        if (Vector3.Distance(transform.position, targetPosition) < 0.1f)
        {
            targetPosition = GetNextPosition();
        }
    }

    Vector3 GetNextPosition()
    {
        Vector3 nextPosition = startPosition;

        if (Random.value > 0.5f)
        {
            nextPosition.x += Random.Range(-areaWidth, areaWidth);
        }
        else
        {
            nextPosition.y += Random.Range(-areaHeight, areaHeight);
        }

        nextPosition.x = Mathf.Clamp(nextPosition.x, startPosition.x - areaWidth, startPosition.x + areaWidth);
        nextPosition.y = Mathf.Clamp(nextPosition.y, startPosition.y - areaHeight, startPosition.y + areaHeight);

        return nextPosition;
    }

    void HandleFire()
    {
        fireTimer -= Time.deltaTime;

        if (fireTimer <= 0f)
        {
            Fire();
            fireTimer = fireRate;
        }
    }

    void Fire()
    {
        if (fireballPrefab != null && firePoint != null)
        {
            Instantiate(fireballPrefab, firePoint.position, Quaternion.identity);
        }
    }
}
