using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Node
{
    public bool isUsable;

    public GameObject shape;

    public Node(bool _isUsable,GameObject _shape)
    {
        isUsable = _isUsable;
        shape = _shape;
    }
}
