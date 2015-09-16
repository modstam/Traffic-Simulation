using UnityEngine;
using System.Collections;

public class IntPair
{

    public int x, y;

    public IntPair(int x, int y)
    {
        this.x = x;
        this.y = y;
    }

    public bool containsBoth(int a, int b)
    {
        if (x == a && y == b)
            return true;
        else if (x == b && y == a)
            return true;
        else
            return false;
    }
}
