using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class ClickAccepter: MonoBehaviour
{
    public abstract void Accept(Vector2 vec, ClickMode mode, bool isLeftmouse);
}
