using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using YieldProlog;
public class ObservableFact
{
    private Atom label;
    private Atom[] values;

    public Atom getLabel()
    {  
        return label;
    }

    public Atom[] getValues()
    {
        return values; 
    }


    public void SetLabel(string str)
    {
        this.label = Atom.a(str);
    }

    public void SetValues(string[] val)
    {
        Atom[] newAtoms = new Atom[val.Length];
        for(int i = 0; i < val.Length; i++)
        {
            newAtoms[i] = Atom.a(val[i]);
        }
        values = newAtoms;
    }
}

public class Observable : MonoBehaviour {

    public List<ObservableFact> observableFacts = new List<ObservableFact>();
    public List<LocationClue> locationClues = new List<LocationClue>;

    public void AddObservableFact(string label, string[] values)
    {
        ObservableFact newFact = new ObservableFact();
        newFact.SetLabel(label);
        newFact.SetValues(values);

        observableFacts.Add(newFact);
    }

    public void AddLocationClue(LocationClue clue){
        locationClues.Add(clue);
    }

}
