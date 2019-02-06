using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraRotation : MonoBehaviour {
    public Transform playerTransform;

    void Update() {
        Vector3 pos = playerTransform.position;
        pos.z -= 11;
        transform.position = pos;
    }
}
