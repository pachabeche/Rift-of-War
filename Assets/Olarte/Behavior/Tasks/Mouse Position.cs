using UnityEngine;
using BehaviorDesigner.Runtime;
using BehaviorDesigner.Runtime.Tasks;

public class MousePosition : Action
{
    public SharedVector3 mousePosition;

    public override TaskStatus OnUpdate()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;
        if (Physics.Raycast(ray, out hit, 100))
        {
            mousePosition.Value = hit.point;
            return TaskStatus.Success;
        }
        return TaskStatus.Failure;
    }
}
