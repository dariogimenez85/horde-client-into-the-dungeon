using UnityEngine;

public class BloodRandomManager : MonoBehaviour
{
    [SerializeField] private Sprite[] bloods;

    public Sprite RandomBlood()
    {
        return bloods[Random.Range(0, bloods.Length)];
    }

    public void DisableChilds()
    {
        Debug.Log("DisableChilds");
        for (int i = 0; i < transform.childCount; i++)
        {
            transform.GetChild(i).gameObject.SetActive(false);
        }
    }

    public void EnableFirstInactiveChild(Vector3 tilePosition)
    {
        for (int i = 0; i < transform.childCount; i++)
        {
            Transform child = transform.GetChild(i);

            if (child.gameObject.activeSelf) continue;

            child.transform.position = tilePosition;
            child.gameObject.SetActive(true);
            break; // Terminamos al encontrar el primero inactivo
        }
    }
}