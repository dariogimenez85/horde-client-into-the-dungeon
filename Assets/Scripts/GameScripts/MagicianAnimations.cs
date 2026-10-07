
using UnityEngine;

[RequireComponent(typeof(Animator))]
public class MagicianAnimations : MonoBehaviour
{

    private Animator mageAnimator;
    
    private void Awake()
    {
        mageAnimator = GetComponent<Animator>();
    }

    public void SetIdleAnimation()
    {
        transform.GetChild(0).gameObject.SetActive(false);
        mageAnimator.CrossFade("Mage_Idle", 0.15f);
    }
}
