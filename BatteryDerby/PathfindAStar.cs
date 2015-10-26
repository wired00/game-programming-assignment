using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace BatteryDerby {

    public class NodeRecord {

        // Current location
        public Point position;

        // Can we walk on this tile?
        public bool movable;

        // store connections to this node tile, up, left, right, down
        public NodeRecord[] connections;

        // parent node, used to trace path back from goal to start node
        public NodeRecord parentNode;

        public bool inOpenList;

        public bool inClosedList;

        // 'crow flies' distance from the start goal node
        public float distanceToGoal;

        // distance from start
        public float costSoFar;
    }

    public class PathfindAStar {
        private NodeRecord[,] searchNodes;

        private int levelWidth;
        // The height of the map.
        private int levelHeight;

        // nodes still available
        private List<NodeRecord> open = new List<NodeRecord>();

        // nodes already searched
        private List<NodeRecord> closed = new List<NodeRecord>();

        public PathfindAStar(MapBuilder map) {
            levelWidth = map.Width;
            levelHeight = map.Height;

            InitializeSearchNodes(map);
        }

        // estimated distance between two points
        private float Heuristic(Point point1, Point point2) {
            return Math.Abs(point1.X - point2.X) +
                   Math.Abs(point1.Y - point2.Y);
        }

        private void InitializeSearchNodes(MapBuilder map) {
            searchNodes = new NodeRecord[levelWidth, levelHeight];

            for (int x = 0; x < levelWidth; x++) {
                for (int y = 0; y < levelHeight; y++) {
                    NodeRecord node = new NodeRecord();
                    node.position = new Point(x, y);

                    node.movable = map.GetIndex(x, y) == 0;

                    if (node.movable == true) {
                        node.connections = new NodeRecord[4];
                        searchNodes[x, y] = node;
                    }
                }
            }

            for (int x = 0; x < levelWidth; x++) {
                for (int y = 0; y < levelHeight; y++) {
                    NodeRecord node = searchNodes[x, y];

                    if (node == null || node.movable == false) {
                        continue;
                    }

                    // implement diagonal in assignment...
                    Point[] connections = new Point[]
                    {
                        new Point (x, y - 1), // up
                        new Point (x, y + 1), // down
                        new Point (x - 1, y), // left
                        new Point (x + 1, y), // right
                    };

                    // loop through connections
                    for (int i = 0; i < connections.Length; i++) {
                        Point position = connections[i];

                        if (position.X < 0 || position.X > levelWidth - 1 ||
                            position.Y < 0 || position.Y > levelHeight - 1) {
                            continue;
                        }

                        NodeRecord connection = searchNodes[position.X, position.Y];

                        if (connection == null || connection.movable == false) {
                            continue;
                        }

                        node.connections[i] = connection;
                    }
                }
            }
        }

        private void ResetSearchNodes() {
            open.Clear();
            closed.Clear();

            for (int x = 0; x < levelWidth; x++) {
                for (int y = 0; y < levelHeight; y++) {
                    NodeRecord node = searchNodes[x, y];

                    if (node == null) {
                        continue;
                    }

                    node.inOpenList = false;
                    node.inClosedList = false;

                    node.costSoFar = float.MaxValue;
                    node.distanceToGoal = float.MaxValue;
                }
            }
        }

        private List<Vector2> FindFinalPath(NodeRecord startNode, NodeRecord endNode) {
            closed.Add(endNode);

            NodeRecord parentTile = endNode.parentNode;

            // trace back through parents to find path
            while (parentTile != startNode) {
                closed.Add(parentTile);
                parentTile = parentTile.parentNode;
            }

            List<Vector2> localisedPath = new List<Vector2>();

            // localise path
            for (int i = closed.Count - 1; i >= 0; i--) {
                localisedPath.Add(new Vector2(closed[i].position.X * MapBuilder.TILE_SIZE,
                                          closed[i].position.Y * MapBuilder.TILE_SIZE));
            }

            return localisedPath;
        }

        private NodeRecord SmallestDistanceNode() {
            NodeRecord currentTile = open[0];

            float smallestDistanceToGoal = float.MaxValue;

            // Find the closest node to the goal.
            for (int i = 0; i < open.Count; i++) {
                if (open[i].distanceToGoal < smallestDistanceToGoal) {
                    currentTile = open[i];
                    smallestDistanceToGoal = currentTile.distanceToGoal;
                }
            }
            return currentTile;
        }

        public List<Vector2> FindPath(Point startPoint, Point endPoint) {
            if (startPoint == endPoint) {
                return new List<Vector2>();
            }

            ResetSearchNodes();

            // keep track of start and end nodes
            NodeRecord startNode = searchNodes[startPoint.X, startPoint.Y];
            NodeRecord goalNode = searchNodes[endPoint.X, endPoint.Y];

            if (startNode != null) {

                startNode.inOpenList = true;

                startNode.distanceToGoal = Heuristic(startPoint, endPoint);
                startNode.costSoFar = 0;

                open.Add(startNode);

                // loop through our open list items...
                while (open.Count > 0) {

                    // work with smallest distance node first. As per Artificial Intelligence for games chapter 4
                    NodeRecord currentNode = SmallestDistanceNode();

                    // terminate if no nodes in open list.
                    if (currentNode == null) {
                        break;
                    }

                    // if node is goal node, then build the path, we're done.
                    if (currentNode == goalNode) {
                        // Trace our path back to the start.
                        return FindFinalPath(startNode, goalNode);
                    }

                    // get all current node's connections
                    for (int i = 0; i < currentNode.connections.Length; i++) {
                        NodeRecord connection = currentNode.connections[i];

                        // check if its a movable area
                        if (connection == null || connection.movable == false) {
                            continue;
                        }

                        float distanceTraveled = currentNode.costSoFar + 1;

                        // estimate distance from this node to goal
                        float heuristic = Heuristic(connection.position, endPoint);

                        if (connection.inOpenList == false && connection.inClosedList == false) {
                            // set connection cost to what we just got
                            connection.costSoFar = distanceTraveled;
                            // set connections distance to goal
                            connection.distanceToGoal = distanceTraveled + heuristic;

                            connection.parentNode = currentNode;

                            connection.inOpenList = true;
                            open.Add(connection);
                        } else if (connection.inOpenList || connection.inClosedList) {

                            if (connection.costSoFar > distanceTraveled) {
                                connection.costSoFar = distanceTraveled;
                                connection.distanceToGoal = distanceTraveled + heuristic;

                                connection.parentNode = currentNode;
                            }
                        }
                    }

                    open.Remove(currentNode);
                    currentNode.inClosedList = true;
                }
            }

            return new List<Vector2>();
        }
    }
}
