using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EconomicUnit
{
    private float income;
    public float Income {
        get
        {
            return income;
        }
        set {
            if (Parent != null)
            {
                Parent.Income -= this.income;
            }
            this.income = value;
            if (Parent != null)
            {
                Parent.Income += value;
            }
        }
    }
    private float expenses;
    public float Expenses
    {
        get
        {
            return expenses;
        }
        set
        {
            if (Parent != null)
            {
                Parent.Expenses -= this.expenses;
            }
            this.expenses = value;
            if (Parent != null)
            {
                Parent.Expenses += value;
            }
        }
    }
    public float EffectiveIncome {
        get
        {
            float finalMultiplier = 1;
            foreach (float multi in IncomeMulitpliers.Values)
            {
                finalMultiplier *= multi;
            }
            return Income * finalMultiplier;
        }
    }
    public Dictionary<string, float> IncomeMulitpliers { get; private set; }
    public HashSet<EconomicUnit> Children { get; private set; }
    public HashSet<RealEstate> ReProperties { get; private set; }
    public float Balance { get; set; }
    public HashSet<Occupation> Occupations { get; private set; }
    public EconomicUnit Parent { get; private set; }
    public EconomicUnit(EconomicUnit _parent)
    {
        IncomeMulitpliers = new Dictionary<string, float>();
        this.Income = 0;
        this.Expenses = 0;
        this.Children = new HashSet<EconomicUnit>();
        this.Parent = _parent;
        this.ReProperties = new HashSet<RealEstate>();
        this.Balance = 0;
        this.Occupations = new HashSet<Occupation>();
    }
}
