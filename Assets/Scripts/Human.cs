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
    public Occupation Occupation
    {
        get { return occupation; }
        set {
            occupation = value;
            value.Employee = this;
        }
    }
    public void init(int _age, CityManager _cityManager)//, Qualification _qualification, int _birthday)
    {
        age = _age;
        //qualification = _qualification;
        //birthday = _birthday;
        id = Guid.NewGuid();
        cityManager = _cityManager;
    }
    //public 
}
