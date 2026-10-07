
using UnityEngine;

public class RandomDead : MonoBehaviour
{
    private Animator animator;

    private static readonly string[] DeadAnimationNames = {
        "goblinDead_1",
        "goblinDead_2",
        "goblinDead_3"
    };

    private void Awake()
    {
        animator = GetComponent<Animator>();
    }

    private void OnEnable()
    {
        int randomIndex = Random.Range(0, DeadAnimationNames.Length);
        animator.CrossFade(DeadAnimationNames[randomIndex], 0.1f);
    }
}

