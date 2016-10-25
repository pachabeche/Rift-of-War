using UnityEngine;
using System.Collections.Generic;
using BehaviorDesigner.Runtime.Tasks;
#if !(UNITY_4_3 || UNITY_4_4)
using Tooltip = BehaviorDesigner.Runtime.Tasks.TooltipAttribute;
#if !(UNITY_4_5 || UNITY_4_6 || UNITY_5_0)
using HelpURL = BehaviorDesigner.Runtime.Tasks.HelpURLAttribute;
#endif
#endif

namespace BehaviorDesigner.Runtime.Tactical.Tasks.SteeringBehavior
{
    [TaskCategory("Tactical/Steering Behavior")]
    [TaskDescription("Surrounds the enemy and starts to attack after all agents are in position")]
    [HelpURL("http://www.opsive.com/assets/BehaviorDesigner/Tactical/documentation.php?id=8")]
    [TaskIcon("Assets/Behavior Designer Tactical/Editor/Icons/{SkinColor}SurroundIcon.png")]
    public class Surround : SteeringBehaviorTacticalGroup
    {
        [Tooltip("The radius of the agents that should surround the target")]
        public SharedFloat radius = 10;

        private float theta;
        private bool inPosition;

        public override void OnStart()
        {
            base.OnStart();

            inPosition = false;
        }

        protected override void AddAgentToGroup(Transform agent)
        {
            base.AddAgentToGroup(agent);

            // 2 * PI = 360 degrees
            theta = 2 * Mathf.PI / agents.Count;
        }

        protected override int RemoveAgentFromGroup(Transform agent)
        {
            var index = base.RemoveAgentFromGroup(agent);

            // 2 * PI = 360 degrees
            theta = 2 * Mathf.PI / agents.Count;

            return index;
        }

        public override TaskStatus OnUpdate()
        {
            var baseStatus = base.OnUpdate();
            if (baseStatus != TaskStatus.Running || agents.Count == 0) {
                return baseStatus;
            }

            var attackCenter = CenterAttackPosition();
            var attackRotation = CenterAttackRotation(attackCenter);
            var offset = Vector3.zero;
            var localInPosition = true;
            for (int i = 0; i < agents.Count; ++i) {
                // Wait until all agents are in position before starting to attack.
                if (inPosition) {
                    if (MoveToAttackPosition(i)) {
                        agents[i].TryAttack();
                    }
                } else {
                    offset.Set(radius.Value * Mathf.Sin(theta * i), 0, radius.Value * Mathf.Cos(theta * i));
                    var destination = TransformPoint(attackCenter, offset, attackRotation);
                    var detour = false;
                    // Don't go through the center when travelling to the other side of the circle
                    if (offset.z < 0 && InverseTransformPoint(attackCenter, agents[i].Transform.position, attackRotation).z < -agents[i].Radius()) {
                        offset.Set((radius.Value + agents[i].Radius()) * Mathf.Sign(Mathf.Sin(theta * i)), 0, 0);
                        destination = TransformPoint(attackCenter, offset, attackRotation);
                        detour = true;
                    }
                    agents[i].SetDestination(destination);
                    // The agents can't be in position if they are taking a detour.
                    if (!detour && agents[i].HasArrived()) {
                        FindAttackTarget(i);
                        // The agents are not in position until they are looking at the target.
                        if (!agents[i].RotateTowardsPosition(agents[i].TargetTransform.position)) {
                            localInPosition = false;
                        }
                    } else {
                        // The agent isn't in position yet so don't attack.
                        localInPosition = false;
                    }
                }
            }
            // Start attacking if all agents are in position.
            if (localInPosition) {
                inPosition = localInPosition;
            }

            return TaskStatus.Running;
        }

        public override void OnReset()
        {
            base.OnReset();

            radius = 10;
        }
    }
}