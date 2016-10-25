using UnityEngine;

public class Animation_Setter : MonoBehaviour
{

    private Animator anim;
    private bool walking;
    private NavMeshAgent navMeshAgent;

    void Start()
    {
        anim = GetComponent<Animator>();
        navMeshAgent = GetComponent<NavMeshAgent>();
    }

    void Update ()
    {
        walking = true;
        if (navMeshAgent.remainingDistance <= navMeshAgent.stoppingDistance)
        {
            if (!navMeshAgent.hasPath || Mathf.Abs(navMeshAgent.velocity.sqrMagnitude) < float.Epsilon)
            {
                walking = false;
            }
        }
        anim.SetBool("IsWalking", walking);
    }
}
