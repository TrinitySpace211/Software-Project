using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Pool;

/// <summary>
/// The Surface Manager can be used to play different 
/// Object and Sound Effects like a Particle System and Impact Sounds 
/// on different Textures that got hit by something (a Raycast or a Collider)
/// </summary>
public class SurfaceManager : MonoBehaviour {
    public static SurfaceManager Instance { get; private set; }

    [SerializeField] private List<SurfaceType> surfacesTypes = new List<SurfaceType>();
    [SerializeField] private int defaultPoolSizes = 10;
    [SerializeField] private Surface defaultSurface;

    private void Awake() {
        if (Instance != null) {
            Debug.LogError("More than one SurfaceManager active in the scene! Destroying latest one: " + name);
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    /// <summary>
    /// If we hit a Terrain with our bullets/foots/... then play the Soundeffect that matches the list of active Textures
    /// If it doesn't find the right type the default Sound will be played 
    /// </summary>
    /// <param name="hitObject">The Object that got hit</param>
    /// <param name="hitPoint">The position of the hit</param>
    /// <param name="hitNormal">The normal Value of the position on the terrain</param>
    /// <param name="impact">What type of impact should be played (Bullets, Footsteps, etc.)?</param>
    public void HandleImpact(GameObject hitObject, Vector3 hitPoint, Vector3 hitNormal, ImpactType impact, int triangleIndex) {
        //if it hits a terrain make impact sound depending on the terrain layer
        if (hitObject.TryGetComponent(out Terrain terrain)) {
            List<TextureAlpha> activeTextures = GetActiveTexturesFromTerrain(terrain, hitPoint);

            foreach (TextureAlpha activeTexture in activeTextures) {
                SurfaceType surfaceType = surfacesTypes.Find(surfaceType => surfaceType.albedo == activeTexture.texture);

                if (surfaceType != null) {
                    foreach (Surface.SurfaceImpactTypeEffect typeEffect in surfaceType.surface.impactTypeEffects) {
                        if (typeEffect.impactType == impact) {
                            PlayEffects(hitPoint, hitNormal, typeEffect.surfaceEffect, activeTexture.alpha);
                        }
                    }
                } else {
                    foreach (Surface.SurfaceImpactTypeEffect typeEffect in defaultSurface.impactTypeEffects) {
                        if (typeEffect.impactType == impact) {
                            PlayEffects(hitPoint, hitNormal, typeEffect.surfaceEffect, 1);
                        }
                    }
                }
            }
        } else {
            ZombieAI zombie = hitObject.GetComponentInParent<ZombieAI>();
            SprinterController sprinter = hitObject.GetComponentInParent<SprinterController>();
            Renderer renderer = null;
            if (zombie != null) {
                renderer = zombie.GetComponentInChildren<SkinnedMeshRenderer>();
            } else if (sprinter != null) {
                renderer = sprinter.GetComponentInChildren<SkinnedMeshRenderer>();
            }

            if (renderer != null) {
                SurfaceType surfaceType = null;
                Texture activeTexture = null;
                if (renderer != null) {
                    activeTexture = GetActiveTextureFromRenderer(renderer, triangleIndex);
                    surfaceType = surfacesTypes.Find(surface => surface.albedo == activeTexture);
                }

                if (surfaceType != null) {
                    foreach (Surface.SurfaceImpactTypeEffect typeEffect in surfaceType.surface.impactTypeEffects) {
                        if (typeEffect.impactType == impact) {
                            PlayEffects(hitPoint, hitNormal, typeEffect.surfaceEffect, 1);
                        }
                    }
                } else {
                    foreach (Surface.SurfaceImpactTypeEffect typeEffect in defaultSurface.impactTypeEffects) {
                        if (typeEffect.impactType == impact) {
                            PlayEffects(hitPoint, hitNormal, typeEffect.surfaceEffect, 1);
                        }
                    }
                }
            } else {
                renderer = hitObject.GetComponent<Renderer>();
                if (renderer == null) {
                    renderer = hitObject.GetComponentInParent<Renderer>();
                }

                if (renderer != null) {
                    Texture activeTexture = GetActiveTextureFromRenderer(renderer, triangleIndex);
                    // Holt die aktive Textur des Meshes

                    SurfaceType surfaceType = surfacesTypes.Find(surface => surface.albedo == activeTexture);
                    // Sucht SurfaceType anhand der Textur

                    if (surfaceType != null) {
                        foreach (Surface.SurfaceImpactTypeEffect typeEffect in surfaceType.surface.impactTypeEffects) {
                            if (typeEffect.impactType == impact) {
                                PlayEffects(hitPoint, hitNormal, typeEffect.surfaceEffect, 1);
                                // Spielt Effekte für erkannte Oberfläche
                            }
                        }
                    } else {
                        foreach (Surface.SurfaceImpactTypeEffect typeEffect in defaultSurface.impactTypeEffects) {
                            if (typeEffect.impactType == impact) {
                                PlayEffects(hitPoint, hitNormal, typeEffect.surfaceEffect, 1);
                                // Fallback-Effekte
                            }
                        }
                    }
                } else {
                    foreach (Surface.SurfaceImpactTypeEffect typeEffect in defaultSurface.impactTypeEffects) {
                        if (typeEffect.impactType == impact) {
                            PlayEffects(hitPoint, hitNormal, typeEffect.surfaceEffect, 1);
                        }
                    }
                }
            }
        }
    }

    /// <summary>
    /// Extracts the textures from the terrain if it has multiple ones
    /// </summary>
    /// <param name="terrain">The Terrain</param>
    /// <param name="hitPoint">The position of the hit</param>
    /// <returns></returns>
    private List<TextureAlpha> GetActiveTexturesFromTerrain(Terrain terrain, Vector3 hitPoint) {
        Vector3 terrainPosition = hitPoint - terrain.transform.position;
        Vector3 splatMapPosition = new Vector3(
            terrainPosition.x / terrain.terrainData.size.x,
            0,
            terrainPosition.z / terrain.terrainData.size.z
        );

        int x = Mathf.FloorToInt(splatMapPosition.x * terrain.terrainData.alphamapWidth);
        int z = Mathf.FloorToInt(splatMapPosition.z * terrain.terrainData.alphamapHeight);

        x = Mathf.Clamp(x, 0, terrain.terrainData.alphamapWidth - 1);
        z = Mathf.Clamp(z, 0, terrain.terrainData.alphamapHeight - 1);

        float[,,] alphaMap = terrain.terrainData.GetAlphamaps(x, z, 1, 1);

        List<TextureAlpha> activeTextures = new List<TextureAlpha>();
        for (int i = 0; i < alphaMap.Length; i++) {
            if (alphaMap[0, 0, i] > 0) {
                activeTextures.Add(new TextureAlpha {
                    texture = terrain.terrainData.terrainLayers[i].diffuseTexture,
                    alpha = alphaMap[0, 0, i]
                });
            }
        }
        return activeTextures;
    }

    /// <summary>
    /// Extracts the texture from a mesh
    /// </summary>
    /// <param name="renderer">The Meshrenderer</param>
    /// <param name="triangleIndex">the index of a triangle on the hit object</param>
    /// <returns>the texture of that mesh</returns>
    private Texture GetActiveTextureFromRenderer(Renderer renderer, int triangleIndex) {
        if (renderer == null) {
            Debug.LogError("Renderer is null in GetActiveTextureFromRenderer.");
            return null;
        }

        Mesh mesh = null;
        if (renderer is SkinnedMeshRenderer skinnedMeshRenderer) {
            if (skinnedMeshRenderer.sharedMaterials.Length > 1) {
                if (skinnedMeshRenderer.sharedMaterials[0].mainTexture == null) {
                    return skinnedMeshRenderer.sharedMaterials[0].GetTexture("_MainTexture");
                } else {
                    return skinnedMeshRenderer.sharedMaterials[0].mainTexture;
                }

            } else {
                if (skinnedMeshRenderer.sharedMaterial.mainTexture == null) {
                    return skinnedMeshRenderer.sharedMaterials[0].GetTexture("_MainTexture");
                } else {
                    return skinnedMeshRenderer.sharedMaterial.mainTexture;
                }
            }

        } else if (renderer.TryGetComponent(out MeshFilter meshFilter)) {
            mesh = meshFilter.sharedMesh;
        }

        if (mesh == null) {
            //Debug.LogWarning($"{renderer.name} has no mesh. Falling back to material texture.");
            return renderer.sharedMaterial != null ? renderer.sharedMaterial.mainTexture : null;
        }

        if (!mesh.isReadable) {
            //Debug.LogWarning($"{renderer.name} mesh is not readable. Falling back to material texture.");
            return renderer.sharedMaterial != null ? renderer.sharedMaterial.mainTexture : null;
        }

        if (triangleIndex < 0 || mesh.triangles == null || triangleIndex * 3 + 2 >= mesh.triangles.Length) {
            //Debug.LogWarning($"Invalid triangle index {triangleIndex} for mesh '{renderer.name}'. Falling back to first material.");
            return renderer.sharedMaterial != null ? renderer.sharedMaterial.mainTexture : null;
        }

        if (mesh.subMeshCount > 1) {
            int[] hitTriangleIndices = new int[]
            {
                mesh.triangles[triangleIndex * 3],
                mesh.triangles[triangleIndex * 3 + 1],
                mesh.triangles[triangleIndex * 3 + 2]
            };

            for (int i = 0; i < mesh.subMeshCount; i++) {
                int[] submeshTriangles = mesh.GetTriangles(i);
                for (int j = 0; j < submeshTriangles.Length; j += 3) {
                    if (submeshTriangles[j] == hitTriangleIndices[0]
                        && submeshTriangles[j + 1] == hitTriangleIndices[1]
                        && submeshTriangles[j + 2] == hitTriangleIndices[2]) {
                        return renderer.sharedMaterials[i].mainTexture;
                    }
                }
            }
        }

        return renderer.sharedMaterial != null ? renderer.sharedMaterial.mainTexture : null;
    }

    /// <summary>
    /// Plays Visual and Audio-Effects
    /// </summary>
    /// <param name="hitPoint">Position of the hit</param>
    /// <param name="hitNormal">the normal of the hit surface</param>
    /// <param name="surfaceEffect">ScriptableObject with the a list of object effects and sound effects</param>
    /// <param name="soundOffset">Sound Multiplier</param>
    private void PlayEffects(Vector3 hitPoint, Vector3 hitNormal, SurfaceEffect surfaceEffect, float soundOffset) {
        if (surfaceEffect.spawnObjectEffects.Count > 0) {
            foreach (SpawnObjectEffects spawnObjectEffect in surfaceEffect.spawnObjectEffects) {
                if (spawnObjectEffect.probability > Random.value) {
                    ObjectPool pool = ObjectPool.CreateInstance(spawnObjectEffect.prefab.GetComponent<PoolableObject>(), defaultPoolSizes);
                    PoolableObject instance = pool.GetObject(hitPoint + hitNormal * 0.001f, Quaternion.LookRotation(hitNormal));

                    instance.transform.forward = hitNormal;
                    if (spawnObjectEffect.randomizeRotation) {

                        Vector3 offset = new Vector3(
                            Random.Range(0, 180 * spawnObjectEffect.randomizedRotationMultiplier.x),
                            Random.Range(0, 180 * spawnObjectEffect.randomizedRotationMultiplier.y),
                            Random.Range(0, 180 * spawnObjectEffect.randomizedRotationMultiplier.z)
                        );
                        instance.transform.rotation = Quaternion.Euler(transform.rotation.eulerAngles + offset);
                    }
                }
            }
        }
        foreach (PlayAudioEffect playAudioEffect in surfaceEffect.playAudioEffects) {
            AudioClip clip = playAudioEffect.audioClips[Random.Range(0, playAudioEffect.audioClips.Count)];
            ObjectPool pool = ObjectPool.CreateInstance(playAudioEffect.audioSourcePrefab.GetComponent<PoolableObject>(), defaultPoolSizes);
            AudioSource audioSource = pool.GetObject().GetComponent<AudioSource>();

            audioSource.transform.position = hitPoint;
            audioSource.PlayOneShot(clip, soundOffset * Random.Range(playAudioEffect.volumeRange.x, playAudioEffect.volumeRange.y));
            StartCoroutine(DisableAudioSource(audioSource, clip.length));
        }
    }

    /// <summary>
    /// Disables the Sound after "time" in seconds has past
    /// </summary>
    /// <param name="audioSource">the audio source</param>
    /// <param name="Time">the time before the sound gets disabled</param>
    private IEnumerator DisableAudioSource(AudioSource audioSource, float Time) {
        yield return new WaitForSeconds(Time);

        audioSource.gameObject.SetActive(false);
    }

    /// <summary>
    /// Has the alpha and the Texture
    /// </summary>
    private class TextureAlpha {
        public float alpha;
        public Texture texture;
    }
}
