
using UnityEngine;

public class EyePool : MonoBehaviour
{

    private void Start()
    {
        // disable all children at the start
        foreach (Transform child in transform)
        {
            child.gameObject.SetActive(false);
        }
    }

    public void DisableAll()
    {
        foreach (Transform child in transform)
        {
            child.gameObject.SetActive(false);
        }
    }

    public void EnableFirstDisabledChild(Transform cellPosition)
    {
        foreach (Transform child in transform)
        {
            if (!child.gameObject.activeSelf)
            {
                child.position = cellPosition.position;
                child.gameObject.SetActive(true);
                break;
            }
        }
    }
}
