using UnityEngine;
using BehaviorDesigner.Runtime;
using BehaviorDesigner.Runtime.Tasks;
public class MoveTo : Action
{
    public SharedGameObject target;
    public SharedVector3 targetPosition;

    private NavMeshAgent navMeshAgent;
    private Animator anim;

    public override void OnAwake()
    {
        navMeshAgent = GetComponent<NavMeshAgent>();
        anim = GetComponent<Animator>();
    }

    public override TaskStatus OnUpdate()
    {
        var position = Target();
        if (position == Vector3.zero)
        {
            anim.SetBool("IsWalking", false);
            return TaskStatus.Success;
        }
        navMeshAgent.destination = position;
        anim.SetBool("IsWalking", true);
        navMeshAgent.Resume();

        if (Vector3.Magnitude(transform.position - position) < 0.3)
        {
            anim.SetBool("IsWalking", false);
            navMeshAgent.Stop();
            return TaskStatus.Success;
        }

        return TaskStatus.Running;
    }


    private Vector3 Target()
    {
        if (target == null || target.Value == null)
        {
            return targetPosition.Value;
        }
        return target.Value.transform.position;
    }


}
