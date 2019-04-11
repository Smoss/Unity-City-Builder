using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public enum PropertyType { Factory, Home}
public class RealEstate : MonoBehaviour
{
    public Guid id;
    public PropertyType type;
    CityManager cityManager;
    public int maxOccupants;
    public float price;
    public float pollution;
    public MeshRenderer meshRenderer;
    public Color32 highColor;
    public Color32 lowColor;
    public Color32 actualColor;
    public CitySquare CitySquare { get; private set; }
    public List<Occupation> OccupationsList { get; private set; }
    public void SetTexture(float min, float max)
    {
        if(meshRenderer != null)
        {
            actualColor = Color32.Lerp(lowColor, highColor, Mathf.InverseLerp(min, max, price));
            meshRenderer.material.mainTexture = TextureGenerator.TextureFromColor(actualColor);
        }
    }
    public float Productivity { get; private set; }
    public float AvgProductivity { get; private set; }
    public float EightyProductivity { get; private set; }
    public float Housing { get; private set; }
    List<Human> occupants;
    public bool OpenUnits { get { return occupants.Count < maxOccupants; } }
    public Dictionary<Qualification, List<Occupation>> Occupations { get; private set; }
    public List<Human> Occupants {
        get { return occupants; }
    }
    public void init(CityManager _cityManager, CitySquare _CitySquare)
    {
        cityManager = _cityManager;
        CitySquare = _CitySquare;
    }
    // Start is called before the first frame update
    void Start()
    {
        Occupations = new Dictionary<Qualification, List<Occupation>>();
        OccupationsList = new List<Occupation>();
        AvgProductivity = 0;
        Productivity = 0;
        occupants = new List<Human>();
        switch (type) {
            case PropertyType.Factory:
                Occupations.Add(Qualification.NoHS, new List<Occupation>());
                Occupations.Add(Qualification.HS, new List<Occupation>());
                Occupations.Add(Qualification.Bachelors, new List<Occupation>());
                for (int x = 0; x < 24; x++)
                {
                    var newOcc = new Occupation(Qualification.NoHS, 40000, this);
                    Occupations[Qualification.NoHS].Add(newOcc);
                    OccupationsList.Add(newOcc);
                    Productivity += 40000;
                }
                for (int x = 0; x < 14; x++)
                {
                    var newOcc = (new Occupation(Qualification.HS, 60000, this));
                    Occupations[Qualification.HS].Add(newOcc);
                    OccupationsList.Add(newOcc);
                    Productivity += 60000;
                }
                for (int x = 0; x < 4; x++)
                {
                    var newOcc = (new Occupation(Qualification.Bachelors, 100000, this));
                    Occupations[Qualification.Bachelors].Add(newOcc);
                    OccupationsList.Add(newOcc);
                    Productivity += 100000;
                }
                maxOccupants = 0;
                int numOccupations = OccupationsList.Count;
                AvgProductivity = Productivity / numOccupations;
                EightyProductivity = OccupationsList[(int)(numOccupations * .8f)].Income;
                Housing = 0;
                break;
            default:
                Productivity = 0;
                AvgProductivity = 0;
                Housing = cityManager.costOfLiving * maxOccupants;
                break;
        }
        id = Guid.NewGuid();
    }
    
    public bool addOccupant(Human human)
    {
        if (this.occupants == null)
        {
            occupants = new List<Human>();
        }
        if (occupants.Count == maxOccupants)
        {
            return false;
        }
        occupants.Add(human);
        human.home = this;
        human.transform.position = this.transform.position;
        return true;
    }
    // Update is called once per frame
    void Update()
    {
        
    }
}
