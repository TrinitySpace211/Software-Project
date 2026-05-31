using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Surface", menuName = "Impact System/Surface")]
public class Surface : ScriptableObject {

    public List<SurfaceImpactTypeEffect> impactTypeEffects = new List<SurfaceImpactTypeEffect>();

    [Serializable]
    public class SurfaceImpactTypeEffect {
        public ImpactType impactType;
        public SurfaceEffect surfaceEffect;
    }

}
