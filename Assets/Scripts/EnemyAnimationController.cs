namespace HorrorRPG.Presentation
{



using UnityEngine;

public class EnemyAnimationController : MonoBehaviour
{
    [Header("Animation")]
    [SerializeField] private Animator animator;

    private const string START_TRIGGER = "Start";
    private const string DEAD_TRIGGER = "Dead";

    public void PlayStartAnimation()
    {
        if (animator != null)
        {
            animator.SetTrigger(START_TRIGGER);
        }
        else
        {
            Debug.LogWarning("EnemyAnimationController: Animator não está definido.");
        }
    }

    public void PlayDeadAnimation()
    {
        if (animator != null)
        {
            animator.SetTrigger(DEAD_TRIGGER);
        }
        else
        {
            Debug.LogWarning("EnemyAnimationController: Animator não está definido.");
        }
    }
}


}
