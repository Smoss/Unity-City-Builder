using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public enum Qualification { NoHS, HS, Bachelors, Masters, PHD, Athlete = -1, Entrepeneur = -2 }
public class Occupation
{
    Qualification requirements;
    readonly Guid id;
    public Qualification Requirements
    {
        get { return requirements; }
    }
    Human employee;
    float income;
    public float Income
    {
        get { return income; }
    }
    public Human Employee {
        get { return employee; }
        set
        {
            if (employee == null || value == null)
            {
                employee = value;
            }
            //performance = 3;
        }
    }
    //int performance;
    int pay;
    public RealEstate Location { get; private set; }
    public bool interview(Human interviewee)
    {
        if ((int) (interviewee.Qualification) >= (int) this.Requirements && this.employee == null)
        {
            interviewee.Occupation = this;
            return true;
        }
        return false;
    }
    public void fire()
    {

    }
    public Occupation(Qualification _requirements, int _income, RealEstate _location)
    {
        this.income = _income;
        this.requirements = _requirements;
        this.Location = _location;
        this.id = Guid.NewGuid();
    }
}
