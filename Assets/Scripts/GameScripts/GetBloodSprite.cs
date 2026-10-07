
using UnityEngine;

public class GetBloodSprite : MonoBehaviour
{
    private SpriteRenderer sprBlood;
    private BloodRandomManager bloodRandomManager;


    private void OnEnable()
    {
        sprBlood = GetComponent<SpriteRenderer>();
        bloodRandomManager = transform.GetComponentInParent<BloodRandomManager>();
        sprBlood.sprite = bloodRandomManager.RandomBlood();
    }

    private void OnDisable()
    {
        transform.localPosition = Vector3.zero;
    }
}
