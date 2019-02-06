using System.Collections.Generic;
using UnityEngine;
using System.Diagnostics;
using System;
using System.Collections;

public class Pathfinding : MonoBehaviour{
    PathRequestManager requestManager;
    Grid grid;

    private void Awake() {
        grid = GetComponent<Grid>();
        requestManager = GetComponent<PathRequestManager>();
    }

    private IEnumerator findPath(Vector3 startPos, Vector3 targetPos) {
        Stopwatch sw = new Stopwatch();
        sw.Start();

        Vector2[] waypoints = new Vector2[0];
        bool pathSucces = false;

        Node startNode = grid.nodeFromWorldPoint(startPos);
        Node targetNode = grid.nodeFromWorldPoint(targetPos);

        if (startNode.walkable && targetNode.walkable) {
            Heap<Node> openSet = new Heap<Node>(grid.maxSize);
            HashSet<Node> closeSet = new HashSet<Node>();
            openSet.add(startNode);

            while (openSet.count > 0) {
                Node currentNode = openSet.removeFirst();
                closeSet.Add(currentNode);

                if (currentNode == targetNode) {
                    sw.Stop();
                    UnityEngine.Debug.Log("Path found: " + sw.ElapsedMilliseconds + " ms");
                    pathSucces = true;
                    break;
                }

                foreach (Node neighbour in grid.getNeighbours(currentNode)) {
                    if (!neighbour.walkable || closeSet.Contains(neighbour)) {
                        continue;
                    }

                    int newMovementCostToNeighbour = currentNode.gCost + getDistance(currentNode, neighbour) + neighbour.movementPenalty;
                    if (newMovementCostToNeighbour < neighbour.gCost || !openSet.contains(neighbour)) {
                        neighbour.gCost = newMovementCostToNeighbour;
                        neighbour.hCost = getDistance(neighbour, targetNode);
                        neighbour.parent = currentNode;

                        if (!openSet.contains(neighbour)) {
                            openSet.add(neighbour);
                        } else {
                            openSet.updateItem(neighbour);
                        }
                    }
                }
            }
        }
        yield return null;

        if (pathSucces) {
            waypoints = retracePath(startNode, targetNode);
        }

        requestManager.finishedProcessingPath(waypoints, pathSucces);
    }

    public void startFindPath(Vector2 startPosition, Vector2 endPosition) {
        StartCoroutine(findPath(startPosition, endPosition));
    }

    private Vector2[] retracePath(Node startNode, Node endNode) {
        List<Node> path = new List<Node>();
        Node currentNode = endNode;

        while (currentNode != startNode) {
            path.Add(currentNode);
            currentNode = currentNode.parent;
        }

        Vector2[] waypoints = simplifyPath(path);
        Array.Reverse(waypoints);
        return waypoints;

    }

    private Vector2[] simplifyPath(List<Node> path) {
        List<Vector2> waypoints = new List<Vector2>();
        Vector2 directionOld = Vector2.zero;

        for (int i = 1; i < path.Count; i++) {
            Vector2 directionNew = new Vector2(path[i - 1].gridX - path[i].gridX, path[i - 1].gridY - path[i].gridY);

            if (!directionNew.Equals(directionOld)) {
                waypoints.Add(path[i].worldPosition);
            }

            directionOld = directionNew;
        }

        return waypoints.ToArray();
    }

    private int getDistance(Node nodeA, Node nodeB) {
        int distanceX = Mathf.Abs(nodeA.gridX - nodeB.gridX);
        int distanceY = Mathf.Abs(nodeA.gridY - nodeB.gridY);

        if(distanceX > distanceY) {
            return 14 * distanceY + 10 * (distanceX - distanceY);
        }
        return 14 * distanceX + 10 * (distanceY - distanceX);
    }
}
