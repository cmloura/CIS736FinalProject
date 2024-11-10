using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu]
public class TerrainData : UpdateableData
{
    public float uniformscale = 2.5f;
    public float meshHeightMultiplier;
    public AnimationCurve meshheightcurve;
    public bool useFalloff;
}
