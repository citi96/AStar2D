using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PathRequestManager : MonoBehaviour {
    Queue<PathRequest> pathRequestQueue = new Queue<PathRequest>();
    PathRequest currentPathRequest;

    static PathRequestManager instance;
    Pathfinding pathfinding;

    bool isProcessingPath;

    private void Awake() {
        instance = this;
        pathfinding = GetComponent<Pathfinding>();
    }

    public static void requestPath(Vector2 pathStart, Vector2 pathEnd, Action<Vector2[], bool> callback) {
        PathRequest newRequest = new PathRequest(pathStart, pathEnd, callback);
        instance.pathRequestQueue.Enqueue(newRequest);
        instance.tryProcessNext();
    }

    private void tryProcessNext() {
        if (!isProcessingPath && pathRequestQueue.Count > 0) {
            currentPathRequest = pathRequestQueue.Dequeue();
            isProcessingPath = true;
            pathfinding.startFindPath(currentPathRequest.pathStart, currentPathRequest.pathEnd);
        }
    }

    public void finishedProcessingPath(Vector2[] path, bool succes) {
        currentPathRequest.callback(path, succes);
        isProcessingPath = false;
        tryProcessNext();
    }

    struct PathRequest {
        public Vector3 pathStart;
        public Vector3 pathEnd;
        public Action<Vector2[], bool> callback;

        public PathRequest(Vector2 pathStart, Vector3 pathEnd, Action<Vector2[], bool> callback) {
            this.pathStart = pathStart;
            this.pathEnd = pathEnd;
            this.callback = callback;
        }
    }
}