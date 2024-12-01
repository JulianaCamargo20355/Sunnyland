using UnityEngine;
using UnityEngine.U2D;

public class CameraMovement : MonoBehaviour {
    public Vector3 offset = new Vector3(0f, 0f, -10f);
    [SerializeField] private float smoothTime = 0.25f;
    private Vector3 velocity = Vector3.zero;
    public Vector2 levelSize;
    public float cameraRange = 3.4f;
    public float scrollRange = 3.4f;
    public GameObject cameraPos;
    [SerializeField] private float n = 0.0f;
    [SerializeField] private float height = 0.0f;
    [SerializeField] private float clampPosition = 0.0f;

    [SerializeField] private Transform target;
    [SerializeField] private OppositeParallax[] parallax;
    private Vector3 newPosition;
    private PixelPerfectCamera ppc;

    void Start() {
        ppc = GetComponent<PixelPerfectCamera>();

        Vector3 targetPosition = target.position + offset;
        float targetX = targetPosition.x;
        float targetY = targetPosition.y;
        targetX = Mathf.Min(Mathf.Max(targetX, 0.0f), levelSize.x);
        targetY = Mathf.Max(Mathf.Min(targetY, 0.0f), -levelSize.y);
        targetPosition = new Vector3(targetX, targetY, targetPosition.z);
        newPosition = targetPosition;
        transform.position = targetPosition;
    }

    private void LateUpdate() {
        Vector3 targetPosition = target.position + offset;
        float targetX = targetPosition.x;
        float targetY = targetPosition.y;
        float relativePos = (targetY + scrollRange);

        height = (2.0f * cameraRange + scrollRange);
        n = Mathf.Floor(relativePos / height) + 1.0f;
        clampPosition = (n - 1) * height;

        float factor = height / scrollRange;  

        if ((n - 1.0f) * height < relativePos && relativePos < n * height - scrollRange) {
            targetY = clampPosition;
        } else {
            targetY = factor * (relativePos - 2.0f * n * cameraRange);
        }

        targetX = Mathf.Min(Mathf.Max(targetX, 0.0f), levelSize.x) + 0.5f;
        targetY = Mathf.Max(Mathf.Min(targetY, 0.0f), -levelSize.y);

        targetPosition = new Vector3(targetX, targetY, targetPosition.z);

        // Used for debugging
        if (cameraPos) {
            cameraPos.transform.position = targetPosition;
        }
        
        float pixelsPerUnit = 25.0f;

        float velocityX = (newPosition.x - targetPosition.x) / smoothTime * Time.deltaTime;
        float velocityY = (newPosition.y - targetPosition.y) / smoothTime * Time.deltaTime;
        newPosition.x -= Mathf.Clamp(velocityX, -5.9f, 5.9f); 
        newPosition.y -= Mathf.Clamp(velocityY, -5.9f, 5.9f);

        // If damping needs to be removed:
        //newPosition = targetPosition;

        transform.position = new Vector3(Mathf.Round(newPosition.x *  pixelsPerUnit) / pixelsPerUnit, 
            Mathf.Round(newPosition.y * pixelsPerUnit) / pixelsPerUnit, transform.position.z);
        //transform.position = ppc.RoundToPixel(newPosition);
        
        foreach (OppositeParallax c in parallax) {
            c.UpdatePosition(transform.position);
        }
    }

}