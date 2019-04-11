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
    CitySquare location;
    CitySquare nextLocation;
    int locationPointer;
    Route routeTo;
    Vector3 rail;
    float dist;
    float distTraveled;
    public float income;
    public float homeValue;
    void Update()
    {
        if(rail != null && nextLocation != null)
        {
            dist = (this.transform.localPosition - (nextLocation.Offset + new Vector3(0, nextLocation.Height + .5f, 0))).magnitude;
            if (dist < .01 || distTraveled > rail.magnitude)
            {
                location = nextLocation;
                this.transform.localPosition = nextLocation.Offset + new Vector3(0, nextLocation.Height + .5f);
                distTraveled = 0;
                if (location == destination.CitySquare)
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
                    routeTo = nextLocation.Routes[destination.CitySquare];
                    getNextLocation();
                    return;
                }
                locationPointer++;
                getNextLocation();
            }
            else
            {
                Vector3 translation = rail * Time.deltaTime * UnityEngine.Random.value;
                distTraveled += translation.magnitude;
                this.transform.Translate(translation);
            }
        }
    }
    void getNextLocation()
    {
        nextLocation = routeTo.Squares[locationPointer];
        rail = nextLocation.Offset - location.Offset;
        rail.y = (nextLocation.Height - location.Height);
    }
    public Occupation Occupation
    {
        get { return occupation; }
        set {
            occupation = value;
            if(value != null)
            {
                value.Employee = this;
                workplace = value.Location;
                this.setDestination();
                this.income = value.Income;
            }
        }
    }
    void setDestination()
    {
        locationPointer = 0;
        destination = occupation.Location;
        routeTo = location.Routes[destination.CitySquare];
        nextLocation = routeTo.Squares[locationPointer];
        rail = nextLocation.Offset - location.Offset;
        rail.y = (nextLocation.Height - location.Height);
        distTraveled = 0;
    }

    public void init(/*int _age,*/ CityManager _cityManager, RealEstate _home, Qualification _qualification)//, int _birthday)
    {
        //age = _age;
        //qualification = _qualification;
        //birthday = _birthday;
        id = Guid.NewGuid();
        cityManager = _cityManager;
        this.transform.parent = cityManager.transform;
        home = _home;
        homeValue = _home.price;
        this.transform.position = home.transform.position;
        location = home.CitySquare;
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
