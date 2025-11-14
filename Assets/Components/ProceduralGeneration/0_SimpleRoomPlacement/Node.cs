using System.Collections.Generic;
using UnityEngine;
using VTools.RandomService;

public class Node
{
    private RectInt cell;
    private Node leftCell;
    private Node rightCell;
    private RectInt room;
    private readonly int minSize = 3;
    private readonly RandomService randomService;

    public Node Left => leftCell;
    public Node Right => rightCell;

    public Node(RectInt room, RandomService randomService)
    {
        this.randomService = randomService;
        this.cell = room;
    }

    public bool Split(int proba)
    {
        if (leftCell != null || rightCell != null)
            return false;

        bool orientation = ChooseOrientation(proba);

        int length = orientation ? cell.height : cell.width;

        // Trop petit pour être split
        if (length < minSize * 2)
            return false;

        int min = minSize;
        int max = length - minSize;

        // Sécurité supplémentaire
        if (max <= min)
            return false;

        int split = randomService.Range(min, max);

        if (orientation)
        {
            leftCell = new Node(new RectInt(cell.x, cell.y, cell.width, split), randomService);
            rightCell = new Node(new RectInt(cell.x, cell.y + split, cell.width, cell.height - split), randomService);
        }
        else
        {
            leftCell = new Node(new RectInt(cell.x, cell.y, split, cell.height), randomService);
            rightCell = new Node(new RectInt(cell.x + split, cell.y, cell.width - split, cell.height), randomService);
        }

        return true;
    }

    public bool isLeaf() => leftCell == null && rightCell == null;

    private bool ChooseOrientation(int probability)
    {
        if (cell.width >= cell.height * 1.25f) return false;
        if (cell.height >= cell.width * 1.25f) return true;

        int rnd = randomService.Range(0, 100);
        return probability <= rnd;
    }

    public RectInt CreateRooms()
    {
        // Empêcher les tailles impossibles
        int maxWidth = Mathf.Max(3, cell.width - 2);
        int maxHeight = Mathf.Max(3, cell.height - 2);

        int minWidth = Mathf.Max(3, cell.width / 2);
        int minHeight = Mathf.Max(3, cell.height / 2);

        if (maxWidth <= minWidth) maxWidth = minWidth + 1;
        if (maxHeight <= minHeight) maxHeight = minHeight + 1;

        int roomWidth = randomService.Range(minWidth, maxWidth);
        int roomHeight = randomService.Range(minHeight, maxHeight);

        // Positions sécurisées
        int posXMax = Mathf.Max(1, cell.width - roomWidth - 1);
        int posYMax = Mathf.Max(1, cell.height - roomHeight - 1);

        int roomX = cell.x + randomService.Range(1, posXMax + 1);
        int roomY = cell.y + randomService.Range(1, posYMax + 1);

        room = new RectInt(roomX, roomY, roomWidth, roomHeight);
        return room;
    }

    public Node GetChild(Node node, bool left)
    {
        return left ? node.Left : node.Right;
    }

    public RectInt? GetRoomRecursive()
    {
        if (room.width > 0 && room.height > 0)
            return room;

        RectInt? lr = leftCell?.GetRoomRecursive();
        if (lr.HasValue) return lr;

        RectInt? rr = rightCell?.GetRoomRecursive();
        return rr;
    }

    public void GetLeaves(List<Node> leaves)
    {
        if (isLeaf())
            leaves.Add(this);
        else
        {
            leftCell?.GetLeaves(leaves);
            rightCell?.GetLeaves(leaves);
        }
    }
}
