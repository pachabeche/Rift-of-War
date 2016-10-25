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
    [TaskDescription("Search for the target by forming two groups and leapfrogging each other. Both groups will start attacking as soon as the target is within sight")]
    [HelpURL("http://www.opsive.com/assets/BehaviorDesigner/Tactical/documentation.php?id=7")]
    [TaskIcon("Assets/Behavior Designer Tactical/Editor/Icons/{SkinColor}LeapfrogIcon.png")]
    public class Leapfrog : NavMeshTacticalGroup
    {
        [Tooltip("The horizontal separation between agents within the group")]
        public SharedFloat separation = 2;
        [Tooltip("The horizontal separation between the two groups")]
        public SharedFloat groupSeparation = 10;
        [Tooltip("The distance of one leap")]
        public SharedFloat leapDistance = 10;

        private List<Vector3> destinationOffset = new List<Vector3>();
        private bool inPosition;
        private bool moveFirstGroup;
        private bool attack;

        public override void OnStart()
        {
            base.OnStart();

            moveFirstGroup = false;
            attack = false;
        }

        protected override void AddAgentToGroup(Transform agent)
        {
            base.AddAgentToGroup(agent);

            var groupIndex = (agents.Count - 1) % 2;
            var offset = Vector3.zero;
            offset.x = separation.Value * (groupIndex == 0 ? -1 : 1) * (int)((agents.Count - 1) / 2);
            if (groupIndex == 1) {
                offset.x += groupSeparation.Value; 
            }
            destinationOffset.Add(offset);

            inPosition = false;
        }

        protected override int RemoveAgentFromGroup(Transform agent)
        {
            int index = base.RemoveAgentFromGroup(agent);

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

            // Move the agents into their starting position if they haven't been there already.
            if (!inPosition) {
                inPosition = true;
                for (int i = 0; i < agents.Count; ++i) {
                    agents[i].SetDestination(TransformPoint(agents[0].Transform.position, destinationOffset[i], agents[0].Transform.rotation));
                    if (agents[i].HasArrived()) {
                        // The agent is in position but it may not be facing the target.
                        if (!agents[i].RotateTowards(agents[0].Transform.rotation)) {
                            inPosition = false;
                        }
                    } else {
                        inPosition = false;
                    }
                }
            } else {
                // The agents are in position. Start leapfrogging each other until a target is detected. When a target is detected have all of the agents attack.
                var arrived = true;
                for (int i = 0; i < agents.Count; ++i) {
                    if (attack) {
                        // An target has been detected. All units should attack.
                        if (MoveToAttackPosition(i)) {
                            agents[i].TryAttack();
                        }
                    } else {
                        // Keep searching for a target as the agents are moving.
                        FindAttackTarget(i);
                        if (agents[i].CanSeeTarget()) {
                            attack = true;
                        }

                        // Don't do an arrived check unless the agent is moving into position.
                        if ((moveFirstGroup && i % 2 == 0) || (!moveFirstGroup && i % 2 == 1)) {
                            continue;
                        }

                        // The agent is moving into position. Check to see if the agent has arrived.
                        if (!agents[i].HasArrived()) {
                            arrived = false;
                        }
                    }
                }

                // If the agents have arrived and are not attacking then alternate which group moves.
                if (arrived && !attack) {
                    moveFirstGroup = !moveFirstGroup;
                    var moveOffset = Vector3.zero;
                    moveOffset.z += leapDistance.Value * (moveFirstGroup ? 1 : 2);
                    for (int i = 0; i < agents.Count; ++i) {
                        // Don't set the destination if the group shouldn't move yet.
                        if ((moveFirstGroup && i % 2 == 0) || (!moveFirstGroup && i % 2 == 1)) {
                            continue;
                        }
                        agents[i].SetDestination(TransformPoint(agents[0].Transform.position, destinationOffset[i] + moveOffset, agents[0].Transform.rotation));
                    }
                }
            }
            return TaskStatus.Running;
        }

        public override void OnReset()
        {
            base.OnReset();

            separation = 2;
            groupSeparation = 10;
            leapDistance = 10;
        }
    }
}