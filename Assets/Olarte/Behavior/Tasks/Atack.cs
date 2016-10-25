using UnityEngine;
using BehaviorDesigner.Runtime;
using BehaviorDesigner.Runtime.Tasks;
using System.Collections;

public class Atack : Action
{

    public SharedGameObject target;
    public SharedFloat range = 2.5f;
    public SharedFloat damage = 1;
    public SharedBool isRange = false;
    public SharedGameObject arrow = null;

    private float sqrMagnitude;

    public override void OnAwake()
    {
        sqrMagnitude = range.Value * range.Value;
    }

    public override TaskStatus OnUpdate()
    {
        Vector3 direction = target.Value.transform.position - transform.position;
        if (Vector3.SqrMagnitude(direction) < sqrMagnitude)
        {
            if (!isRange.Value)
            {
                transform.LookAt(target.Value.transform);
                try { target.Value.GetComponent<Life>().TakeDamage(damage.Value); } catch { }
                return TaskStatus.Success;
            }
            else
            {
                transform.LookAt(target.Value.transform);
                    Bullet b = arrow.Value.GetComponent<Bullet>();
                    b.h = 5;
                    b.target = target.Value;

                return TaskStatus.Success;
            }

        }
        return TaskStatus.Failure;
    }



    public override void OnDrawGizmos()
    {
#if UNITY_EDITOR
        if (Owner == null || range == null)
        {
            return;
        }
        var oldColor = UnityEditor.Handles.color;
        UnityEditor.Handles.color = Color.yellow;
        UnityEditor.Handles.DrawWireDisc(Owner.transform.position, Owner.transform.up, range.Value);
        UnityEditor.Handles.color = oldColor;
#endif
    }

}
