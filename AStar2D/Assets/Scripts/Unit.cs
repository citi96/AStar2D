using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Unit : MonoBehaviour {
    private const float pathUpdateMoveTreshold = 0.5f;
    private const float minPathUpdateTime = 0.2f;

    public Transform target;
    public float speed = 20;
    public float turnDinstance = 5;
    public float turnSpeed = 3;
    public float stoppingDistance = 10;

    Path path;
    Quaternion initialRotation;

    private void Start() {
        initialRotation = new Quaternion(transform.rotation.x, transform.rotation.y, transform.rotation.z, transform.rotation.w);
        if(target != null) {
            StartCoroutine(updatePath(null));
        }
    }

    private void Update() {
        if (target == null && Input.GetMouseButtonDown(0)) {
            StopCoroutine(updatePath(Camera.main.ScreenToWorldPoint(Input.mousePosition)));
            StartCoroutine(updatePath(Camera.main.ScreenToWorldPoint(Input.mousePosition)));
        }
    }

    private void onPathFound(Vector2[] waypoints, bool pathSuccessful) {
        if (pathSuccessful) {
            path = new Path(waypoints, transform.position, turnDinstance, stoppingDistance);
            StopCoroutine("followPath");
            StartCoroutine("followPath");
        }
    }

    private IEnumerator updatePath(Vector3? localTarget) {
        Vector3 targetPosition;
        if(Time.timeSinceLevelLoad < 0.3f) {
            yield return new WaitForSeconds(0.3f);
        }
        
        if(localTarget != null) {
            targetPosition = (Vector3)localTarget;
        } else {
            targetPosition = target.position;
        }

        PathRequestManager.requestPath(transform.position, targetPosition, onPathFound);

        float squareMoveTreshold = pathUpdateMoveTreshold * pathUpdateMoveTreshold;
        Vector3 targetPosOld = targetPosition;

        while (target != null) {
            targetPosition = target.position;
            yield return new WaitForSeconds(minPathUpdateTime);
            if ((targetPosition - targetPosOld).sqrMagnitude > squareMoveTreshold) {
                PathRequestManager.requestPath(transform.position, targetPosition, onPathFound);
                targetPosOld = targetPosition;
            }
        }

        while (Input.GetMouseButton(0)) {
            targetPosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            yield return new WaitForSeconds(minPathUpdateTime);
            if ((targetPosition - targetPosOld).sqrMagnitude > squareMoveTreshold) {
                PathRequestManager.requestPath(transform.position, targetPosition, onPathFound);
                targetPosOld = targetPosition;
            }
        }
    }

    private IEnumerator followPath() {
        bool followingPath = true;
        int pathIndex = 0;

        if (path.lookPoints.Length > 0) {
            transform.LookAt(path.lookPoints[0]);
        }

        float speedPercent = 1;

        Quaternion currentRotation = initialRotation;
        while (true) {
            Vector2 pos2D = new Vector2(transform.position.x, transform.position.y);
            while (path.turnBoundaries.Length > 0 && path.turnBoundaries[pathIndex].hasCrossedLine(pos2D)) {
                if (pathIndex == path.finishLineIndex) {
                    followingPath = false;
                    break;
                } else {
                    pathIndex++;
                }
            }

            if (followingPath && path.lookPoints.Length > 0) {
                if (pathIndex >= path.slowDownIndex && stoppingDistance > 0) {
                    speedPercent = Mathf.Clamp01(path.turnBoundaries[path.finishLineIndex].distanceFromPoint(pos2D) / stoppingDistance);
                    if(speed < 0.01f) {
                        followingPath = false;
                    }
                }

                Vector3 myLocation = transform.position;
                Vector3 targetLocation = path.lookPoints[pathIndex];
                targetLocation.z = myLocation.z;

                Vector3 vectorToTarget = targetLocation - myLocation;

                float rotationZ = Mathf.Atan2(vectorToTarget.y, vectorToTarget.x) * Mathf.Rad2Deg;

                Quaternion targetRotation = Quaternion.Euler(0.0f, 0.0f, rotationZ);

                while (pathIndex == 0 && Mathf.Abs(Quaternion.Dot(currentRotation, targetRotation)) < 0.999f) {
                    transform.localRotation = Quaternion.Lerp(currentRotation, targetRotation, turnSpeed * 5 * Time.deltaTime);
                    currentRotation = transform.localRotation;
                    yield return null;
                }

                transform.rotation = Quaternion.Lerp(currentRotation, targetRotation, turnSpeed * Time.deltaTime);
                transform.Translate(Vector3.right * speed * speedPercent * Time.deltaTime, Space.Self);
                currentRotation = transform.rotation;
            }

            yield return null;
            initialRotation = new Quaternion(transform.rotation.x, transform.rotation.y, transform.rotation.z, transform.rotation.w);
        }
    }

    private void OnDrawGizmos() {
        if(path != null) {
            path.drawWithGizmos();
        }
    }
}
