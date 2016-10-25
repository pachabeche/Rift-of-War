using UnityEngine;
using BehaviorDesigner.Runtime;
using BehaviorDesigner.Runtime.Tasks;

public class FindSelf : Decorator
{
    public SharedGameObject target;

    public override TaskStatus OnUpdate()
    {
        target.Value = gameObject;
        return TaskStatus.Success;
    }
}
