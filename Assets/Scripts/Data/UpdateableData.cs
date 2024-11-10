using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UpdateableData : ScriptableObject
{
    public event System.Action OnValuesUpdated;
    public bool autoupdate;
    
    protected virtual void OnValidate() 
    {
        if(autoupdate)
            NotifyOfUpdatedValues();
    }

    public void NotifyOfUpdatedValues()
    {
        if(OnValuesUpdated != null)
            OnValuesUpdated();
    }
}
