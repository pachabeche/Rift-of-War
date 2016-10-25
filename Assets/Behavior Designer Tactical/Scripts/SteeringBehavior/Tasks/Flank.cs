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
    [TaskDescription("Flanks the target from the left and right")]
    [HelpURL("http://www.opsive.com/assets/BehaviorDesigner/Tactical/documentation.php?id=4")]
    [TaskIcon("Assets/Behavior Designer Tactical/Editor/Icons/{SkinColor}FlankIcon.png")]
    public class Flank : SteeringBehaviorTacticalGroup
    {
        [Tooltip("Should the agents flank from both the left and right side?")]
        public SharedBool dualFlank = false;
        [Tooltip("The amount of time the left and right groups should wait after the center group has started to attack")]
        public SharedFloat attackDelay = 0;
        [Tooltip("Optionally set an extra distance that the agents should first move towards. This will prevent the agents from crossing in front of the enemies")]
        public SharedFloat approachDistance = 2;
        [Tooltip("The distance that the agents should be separated while attacking")]
        public SharedFloat separation = 2;

        private float attackStartTime;
        private List<Vector3> destinationOffset = new List<Vector3>();
        private bool inPosition;

        public override void OnStart()
        {
            base.OnStart();

            attackStartTime = -1;
        }

        protected override void AddAgentToGroup(Transform agent)
        {
            base.AddAgentToGroup(agent);

            // Determine the initial move to offset. This allows the agents to sneak up on the target without crossing directly in front of the target's field of view.
            var groupCount = dualFlank.Value ? 3 : 2;
            var groupIndex = (agents.Count - 1) % groupCount;
            var offset = Vector3.zero;
            if (groupIndex == 0) { // center
                offset.Set(0, 0, agents[agents.Count - 1].AttackAgent.AttackDistance());
            } else if (groupIndex == 1) { // right
                offset.Set(-agents[agents.Count - 1].AttackAgent.AttackDistance() - approachDistance.Value, 0, 0);
            } else { // left
                offset.Set(agents[agents.Count - 1].AttackAgent.AttackDistance() + approachDistance.Value, 0, 0);
            }
            destinationOffset.Add(offset);
        }

        protected override int RemoveAgentFromGroup(Transform agent)
        {
            var index = base.RemoveAgentFromGroup(agent);

            if (index != -1) {
                destinationOffset.RemoveAt(index);
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
            var centerRotation = CenterAttackRotation(attackCenter);
            var groupCount = dualFlank.Value ? 3 : 2;

            inPosition = true;
            for (int i = 0; i < agents.Count; ++i) {
                if (!agents[i].AttackPosition) {
                    var groupIndex = i % groupCount;
                    var destination = TransformPoint(attackCenter, destinationOffset[i], centerRotation);
                    // Arrange the agents in a row to prevent two agents from trying to move to the same destination.
                    if (i + 1 > groupCount) {
                        var offset = Vector3.zero;
                        offset.x += separation.Value * ((i / groupCount) % 2 == 0 ? -1 : 1) * (((groupIndex / 2) + 1));
                        destination = TransformPoint(destination, offset, Quaternion.LookRotation(attackCenter - destination));
                    }
                    agents[i].SetDestination(destination);
                    // Set AttackPosition to true when the agent arrived at the destination. This will put the agent in attack mode and start to rotate towards
                    // the target.
                    if (agents[i].HasArrived()) {
                        agents[i].AttackPosition = true;
                    }
                    inPosition = false;
                } else if (!MoveToAttackPosition(i)) {
                    // The agent isn't in position yet. One case of MoveToAttackPosition returning false is when the agent still needs to rotate to face the target.
                    inPosition = false;
                }
            }

            if (inPosition) {
                // All of the agents are in position. Start the attack.
                for (int i = 0; i < agents.Count; ++i) {
                    // Optionally allow the center group to attack first.
                    var center = i % groupCount == 0;
                    if ((center || attackStartTime + attackDelay.Value <= Time.time) && agents[i].TryAttack()) {
                        if (attackStartTime == -1 && center) {
                            // The center group has attacked. Set the attack start time.
                            attackStartTime = Time.time;
                        }
                    }
                }
            }

            return TaskStatus.Running;
        }

        public override void OnReset()
        {
            base.OnReset();

            dualFlank = false;
            attackDelay = 0;
            approachDistance = 2;
            separation = 2;
        }
    }
}