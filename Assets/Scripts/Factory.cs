using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Factory : MonoBehaviour
{
    public Occupation[] occupations;
    // Start is called before the first frame update
    void Start()
    {
        occupations = new Occupation[12];
        for(int x = 0; x < 10; x++)
        {
            occupations[x] = new Occupation(Qualification.NoHS);
        }
        occupations[10] = new Occupation(Qualification.HS);
        occupations[11] = new Occupation(Qualification.Bachelors);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
