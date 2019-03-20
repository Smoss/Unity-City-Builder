using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CityManagerClicker : ClickAccepter
{
    public CityManager City;
    public override void Accept(Vector2 vec)
    {
        City.Accept(vec);
    }
}
