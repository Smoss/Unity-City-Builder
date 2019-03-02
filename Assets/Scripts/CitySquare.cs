using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CitySquare
{
    float height;
    float fertility;
    public float Height
    {
        get { return height; }
    }
    public float Fertility
    {
        get { return fertility; }
    }
    public float getRequestedValue(DrawMode drawMode)
    {
        switch (drawMode)
        {
            case DrawMode.Fertility:
                return this.Fertility;
            default:
                return this.Height;
        }
    }
    public CitySquare(float _height, float _fertility)
    {
        height = _height;
        fertility = _fertility;
    }
}
