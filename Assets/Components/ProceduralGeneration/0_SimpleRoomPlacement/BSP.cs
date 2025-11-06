using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;
using VTools.Grid;
using VTools.ScriptableObjectDatabase;
using VTools.Utility;

namespace Components.ProceduralGeneration.BSP
{
    [CreateAssetMenu(menuName = "Procedural Generation Method/BSP")]
    public class BSP : ProceduralGenerationMethod
    {
        [Header("Room Parameters")]
        [SerializeField] private int _maxDepth = 4;
        [SerializeField] private int _minRoomSize = 6;
        [SerializeField] private int _maxRoomSize = 15;
        [Range(0, 100)]
        [SerializeField] private int probaOrientation;

        protected override async UniTask ApplyGeneration(CancellationToken cancellationToken)
        {
            Node root = new Node(new RectInt(0, 0, Grid.Width, Grid.Lenght), RandomService);

            SplitRecursively(root, _maxDepth);

            await UniTask.Delay(GridGenerator.StepDelay, cancellationToken: cancellationToken);

            List<Node> leaves = new();
            root.GetLeaves(leaves);

            List<RectInt> rooms = new();

            foreach (var leaf in leaves)
            {
                cancellationToken.ThrowIfCancellationRequested();

                RectInt room = leaf.CreateRooms();
                rooms.Add(room);
                PlaceRoom(room);

                await UniTask.Delay(GridGenerator.StepDelay, cancellationToken: cancellationToken);
            }

            ConnectNodes(root);

            BuildGround();
        }


        private void SplitRecursively(Node node, int depth)
        {
            if (depth <= 0) return;
            node.Split(probaOrientation);

            if (!node.isLeaf())
            {
                SplitRecursively(node.Left, depth - 1);
                SplitRecursively(node.Right, depth - 1);
            }
        }

        private void ConnectNodes(Node node)
        {
            if (node == null) return;

            if (!node.isLeaf())
            {
                var leftRoom = node.Left?.GetRoomRecursive();
                var rightRoom = node.Right?.GetRoomRecursive();

                if (leftRoom.HasValue && rightRoom.HasValue)
                {
                    Vector2Int start = leftRoom.Value.GetCenter();
                    Vector2Int end = rightRoom.Value.GetCenter();

                    CreateStraightCorridor(start, end);
                }

                // Recurse down
                ConnectNodes(node.Left);
                ConnectNodes(node.Right);
            }
        }

        private void CreateStraightCorridor(Vector2Int start, Vector2Int end)
        {
            int xMin = Mathf.Min(start.x, end.x);
            int xMax = Mathf.Max(start.x, end.x);
            int yMin = Mathf.Min(start.y, end.y);
            int yMax = Mathf.Max(start.y, end.y);

            for (int x = xMin; x <= xMax; x++)
            {
                if (!Grid.TryGetCellByCoordinates(x, start.y, out var cell))
                    continue;

                AddTileToCell(cell, CORRIDOR_TILE_NAME, true);
            }

            for (int y = yMin; y <= yMax; y++)
            {
                if (!Grid.TryGetCellByCoordinates(end.x, y, out var cell))
                    continue;

                AddTileToCell(cell, CORRIDOR_TILE_NAME, true);
            }
        }

        private void BuildGround()
        {
            var groundTemplate = ScriptableObjectDatabase.GetScriptableObject<GridObjectTemplate>("Grass");

            for (int x = 0; x < Grid.Width; x++)
            {
                for (int z = 0; z < Grid.Lenght; z++)
                {
                    if (!Grid.TryGetCellByCoordinates(x, z, out var chosenCell))
                    {
                        Debug.LogError($"Unable to get cell on coordinates : ({x}, {z})");
                        continue;
                    }

                    GridGenerator.AddGridObjectToCell(chosenCell, groundTemplate, false);
                }
            }
        }
    }
}
