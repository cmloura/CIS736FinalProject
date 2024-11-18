using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu()]
public class TerrainData : UpdateableData
{
    public float uniformscale = 2.5f;
    public float meshHeightMultiplier;
    public AnimationCurve meshheightcurve;
    public bool useFalloff;

    public float minHeight
    {
        get {return uniformscale * meshHeightMultiplier * meshheightcurve.Evaluate(0);}
    }

    public float maxHeight
    {
        get {return uniformscale * meshHeightMultiplier * meshheightcurve.Evaluate(1);}
    }
}
