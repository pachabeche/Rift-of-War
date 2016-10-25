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
    [TaskDescription("Move towards the target. The agents will start attacking when they are within distance")]
    [HelpURL("http://www.opsive.com/assets/BehaviorDesigner/Tactical/documentation.php?id=3")]
    [TaskIcon("Assets/Behavior Designer Tactical/Editor/Icons/{SkinColor}MarchingFireIcon.png")]
    public class MarchingFire : SteeringBehaviorTacticalGroup
    {
        [Tooltip("The number of agents that should be in a row")]
        public SharedInt agentsPerRow = 2;
        [Tooltip("The separation between agents")]
        public SharedVector2 separation = new Vector2(2, 2);
        [Tooltip("The distance to stop marching fire and continue attacking")]
        public SharedFloat attackDistance = 2;

        private List<Vector3> offset = new List<Vector3>();
        private bool inPosition;

        protected override void AddAgentToGroup(Transform agent)
        {
            base.AddAgentToGroup(agent);

            var row = offset.Count / agentsPerRow.Value;
            var column = offset.Count % agentsPerRow.Value;

            // Arrange the agents in charging position.
            if (column == 0) {
                offset.Add(new Vector3(0, 0, -separation.Value.y * row));
            } else {
                offset.Add(new Vector3(separation.Value.x * (column % 2 == 0 ? -1 : 1) * (((column - 1) / 2) + 1), 0, -separation.Value.y * row));
            }
            inPosition = false;
        }

        protected override int RemoveAgentFromGroup(Transform agent)
        {
            var index = base.RemoveAgentFromGroup(agent);
            if (index != -1) {
                offset.RemoveAt(index);
            }
            return index;
        }

        public override TaskStatus OnUpdate()
        {
            var baseStatus = base.OnUpdate();
            if (baseStatus != TaskStatus.Running || agents.Count == 0) {
                return baseStatus;
            }
            var attackCenter = CenterAttackPosition();
            var attackRotation = ReverseCenterAttackRotation(attackCenter);
            // Move the agents into their starting position if they haven't been there already.
            if (!inPosition) {
                inPosition = true;
                for (int i = 0; i < agents.Count; ++i) {
                    var destination = TransformPoint(agents[0].Transform.position, offset[i], attackRotation);
                    if (agents[i].HasArrived()) {
                        // The agent is in position but it may not be facing the target.
                        if (!agents[i].RotateTowardsPosition(TransformPoint(attackCenter, offset[i], attackRotation))) {
                            inPosition = false;
                        }
                    } else {
                        agents[i].SetDestination(destination);
                        inPosition = false;
                    }
                }
            } else {
                // All of the agents are in position. Start moving towards the attack point until the agents get within attack distance. Once they are
                // within attack distance they should start attacking and stop marching fire. The agents can attack while they are moving into position.
                for (int i = 0; i < agents.Count; ++i) {
                    var destination = TransformPoint(attackCenter, offset[i], attackRotation);
                    if (agents[i].AttackPosition || (destination - agents[i].Transform.position).magnitude <= attackDistance.Value) {
                        if (!agents[i].AttackPosition) {
                            agents[i].AttackPosition = true;
                        }
                        MoveToAttackPosition(i);
                        agents[i].TryAttack();
                    } else {
                        // The agent isn't in position yet. Set the destination and try to attack if the agent is within the attack distance.
                        agents[i].SetDestination(destination);
                        FindAttackTarget(i);
                        if ((agents[i].Transform.position - agents[i].TargetTransform.position).magnitude <= agents[i].AttackAgent.AttackDistance()) {
                            if (agents[i].RotateTowardsPosition(agents[i].TargetTransform.position)) {
                                agents[i].TryAttack();
                            }
                        }
                    }
                }
            }

            return TaskStatus.Running;
        }

        public override void OnReset()
        {
            base.OnReset();

            agentsPerRow = 2;
            separation = new Vector2(2, 2);
            attackDistance = 2;
        }
    }
}