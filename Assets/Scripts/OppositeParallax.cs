using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OppositeParallax : MonoBehaviour {
    public void UpdatePosition(Vector3 cameraPosition) {
        transform.position = new Vector3(cameraPosition.x, transform.position.y, transform.position.z);
    }
}
