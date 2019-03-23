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
    public Vector3 rail;
    public float dist;
    public float distTraveled;
    void Update()
    {
        dist = (this.transform.localPosition - (nextLocation.Offset + new Vector3(0, nextLocation.Height + .5f, 0))).magnitude;
        if (dist < .01 || distTraveled > rail.magnitude)
        {
            location = nextLocation;
            this.transform.localPosition = nextLocation.Offset + new Vector3(0, nextLocation.Height + .5f);
            distTraveled = 0;
            if (location == destination.CitySquare)
            {
                if(destination == workplace)
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
            Vector3 translation = rail * Time.deltaTime * 5;
            distTraveled += translation.magnitude;
            this.transform.Translate(translation);
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
            value.Employee = this;
        }
    }
    public void init(/*int _age,*/ CityManager _cityManager, RealEstate _home, RealEstate _workplace)//, Qualification _qualification, int _birthday)
    {
        //age = _age;
        //qualification = _qualification;
        //birthday = _birthday;
        id = Guid.NewGuid();
        cityManager = _cityManager;
        this.transform.parent = cityManager.transform;
        home = _home;
        workplace = _workplace;
        destination = _workplace;
        this.transform.position = home.transform.position;
        location = home.CitySquare;
        locationPointer = 0;
        routeTo = location.Routes[destination.CitySquare];
        nextLocation = routeTo.Squares[locationPointer];
        rail = nextLocation.Offset - location.Offset;
        rail.y = (nextLocation.Height - location.Height);
        distTraveled = 0;
    }
    //public 
}
