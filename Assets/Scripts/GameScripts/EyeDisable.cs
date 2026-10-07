
using UnityEngine;

public class EyeDisable : MonoBehaviour
{
    public void OffEye()
    {
        transform.position = Vector3.zero;
        gameObject.SetActive(false);
    }
}
