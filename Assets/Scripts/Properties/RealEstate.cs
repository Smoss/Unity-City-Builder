using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public enum PropertyType { Factory, Home}
public class RealEstate : MonoBehaviour
{
    Dictionary<Qualification, List<Occupation>> occupations;
    public Guid id;
    public PropertyType type;
    CityManager cityManager;
    int maxOccupants;
    public float price;
    float productivity;
    float avgProductivity;
    public float pollution;
    public MeshRenderer meshRenderer;
    public Color32 highColor;
    public Color32 lowColor;
    public Color32 actualColor;
    public CitySquare CitySquare { get; private set; }
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
    public Dictionary<Qualification, List<Occupation>> Occupations
    {
        get { return occupations; }
    }
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
        occupations = new Dictionary<Qualification, List<Occupation>>();
        occupants = new List<Human>();
        avgProductivity = 0;
        productivity = 0;
        int numOccs = 12;
        switch (type) {
            case PropertyType.Factory:
                occupations.Add(Qualification.NoHS, new List<Occupation>());
                occupations.Add(Qualification.HS, new List<Occupation>());
                occupations.Add(Qualification.Bachelors, new List<Occupation>());
                for (int x = 0; x < 10; x++)
                {
                    occupations[Qualification.NoHS].Add(new Occupation(Qualification.NoHS, 40000));
                    productivity += 40000;
                }
                occupations[Qualification.HS].Add(new Occupation(Qualification.HS, 60000));
                productivity += 60000;
                occupations[Qualification.HS].Add(new Occupation(Qualification.Bachelors, 100000));
                productivity += 100000;
                maxOccupants = 0;
                avgProductivity = productivity / numOccs;
                break;
            default:
                maxOccupants = 1;
                break;
        }
        id = Guid.NewGuid();
    }
    
    public bool addOccupant(Human human)
    {
        if(occupants.Count == maxOccupants)
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
