using UnityEngine;

public class Camera2 : MonoBehaviour {
    private Vector3 offset = new Vector3(0f, 0f, -10f);
    [SerializeField] private Transform target;

    private void Update() {
        transform.position = target.position + offset;
    }
}