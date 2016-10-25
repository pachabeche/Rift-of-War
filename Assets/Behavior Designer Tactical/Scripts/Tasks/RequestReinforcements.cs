using UnityEngine;
using System.Collections.Generic;
using BehaviorDesigner.Runtime.Tasks;
#if !(UNITY_4_3 || UNITY_4_4)
using Tooltip = BehaviorDesigner.Runtime.Tasks.TooltipAttribute;
#if !(UNITY_4_5 || UNITY_4_6 || UNITY_5_0)
using HelpURL = BehaviorDesigner.Runtime.Tasks.HelpURLAttribute;
#endif
#endif

namespace BehaviorDesigner.Runtime.Tactical.Tasks
{
    [TaskCategory("Tactical")]
    [TaskDescription("Requests reinforcements")]
    [HelpURL("http://www.opsive.com/assets/BehaviorDesigner/Tactical/documentation.php?id=12")]
    [TaskIcon("Assets/Behavior Designer Tactical/Editor/Icons/{SkinColor}RequestReinforcementsIcon.png")]
    public class RequestReinforcements : Action
    {
        public override TaskStatus OnUpdate()
        {
            Owner.SendEvent<GameObject>("RequestReinforcements", gameObject);

            return TaskStatus.Success;
        }
    }
}