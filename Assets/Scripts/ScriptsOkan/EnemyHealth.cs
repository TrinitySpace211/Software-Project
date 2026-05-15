using UnityEngine;

public class EnemyHealth : MonoBehaviour
{
    private Animator animator;
    public int health = 100;
    private const string IS_DEAD = "isDead";
    void Start()
    {
        animator = GetComponent<Animator>();
    }

    public void TakeDamage(int damage)
    {
        health -= damage;
        if (health <= 0)
        {
            animator.SetBool(IS_DEAD, true);
        }
    }
}