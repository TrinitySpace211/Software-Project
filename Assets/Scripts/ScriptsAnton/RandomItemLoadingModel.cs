using UnityEngine;

public class RandomItemLoadingModel : MonoBehaviour {

    [SerializeField] private GameObject[] items;

    private void Awake() {
        foreach (GameObject item in items) {
            item.SetActive(false);
        }

        int index = Random.Range(0, items.Length);

        items[index].SetActive(true);
    }
}
