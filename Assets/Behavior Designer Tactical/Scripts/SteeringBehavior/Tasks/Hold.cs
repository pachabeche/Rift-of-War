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
    [TaskDescription("Defends the object within a defend radius. Will seek and attack a target for as long as it takes")]
    [HelpURL("http://www.opsive.com/assets/BehaviorDesigner/Tactical/documentation.php?id=11")]
    [TaskIcon("Assets/Behavior Designer Tactical/Editor/Icons/{SkinColor}HoldIcon.png")]
    public class Hold : SteeringBehaviorTacticalGroup
    {
        [Tooltip("The object to defend")]
        public SharedGameObject defendObject;
        [Tooltip("The radius around the defend object to position the agents")]
        public SharedFloat radius = 5;
        [Tooltip("The radius around the defend object to defend")]
        public SharedFloat defendRadius = 10;

        private float theta;

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

            // Loop through the possible target transforms and determine which transform is the closest to each agent.
            for (int i = targetTransforms.Count - 1; i > -1; --i) {
                // The target has to be alive.
                if (targets[i].IsAlive()) {
                    // Find an agent that is close to this target object if the target object is within the defend radius.
                    if ((defendObject.Value.transform.position - targetTransforms[i].position).magnitude < defendRadius.Value) {
                        var distance = float.MaxValue;
                        float localDistance;
                        var index = -1;
                        // Find the closest agent.
                        for (int j = 0; j < agents.Count; ++j) {
                            if ((localDistance = (agents[j].Transform.position - targetTransforms[i].position).magnitude) < distance) {
                                distance = localDistance;
                                index = j;
                            }
                        }
                        if (index > -1) {
                            agents[index].TargetDamagable = targets[i];
                            agents[index].TargetTransform = targetTransforms[i];
                        }
                    }
                } else {
                    // The target is no longer alive - remove it from the list.
                    targets.RemoveAt(i);
                    targetTransforms.RemoveAt(i);
                }
            }

            for (int i = 0; i < agents.Count; ++i) {
                agents[i].AttackPosition = false;
                // Attack the target if the agent has a target.
                if (agents[i].TargetTransform != null) {
                    // Stop attacking if the target is no longer alive.
                    if (!agents[i].TargetDamagable.IsAlive()) {
                        agents[i].TargetTransform = null;
                        agents[i].TargetDamagable = null;
                    } else {
                        // The target is within distance. Keep moving towards it.
                        agents[i].AttackPosition = true;
                        if (MoveToAttackPosition(i)) {
                            agents[i].TryAttack();
                        }
                    }
                }

                // The agent isn't attacking. Move near the defend object.
                if (!agents[i].AttackPosition) {
                    var targetPosition = defendObject.Value.transform.TransformPoint(radius.Value * Mathf.Sin(theta * i), 0, radius.Value * Mathf.Cos(theta * i));
                    agents[i].SetDestination(targetPosition);
                    if (agents[i].HasArrived()) {
                        // Face away from the defending object.
                        var direction = targetPosition - defendObject.Value.transform.position;
                        direction.y = 0;
                        agents[i].RotateTowards(Quaternion.LookRotation(direction));
                    }
                }
            }

            return TaskStatus.Running;
        }

        public override void OnReset()
        {
            base.OnReset();

            defendObject = null;
            radius = 5;
            defendRadius = 10;
        }
    }
}