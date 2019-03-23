using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

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
    public float dist;
    void Update()
    {
        dist = (this.transform.localPosition - (nextLocation.Offset + new Vector3(0, nextLocation.Height + .5f, 0))).magnitude;
        if ((this.transform.localPosition - (nextLocation.Offset + new Vector3(0, nextLocation.Height + .5f, 0))).magnitude < .1)
        {
            location = nextLocation;
            if(location == destination.CitySquare)
            {
                if(destination == workplace)
                {
                    destination = home;
                }
                else
                {
                    destination = workplace;
                }
                transform.localPosition = nextLocation.Offset + new Vector3(0, nextLocation.Height);
                locationPointer = 0;
                routeTo = nextLocation.Routes[destination.CitySquare];
                nextLocation = routeTo.Squares[locationPointer];
                rail = nextLocation.Offset - location.Offset;
                rail.y = (nextLocation.Height - location.Height);
                return;
            }
            locationPointer++;
            nextLocation = routeTo.Squares[locationPointer];
            rail = nextLocation.Offset - location.Offset;
            rail.y = (nextLocation.Height - location.Height);
            this.transform.localPosition = nextLocation.Offset + new Vector3(0, nextLocation.Height);
        }
        else
        {
            this.transform.Translate(rail * Time.deltaTime);
        }
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
    }
    //public 
}
