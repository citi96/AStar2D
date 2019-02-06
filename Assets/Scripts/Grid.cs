using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Grid : MonoBehaviour {
    public bool displayGridGizmos;
    public LayerMask unwalkableMask;
    public Vector2 gridWorldSize;
    public float nodeRadius;
    public TerrainType[] walkableRegion;
    public int obstacleProximityPenalty = 10;

    Dictionary<int, int> walkableReagionDictionary = new Dictionary<int, int>();
    LayerMask walkableMask;
    Node[,] grid;
    float nodeDiameter;
    int gridSizeX, gridSizeY;

    int penaltyMin = int.MaxValue;
    int penaltyMax = int.MinValue;

    private void Awake() {
        //How many node will fit in the grid
        nodeDiameter = nodeRadius * 2;
        gridSizeX = Mathf.RoundToInt(gridWorldSize.x / nodeDiameter);
        gridSizeY = Mathf.RoundToInt(gridWorldSize.y / nodeDiameter);

        foreach (TerrainType region in walkableRegion) {
            walkableMask.value |= region.terrainMask.value;
            walkableReagionDictionary.Add((int)Mathf.Log(region.terrainMask.value, 2), region.terrainPenalty);
        }

        createGrid();
    }

    public int maxSize {
        get {
            return gridSizeX * gridSizeY;
        }
    }

    private void createGrid() {
        grid = new Node[gridSizeX, gridSizeY];
        Vector3 worldBottomLeft = new Vector2(transform.position.x, transform.position.y) - Vector2.right * gridWorldSize.x / 2 - Vector2.up * gridWorldSize.y / 2;

        for (int x = 0; x < gridSizeX; x++) {
            for (int y = 0; y < gridSizeY; y++) {
                Vector3 worldPoint = worldBottomLeft + Vector3.right * (x * nodeDiameter + nodeRadius) + Vector3.up * (y * nodeDiameter + nodeRadius);
                bool walkable = !Physics2D.OverlapCircle(worldPoint, nodeRadius, unwalkableMask);

                int movementPenalty = 0;

                RaycastHit2D hit = Physics2D.Raycast(worldPoint, Vector2.zero, Mathf.Infinity, walkableMask);
                if (hit && hit.collider != null) {
                    walkableReagionDictionary.TryGetValue(hit.collider.gameObject.layer, out movementPenalty);
                }

                if (!walkable) {
                    movementPenalty += obstacleProximityPenalty;
                }

                grid[x, y] = new Node(walkable, worldPoint, x, y, movementPenalty);
            }
        }

        blurPenaltyMap(3);
    }




    private void blurPenaltyMap(int blurSize) {
        int kernalSize = blurSize * 2 + 1;
        int kernalExtents = (kernalSize - 1) / 2;

        int[,] penalatyHorizontalPass = new int[gridSizeX, gridSizeY];
        int[,] penalatyVerticalPass = new int[gridSizeX, gridSizeY];

        for (int y = 0; y < gridSizeY; y++) {
            for (int x = -kernalExtents; x <= kernalExtents; x++) {
                int sampleX = Mathf.Clamp(x, 0, kernalExtents);
                penalatyHorizontalPass[0, y] += grid[sampleX, y].movementPenalty;
            }

            for (int x = 1; x < gridSizeX; x++) {
                int removeIndex = Mathf.Clamp(x - kernalExtents - 1, 0, gridSizeX);
                int addIndex = Mathf.Clamp(x + kernalExtents, 0, gridSizeX - 1);

                penalatyHorizontalPass[x, y] = penalatyHorizontalPass[x - 1, y] - grid[removeIndex, y].movementPenalty + grid[addIndex, y].movementPenalty;
            }
        }

        for (int x = 0; x < gridSizeX; x++) {
            for (int y = -kernalExtents; y <= kernalExtents; y++) {
                int sampleY = Mathf.Clamp(y, 0, kernalExtents);
                penalatyVerticalPass[x, 0] += penalatyHorizontalPass[x, sampleY];
            }

            int blurredPenalty = Mathf.RoundToInt((float)penalatyVerticalPass[x, 0] / (kernalSize * kernalSize));
            grid[x, 0].movementPenalty = blurredPenalty;

            for (int y = 1; y < gridSizeY; y++) {
                int removeIndex = Mathf.Clamp(y - kernalExtents - 1, 0, gridSizeY);
                int addIndex = Mathf.Clamp(y + kernalExtents, 0, gridSizeY - 1);

                penalatyVerticalPass[x, y] = penalatyVerticalPass[x, y - 1] - penalatyHorizontalPass[x, removeIndex] + penalatyHorizontalPass[x, addIndex];

                blurredPenalty = Mathf.RoundToInt((float)penalatyVerticalPass[x, y] / (kernalSize * kernalSize));
                grid[x, y].movementPenalty = blurredPenalty;

                if (blurredPenalty > penaltyMax) {
                    penaltyMax = blurredPenalty;
                }
                if(blurredPenalty < penaltyMin) {
                    penaltyMin = blurredPenalty;
                }
            }
        }
    }

    public List<Node> getNeighbours(Node node) {
        List<Node> neighbours = new List<Node>();

        for (int x = -1; x <= 1; x++) {
            for (int y = -1; y <= 1; y++) {
                if (x == 0 && y == 0) {
                    continue;
                }

                int checkX = node.gridX + x;
                int checkY = node.gridY + y;

                if (checkX >= 0 && checkX < gridSizeX && checkY >= 0 && checkY < gridSizeY) {
                    neighbours.Add(grid[checkX, checkY]);
                }
            }
        }

        return neighbours;
    }

    public Node nodeFromWorldPoint(Vector3 worldPosition) {
        float percentX = (worldPosition.x + gridWorldSize.x / 2) / gridWorldSize.x;
        float percentY = (worldPosition.y + gridWorldSize.y / 2) / gridWorldSize.y;
        percentX = Mathf.Clamp01(percentX);
        percentY = Mathf.Clamp01(percentY);

        int x = Mathf.RoundToInt((gridSizeX - 1) * percentX);
        int y = Mathf.RoundToInt((gridSizeY - 1) * percentY);

        return grid[x, y];
    }

    private void OnDrawGizmos() {
        Gizmos.DrawWireCube(transform.position, new Vector2(gridWorldSize.x, gridWorldSize.y));

        if (grid != null && displayGridGizmos) {
            foreach (Node node in grid) {
                Gizmos.color = Color.Lerp(Color.white, Color.black, Mathf.InverseLerp(penaltyMin, penaltyMax, node.movementPenalty));
                Gizmos.color = node.walkable ? Gizmos.color : Color.red;
                Gizmos.DrawCube(node.worldPosition, Vector2.one * nodeDiameter);
            }
        }
    }

    [System.Serializable]
    public class TerrainType {
        public LayerMask terrainMask;
        public int terrainPenalty;
    }
}
