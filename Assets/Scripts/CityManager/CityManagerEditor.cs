using UnityEngine;
using System.Collections;
using UnityEditor;


[CustomEditor(typeof(CityManager))]
public class CityManagerEditor : Editor
{

    public override void OnInspectorGUI()
    {
        CityManager cityManager = (CityManager)target;

        if (DrawDefaultInspector())
        {
        }

        if (GUILayout.Button("Generate"))
        {
            cityManager.GenerateMap();
        }
    }
}