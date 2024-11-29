using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Dragon : MonoBehaviour
{
    public float moveSpeed = 2f;
    public float moveDistanceX = 2f;
    public float moveDistanceY = 2f;
    public GameObject fireballPrefab;
    public Transform firePoint;
    public float fireRate = 2f;

    private Vector3 startPosition;
    private Vector2 direction;
    private float fireTimer;

    void Start()
    {
        startPosition = transform.position;
        direction = Vector2.right;
        fireTimer = fireRate;
    }

    void Update()
    {
        MoveEnemy();
        HandleFire();
    }

    void MoveEnemy()
    {
        Vector3 targetPosition = startPosition;

        if (direction == Vector2.right)
            targetPosition.x += moveDistanceX;
        else if (direction == Vector2.left)
            targetPosition.x -= moveDistanceX;
        else if (direction == Vector2.up)
            targetPosition.y += moveDistanceY;
        else if (direction == Vector2.down)
            targetPosition.y -= moveDistanceY;

        transform.position = Vector3.MoveTowards(transform.position, targetPosition, moveSpeed * Time.deltaTime);

        if (Vector3.Distance(transform.position, targetPosition) < 0.1f)
        {
            if (direction == Vector2.right)
                direction = Vector2.down;
            else if (direction == Vector2.down)
                direction = Vector2.left;
            else if (direction == Vector2.left)
                direction = Vector2.up;
            else if (direction == Vector2.up)
                direction = Vector2.right;

            startPosition = transform.position;
        }
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
