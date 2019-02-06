using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Path{
    public readonly Vector2[] lookPoints;
    public readonly Line[] turnBoundaries;
    public readonly int finishLineIndex;
    public readonly int slowDownIndex;

    public Path(Vector2[] waypoints, Vector2 startPosition, float turnDinstance, float stoppingDistance) {
        lookPoints = waypoints;
        turnBoundaries = new Line[lookPoints.Length];
        finishLineIndex = turnBoundaries.Length - 1;

        Vector2 previousPoint = startPosition;
        for (int i = 0; i < lookPoints.Length; i++) {
            Vector2 currentPoint = lookPoints[i];
            Vector2 dirToCurrentPoint = (currentPoint - previousPoint).normalized;
            Vector2 turnBoundaryPoints = (i == finishLineIndex) ? currentPoint : currentPoint - dirToCurrentPoint * turnDinstance;
            turnBoundaries[i] = new Line(turnBoundaryPoints, previousPoint - dirToCurrentPoint * turnDinstance);
            previousPoint = turnBoundaryPoints;
        }

        float distanceFromEndPoint = 0;

        for (int i = lookPoints.Length - 1; i > 0; i--) {
            distanceFromEndPoint += Vector2.Distance(lookPoints[i], lookPoints[i - 1]);
            if(distanceFromEndPoint > stoppingDistance) {
                slowDownIndex = i;
                break;
            }
        }
    }

    public void drawWithGizmos() {
        Gizmos.color = Color.black;
        foreach (Vector2 p in lookPoints) {
            Gizmos.DrawCube(p, Vector3.one);
        }

        Gizmos.color = Color.white;
        foreach (Line l in turnBoundaries) {
            l.drawWithGizmos(10);
        }
    }
}
