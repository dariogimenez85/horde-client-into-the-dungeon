
using UnityEngine;

public class PointAnimationDisabled : MonoBehaviour
{
    public void DisableObject()
    {
        // Desactivar el objeto para que no se muestre en la escena
        gameObject.transform.parent.gameObject.SetActive(false);
    }
}


