using Components.ProceduralGeneration.SimpleRoomPlacement;
using NUnit.Framework.Internal;
using System.Collections.Generic;
using UnityEngine;
using VTools.RandomService;
using VTools.Utility;

public class Node 
{
    private RectInt cell;
    private Node leftCell;
    private Node rightCell;
    private RectInt room;
    private int minSize = 3;
    private readonly RandomService randomService;

    public Node Left => leftCell;
    public Node Right => rightCell;


    public Node(RectInt room, RandomService _randomService)
    {
        randomService = _randomService;
        this.cell = room;
    }

    public bool Split(int proba)
    {
        if (leftCell != null || rightCell != null) return false;

        bool orientation = ChooseOrientation(proba);

        int max = (orientation ? cell.height : cell.width) - minSize;
        if (max <= minSize) return false;

        int split = randomService.Range(minSize, max);

        if (orientation)
        {
            leftCell = new Node(new RectInt(cell.x, cell.y, cell.width, split), randomService);
            rightCell = new Node(new RectInt(cell.x, cell.y + split, cell.width, cell.height - split), randomService);
            return true;
        }
        else
        {
            leftCell = new Node(new RectInt(cell.x, cell.y, split, cell.height), randomService);
            rightCell = new Node(new RectInt(cell.x + split, cell.y, cell.width - split, cell.height), randomService);
            return true;
        }
    }


    public bool isLeaf()
    {
        return leftCell == null && rightCell == null;
    }

    private bool ChooseOrientation(int probability)
    {
        if (cell.width >= cell.height * 1.25f) return false; // vertical
        if (cell.height >= cell.width * 1.25f) return true;  // horizontal
        int randnumber = randomService.Range(0, 100);
        return (probability <= randnumber);
    }

    public RectInt CreateRooms()
    {
        int roomWidth = randomService.Range(cell.width / 2, cell.width - 2);
        int roomHeight = randomService.Range(cell.height / 2, cell.height - 2);
        int roomX = cell.x + randomService.Range(1, cell.width - roomWidth - 1);
        int roomY = cell.y + randomService.Range(1, cell.height - roomHeight - 1);
        room = new RectInt(roomX, roomY, roomWidth, roomHeight);
        return room;
    }
    public Node GetChild(Node node, bool left)
    {
        if (left)
            return node.Left;
        else
            return node.Right;
    }

    public RectInt? GetRoomRecursive()
    {
        if (room.width > 0 && room.height > 0)
            return room;

        RectInt? leftRoom = leftCell?.GetRoomRecursive();
        RectInt? rightRoom = rightCell?.GetRoomRecursive();

        if (leftRoom.HasValue) return leftRoom;
        if (rightRoom.HasValue) return rightRoom;

        return null;
    }

    public void GetLeaves(List<Node> leaves)
    {
        if (isLeaf()) leaves.Add(this);
        else
        {
            leftCell?.GetLeaves(leaves);
            rightCell?.GetLeaves(leaves);
        }
    }
}
