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
    float productivity;
    float avgProductivity;
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
    public float Productivity
    {
        get { return productivity; }
    }
    public float AvgProductivity
    {
        get { return avgProductivity; }
    }
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
        avgProductivity = 0;
        productivity = 0;
        occupants = new List<Human>();
        switch (type) {
            case PropertyType.Factory:
                int numOccs = 42;
                Occupations.Add(Qualification.NoHS, new List<Occupation>());
                Occupations.Add(Qualification.HS, new List<Occupation>());
                Occupations.Add(Qualification.Bachelors, new List<Occupation>());
                for (int x = 0; x < 11; x++)
                {
                    var newOcc = new Occupation(Qualification.NoHS, 40000, this);
                    Occupations[Qualification.NoHS].Add(newOcc);
                    OccupationsList.Add(newOcc);
                    productivity += 40000;
                }
                for (int x = 0; x < 7; x++)
                {
                    var newOcc = (new Occupation(Qualification.HS, 60000, this));
                    Occupations[Qualification.HS].Add(newOcc);
                    OccupationsList.Add(newOcc);
                    productivity += 60000;
                }
                for (int x = 0; x < 2; x++)
                {
                    var newOcc = (new Occupation(Qualification.Bachelors, 100000, this));
                    Occupations[Qualification.Bachelors].Add(newOcc);
                    OccupationsList.Add(newOcc);
                    productivity += 100000;
                }
                maxOccupants = 0;
                avgProductivity = productivity / numOccs;
                break;
            default:
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
