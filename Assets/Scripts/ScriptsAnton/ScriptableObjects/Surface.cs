using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Scriptable Object to determine which impact type should be played a which surface
/// </summary>
[CreateAssetMenu(fileName = "Surface", menuName = "Impact System/Surface")]
public class Surface : ScriptableObject {

    public List<SurfaceImpactTypeEffect> impactTypeEffects = new List<SurfaceImpactTypeEffect>();

    [Serializable]
    public class SurfaceImpactTypeEffect {
        public ImpactType impactType;
        public SurfaceEffect surfaceEffect;
    }

}
