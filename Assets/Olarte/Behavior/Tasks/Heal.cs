using UnityEngine;
using BehaviorDesigner.Runtime;
using BehaviorDesigner.Runtime.Tasks;
using System.Collections;

public class Heal : Action
{

    public SharedGameObject target;
    public SharedFloat range = 2.5f;


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

                try { target.Value.GetComponent<Life>()._unit.GetComponent<Unit>().AddAgent(); } catch { }
                return TaskStatus.Success;

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
