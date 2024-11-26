using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Camera : MonoBehaviour
{

    public MeshRenderer mr;

    public Transform target; // personagem
    public Vector3 offset;   //  câmera em relação ao personagem
    public float speed = 0.125f; // Velocidade 

    void LateUpdate()
    {
        if (target != null)
        {
            Vector3 desiredPosition = target.position + offset;

            Vector3 smoothedPosition = Vector3.Lerp(transform.position, desiredPosition, speed);

            transform.position = smoothedPosition;
        }
    }
}
