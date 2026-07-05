using UnityEngine;

/// <summary>
/// Animates the Helicopter rotors blades and starts the Sound Effect that the Helicopter is flying away
/// </summary>
public class Helicopter : MonoBehaviour {
    [SerializeField] private float helicopterSoundVolume;
    [SerializeField] private GameObject rotor_lower;
    [SerializeField] private GameObject rotor_upper;
    private AudioSource audioSource;

    public float rotorMaxSpeed = 50f;
    private float rotorSpeed = 0f;

    /// <summary>
    /// Starts the Sound Effect of the Helicopter flying away
    /// </summary>
    private void Start() {
        audioSource = GetComponent<AudioSource>();
        audioSource.volume = helicopterSoundVolume;
        audioSource.Play();
    }

    /// <summary>
    /// Rotates the Rotors increasing its speed
    /// </summary>
    private void Update() {
        rotorSpeed = Mathf.MoveTowards(rotorSpeed, rotorMaxSpeed, 10f * Time.deltaTime);
        rotor_lower.transform.Rotate(Vector3.down, rotorSpeed * Time.deltaTime);
        rotor_upper.transform.Rotate(Vector3.up, rotorSpeed * Time.deltaTime);
    }
}
