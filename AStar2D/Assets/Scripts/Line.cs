using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public struct Line {
    const float verticalLineGradient = 1e5f;

    private float gradient;
    private float yIntercept;       //y = mx + q --> q = yIntercept
    private Vector2 pointOnLine1;
    private Vector2 pointOnLine2;

    private float gradientPerpendicular;

    private bool approachSide;

    public Line(Vector2 pointOnLine, Vector2 pointPerpendicularToLine) {
        float dx = pointOnLine.x - pointPerpendicularToLine.x;
        float dy = pointOnLine.y - pointPerpendicularToLine.y;

        if(dx == 0) {
            gradientPerpendicular = verticalLineGradient;
        } else {
            gradientPerpendicular = dy / dx;
        }

        if (gradientPerpendicular == 0) {
            gradient = verticalLineGradient;
        } else {
            gradient = -1 / gradientPerpendicular;
        }

        yIntercept = pointOnLine.y - gradient * pointOnLine.x;
        pointOnLine1 = pointOnLine;
        pointOnLine2 = pointOnLine + new Vector2(1, gradient);

        approachSide = false;
        approachSide = getSide(pointPerpendicularToLine);
    }

    private bool getSide(Vector2 p) {
        return (p.x - pointOnLine1.x) * (pointOnLine2.y - pointOnLine1.y) > (p.y - pointOnLine1.y) * (pointOnLine2.x - pointOnLine1.x);
    }

    public bool hasCrossedLine(Vector2 p) {
        return getSide(p) != approachSide;
    }

    public float distanceFromPoint(Vector2 p) {
        float yInterceptPerpendicular = p.y - gradientPerpendicular * p.x;
        float intersectX = (yInterceptPerpendicular - yIntercept) / (gradient - gradientPerpendicular);
        float intersectY = gradient * intersectX + yIntercept;

        return Vector2.Distance(p, new Vector2(intersectX, intersectY));
    }

    public void drawWithGizmos(float lenght) {
        Vector3 lineDir = new Vector3(1, 0, gradient).normalized;
        Vector3 lineCenter = new Vector3(pointOnLine1.x, 0, pointOnLine1.y) + Vector3.up;
        Gizmos.DrawLine(lineCenter - lineDir * lenght / 2f, lineCenter + lineDir * lenght / 2f);
    }
}
