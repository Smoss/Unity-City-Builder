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
    public void init(CityManager _cityManager)
    {
        cityManager = _cityManager;
    }
    // Start is called before the first frame update
    void Start()
    {
        occupants = new List<Human>();
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
                maxOccupants = 5;
                break;
        }
        id = Guid.NewGuid();
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
