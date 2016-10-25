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
    [TaskDescription("Retreats in the opposite direction of the target")]
    [HelpURL("http://www.opsive.com/assets/BehaviorDesigner/Tactical/documentation.php?id=9")]
    [TaskIcon("Assets/Behavior Designer Tactical/Editor/Icons/{SkinColor}RetreatIcon.png")]
    public class Retreat : NavMeshTacticalGroup
    {
        [Tooltip("The distance away from the targets that is considered safe")]
        public SharedFloat safeDistance;

        private Quaternion direction;
        private Vector3 safeOffset;

        protected override void AddAgentToGroup(Transform agent)
        {
            base.AddAgentToGroup(agent);

            // Prevent the agent from updating its rotation so the agent can attack while retreating.
            agents[agents.Count - 1].UpdateRotation(false);
        }

        protected override void StartGroup()
        {
            base.StartGroup();

            direction = transform.rotation;
            safeOffset.z = -safeDistance.Value;
        }

        public override TaskStatus OnUpdate()
        {
            var baseStatus = base.OnUpdate();
            if (baseStatus != TaskStatus.Running || agents.Count == 0) {
                return baseStatus;
            }

            var attackCenter = CenterAttackPosition();
            var allSafe = true;
            for (int i = 0; i < agents.Count; ++i) {
                // Try to attack the enemy while retreating.
                FindAttackTarget(i);
                if (agents[i].CanSeeTarget()) {
                    if (agents[i].RotateTowardsPosition(agents[i].TargetTransform.position)) {
                        agents[i].TryAttack();
                    }
                } else {
                    // The agent can update its rotation when the agent is far enough away that it can't attack.
                    agents[i].UpdateRotation(true);
                }

                // The agents are only save once they have moved more than the safe distance away from the attack point.
                if ((attackCenter - agents[i].Transform.position).magnitude < safeDistance.Value) {
                    allSafe = false;
                    var targetPosition = TransformPoint(agents[i].Transform.position, safeOffset, direction);
                    agents[i].SetDestination(targetPosition);
                } else {
                    agents[i].Stop();
                }
            }

            return allSafe ? TaskStatus.Success : TaskStatus.Running;
        }
    }
}