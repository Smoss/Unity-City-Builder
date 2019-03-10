using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum Qualification { NoHS, HS, Bachelors, Masters, PHD, Athlete = -1, Entrepeneur = -2 }
public class Occupation
{
    Qualification requirements;
    public Qualification Requirements
    {
        get { return requirements; }
    }
    Human employee;
    public Human Employee {
        get { return employee; }
        set
        {
            if (employee == null)
            {
                employee = value;
            }
            //performance = 3;
        }
    }
    //int performance;
    int pay;
    public bool interview(Human interviewee)
    {
        if ((int) (interviewee.Qualification) > (int) this.Requirements && this.employee == null)
        {
            interviewee.Occupation = this;
            return true;
        }
        return false;
    }
    public void fire()
    {

    }
    public Occupation(Qualification _requirements)
    {

    }
}
