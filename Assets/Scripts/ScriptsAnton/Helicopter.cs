using UnityEngine;

public class Helicopter : MonoBehaviour {
    [SerializeField] private float helicopterSoundVolume;
    [SerializeField] private GameObject rotor_lower;
    [SerializeField] private GameObject rotor_upper;
    private AudioSource audioSource;

    public float rotorMaxSpeed = 50f;
    private float rotorSpeed = 0f;

    private void Start() {
        audioSource = GetComponent<AudioSource>();
        audioSource.volume = helicopterSoundVolume;
        audioSource.Play();
    }

    private void Update() {
        rotorSpeed = Mathf.MoveTowards(rotorSpeed, rotorMaxSpeed, 10f * Time.deltaTime);
        rotor_lower.transform.Rotate(Vector3.down, rotorSpeed * Time.deltaTime);
        rotor_upper.transform.Rotate(Vector3.up, rotorSpeed * Time.deltaTime);
    }
}
