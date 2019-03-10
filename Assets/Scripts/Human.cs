using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class Human
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
    public Guid ID {
        get { return id; }
    }
    public Occupation Occupation
    {
        get { return occupation; }
        set {
            occupation = value;
            value.Employee = this;
        }
    }
    public Human(int _age, Qualification _qualification, int _birthday)
    {
        age = _age;
        qualification = _qualification;
        birthday = _birthday;
        id = Guid.NewGuid();
    }
    //public 
}
