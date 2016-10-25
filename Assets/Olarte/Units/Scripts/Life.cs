using UnityEngine;
using System.Collections;

public class Life : MonoBehaviour
{

    public float maxHealth = 5;

    public GameObject _unit;

    private float currentHealth;
    private Animator anim;
    private bool dead= false;

	void Start ()
    {
        anim = GetComponent<Animator>();
        currentHealth = maxHealth;
	}

    public void TakeDamage(float damage)
    {
        if (damage < 0)
            return;
        currentHealth -= damage;
        if (currentHealth <= 0 && !dead)
           StartCoroutine(Die());
    }

    public void Heal(float health)
    {
        if (health < 0)
            return;
        currentHealth += health;
        if (currentHealth > maxHealth)
            currentHealth = maxHealth;
    }

    IEnumerator Die()
    {
        dead = true;
        try {_unit.GetComponent<Unit>().AgentDied(gameObject);}catch { }
        anim.SetBool("IsDead",true);
        yield return new WaitForSeconds(1);
        Destroy(gameObject);
    }
}
