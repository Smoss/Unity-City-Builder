using System;
using UnityEngine;

public class Human: MonoBehaviour
{
    Qualification qualification;
    public Qualification Qualification
    {
        get { return qualification; }
    }
    int age;
    int birthday;
    Guid id;
    Occupation occupation;
    CityManager cityManager;
    public RealEstate home;
    public RealEstate workplace;
    private RealEstate destination;
    public float speed;
    CitySquare location;
    CitySquare Location
    {
        get
        {
            return location;
        }
        set
        {
            location = value;
        }
    }
    CitySquare nextLocation;
    int locationPointer;
    Route routeTo;
    Vector3 rail;
    float dist;
    float distTraveled;
    public float income;
    public float homeValue;
    public EconomicUnit Actor;
    void Start()
    {
        speed = UnityEngine.Random.value + .5f;
        //Material[] mats = GetComponents<Material>();
        //foreach(var mat in mats)
        //{

            //mat.color = new Color(1f, UnityEngine.Random.value, UnityEngine.Random.value);
        //}
        //mat. = new Color(1f, UnityEngine.Random.value, UnityEngine.Random.value);
    }
    void Update()
    {
        if(
            rail != null &&
            nextLocation != null &&
            Location != destination.CitySquare &&
            workplace != home
        )
        {
            dist = (this.transform.localPosition - (nextLocation.Offset + new Vector3(0, nextLocation.Height + .5f, 0))).magnitude;
            if (dist < .01 || distTraveled > rail.magnitude)
            {
                Location = nextLocation;
                this.transform.localPosition = nextLocation.Offset + new Vector3(0, nextLocation.Height + .5f);
                distTraveled = 0;
                if (Location == destination.CitySquare)
                {
                    if (destination == workplace)
                    {
                        destination = home;
                    }
                    else
                    {
                        destination = workplace;
                    }
                    locationPointer = 0;
                    if (nextLocation.Routes.ContainsKey(destination.CitySquare))
                    {
                        routeTo = nextLocation.Routes[destination.CitySquare];
                        getNextLocation();
                    }
                    return;
                }
                locationPointer++;
                getNextLocation();
            }
            else
            {
                Vector3 translation = rail * Time.deltaTime * speed;
                distTraveled += translation.magnitude;
                this.transform.Translate(translation);
            }
        }
    }
    void getNextLocation()
    {
        nextLocation = routeTo.Squares[locationPointer];
        rail = nextLocation.Offset - Location.Offset;
        rail.y = (nextLocation.Height - Location.Height);
    }
    public Occupation Occupation
    {
        get { return occupation; }
        set {
            if (workplace != null)
            {
                foreach (var square in home.CitySquare.Routes[workplace.CitySquare].Squares)
                {
                    square.removePassThrough(this);
                }
                foreach (var square in workplace.CitySquare.Routes[home.CitySquare].Squares)
                {
                    square.removePassThrough(this);
                }
            }
            if (value != null)
            {
                this.Actor.Income -= this.income;
                value.Employee = this;
                workplace = value.Location;
                this.setDestination();
                this.income = value.Income;
                this.Actor.Income += value.Income;
                foreach (var square in home.CitySquare.Routes[workplace.CitySquare].Squares)
                {
                    square.addPassThrough(this);
                }
                foreach (var square in workplace.CitySquare.Routes[home.CitySquare].Squares)
                {
                    square.addPassThrough(this);
                }
            }
            else
            {
                workplace = home;
                this.income = 0;
                this.Actor.Income -= occupation.Income;
            }
            occupation = value;
        }
    }
    void setDestination()
    {
        locationPointer = 0;
        destination = workplace;
        routeTo = Location.Routes[destination.CitySquare];
        nextLocation = routeTo.Squares[locationPointer];
        rail = nextLocation.Offset - Location.Offset;
        rail.y = (nextLocation.Height - Location.Height);
        distTraveled = 0;
    }

    public void init(/*int _age,*/ CityManager _cityManager, RealEstate _home, Qualification _qualification, EconomicUnit Economy)//, int _birthday)
    {
        //age = _age;
        //qualification = _qualification;
        //birthday = _birthday;
        Actor = new EconomicUnit(Economy);
        id = Guid.NewGuid();
        cityManager = _cityManager;
        this.transform.parent = cityManager.transform;
        home = _home;
        homeValue = _home.Price;
        this.transform.position = home.transform.position;
        Location = home.CitySquare;
        qualification = _qualification;
    }

    private void OnDestroy()
    {
        cityManager.humans.Remove(this);
        if(occupation != null)
        {
            occupation.Employee = null;
        }
    }
    //public 
}
