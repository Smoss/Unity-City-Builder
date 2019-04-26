using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;
using UnityEngine.EventSystems;

public enum PropertyType { Factory, Home, PowerPlant, Shop }
public class RealEstate : MonoBehaviour, IPointerDownHandler
{
    public Guid id;
    public PropertyType type;
    CityManager CityManager;
    public int maxOccupants;
    private float price;
    public float Price
    {
        get
        {
            return price;
        }
        set
        {
            price = value;
            buildingValue.text = value.ToString();
        }
    }
    public float pollution;
    public MeshRenderer meshRenderer;
    public Color32 highColor;
    public Color32 lowColor;
    public Color32 actualColor;
    public CitySquare CitySquare { get; private set; }
    public List<Occupation> OccupationsList { get; private set; }
    public Text buildingValue;
    public Text buildingProductivity;
    public RectTransform canvasTransform;
    private bool displaying;
    public void SetTexture(float min, float max)
    {
        if(meshRenderer != null)
        {
            actualColor = Color32.Lerp(lowColor, highColor, Mathf.InverseLerp(min, max, Price));
            meshRenderer.material.mainTexture = TextureGenerator.TextureFromColor(actualColor);
        }
    }
    public float MaxProductivity { get; private set; }
    public float EightyIncome { get; private set; }
    public float Housing { get; private set; }
    public float TheoreticalProductivity { get; private set; }
    private float payroll;
    private float oldTaxes;
    List<Human> occupants;
    public bool OpenUnits { get { return occupants.Count < maxOccupants; } }
    public Dictionary<Qualification, List<Occupation>> Occupations { get; private set; }
    public List<Human> Occupants {
        get { return occupants; }
    }
    public int AvailableJobs { get; private set; }
    public EconomicUnit Owner { get; private set; }
    public float ElectricityRequirement;
    public float MaxElectricityGeneration;
    //public float ElectricityGeneration { get { return } }
    public float ElectricityProvided;
    public float EffectiveProductivity { get; private set; }

    public void init(CityManager _cityManager, CitySquare _CitySquare, EconomicUnit _owner)
    {
        CityManager = _cityManager;
        CitySquare = _CitySquare;
        Owner = _owner;
    }
    public void updateProductivity(Occupation occupation) {
        if (occupation.Employee != null)
        {
            TheoreticalProductivity += occupation.Productivity;
            Owner.Income += occupation.Productivity;
            Owner.Expenses += occupation.Income;
            payroll += occupation.Income;
            AvailableJobs--;
        } 
        else
        {
            TheoreticalProductivity -= occupation.Productivity;
            Owner.Income -= occupation.Productivity;
            Owner.Expenses -= occupation.Income;
            payroll -= occupation.Income;
            AvailableJobs++;
        }
        if( buildingProductivity != null)
        {
            var electricityMultiplier = ElectricityRequirement == 0 ? 1 : Mathf.Pow(ElectricityProvided / (float)ElectricityRequirement, 2);
            buildingProductivity.text = Owner.NetIncome.ToString();
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        Occupations = new Dictionary<Qualification, List<Occupation>>();
        OccupationsList = new List<Occupation>();
        TheoreticalProductivity = 0;
        MaxProductivity = 0;
        occupants = new List<Human>();
        oldTaxes = 0;
        buildingProductivity.text = Owner.NetIncome.ToString();
        switch (type) {
            case PropertyType.Factory:
                {
                    Occupations.Add(Qualification.NoHS, new List<Occupation>());
                    Occupations.Add(Qualification.HS, new List<Occupation>());
                    Occupations.Add(Qualification.Bachelors, new List<Occupation>());
                    /*for (int x = 0; x < 24; x++)
                    {
                        var newOcc = new Occupation(Qualification.NoHS, 40000, this, 80000);
                        Occupations[newOcc.Requirements].Add(newOcc);
                        OccupationsList.Add(newOcc);
                        MaxProductivity += 80000;
                        AvailableJobs++;
                    }*/
                    for (int x = 0; x < 14; x++)
                    {
                        var newOcc = (new Occupation(Qualification.HS, 60000, this, 90000));
                        Occupations[newOcc.Requirements].Add(newOcc);
                        OccupationsList.Add(newOcc);
                        MaxProductivity += 90000;
                    }
                    /*for (int x = 0; x < 4; x++)
                    {
                        var newOcc = (new Occupation(Qualification.Bachelors, 100000, this, 120000));
                        Occupations[newOcc.Requirements].Add(newOcc);
                        OccupationsList.Add(newOcc);
                        Productivity += 120000;
                    }*/
                    maxOccupants = 0;
                    int numOccupations = OccupationsList.Count;
                    EightyIncome = OccupationsList[(int)(numOccupations * .8f)].Income;
                    Housing = 0;
                    break;
                }
            case PropertyType.Shop:
                {
                    break;
                }
            case PropertyType.PowerPlant:
                {
                    Occupations.Add(Qualification.NoHS, new List<Occupation>());
                    Occupations.Add(Qualification.HS, new List<Occupation>());
                    Occupations.Add(Qualification.Bachelors, new List<Occupation>());
                    for (int x = 0; x < 12; x++)
                    {
                        var newOcc = new Occupation(Qualification.HS, 60000, this, 90000);
                        Occupations[newOcc.Requirements].Add(newOcc);
                        OccupationsList.Add(newOcc);
                        MaxProductivity += newOcc.Productivity;
                        AvailableJobs++;
                    }
                    break;
                }
            default:
                MaxProductivity = 0;
                Housing = CityManager.costOfLiving * maxOccupants;
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
        Owner.Income += Price / 10;
        human.Actor.Expenses += Price / 10;
        human.transform.position = this.transform.position;
        buildingProductivity.text = Owner.NetIncome.ToString();
        return true;
    }
    public void CalculateTaxes()
    {
        this.Owner.Expenses -= oldTaxes;
        this.CityManager.Government.Income -= oldTaxes;
        float newTaxes = this.Price * CityManager.taxValue;
        this.Owner.Expenses += newTaxes;
        this.CityManager.Government.Income += oldTaxes;
        oldTaxes = newTaxes;
    }
    // Update is called once per frame
    void Update()
    {
        if (canvasTransform != null)
        {
            Vector3 camPos = Camera.main.transform.position;
            canvasTransform.LookAt(
                new Vector3(camPos.x, canvasTransform.position.y, camPos.z)
            );
            canvasTransform.Rotate(0, 180, 0);
        }
    }

    public void minimizeUI()
    {
        if (canvasTransform != null)
        {
            displaying = false;
            canvasTransform.localScale *= 0;
        }
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if (canvasTransform != null && CityManager.SelectedClickMode == ClickMode.Select)
        {
            if (eventData.button == PointerEventData.InputButton.Left)
            {
                displaying = true;
                canvasTransform.localScale = new Vector3(1, 1, 1);
            }
            else
            {
                displaying = false;
                canvasTransform.localScale *= 0;
            }
        }
        CityManager.setSelectedBuilding(this);
    }
}
