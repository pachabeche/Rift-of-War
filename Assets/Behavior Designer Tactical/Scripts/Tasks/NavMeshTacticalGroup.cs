using UnityEngine;
using System.Collections;
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
    /// <summary>
    /// Base class for all NavMeshAgent Tactical tasks.
    /// </summary>
    public abstract class NavMeshTacticalGroup : TacticalGroup
    {
        /// <summary>
        /// The NavMeshTacticalAgent class contains component references and variables for each NavMeshAgent.
        /// </summary>
        private class NavMeshTacticalAgent : TacticalAgent
        {
            private NavMeshAgent navMeshAgent;
            private bool destinationSet;

            /// <summary>
            /// Caches the component references and initialize default values.
            /// </summary>
            public NavMeshTacticalAgent(Transform agent) : base(agent)
            {
                navMeshAgent = agent.GetComponent<NavMeshAgent>();

                navMeshAgent.enabled = true;
                if (navMeshAgent.hasPath) {
                    navMeshAgent.ResetPath();
                    navMeshAgent.Stop();
                }
            }

            /// <summary>
            /// Sets the destination.
            /// </summary>
            public override void SetDestination(Vector3 destination)
            {
                destinationSet = true;
                destination.y = navMeshAgent.destination.y;
                if (navMeshAgent.destination != destination) {
                    navMeshAgent.SetDestination(destination);
                }
            }

            /// <summary>
            /// Has the agent arrived at its destination?
            /// </summary>
            public override bool HasArrived()
            {
                return destinationSet && !navMeshAgent.pathPending && navMeshAgent.remainingDistance <= navMeshAgent.stoppingDistance;
            }

            /// <summary>
            /// Rotates towards the target rotation.
            /// </summary>
            public override bool RotateTowards(Quaternion targetRotation)
            {
                Transform.rotation = Quaternion.RotateTowards(Transform.rotation, targetRotation, navMeshAgent.angularSpeed * Time.deltaTime);
                if (Quaternion.Angle(Transform.rotation, targetRotation) < AttackAgent.AttackAngle()) {
                    return true;
                }
                return false;
            }

            /// <summary>
            /// Returns the radius of the agent.
            /// </summary>
            public override float Radius()
            {
                return navMeshAgent.radius;
            }

            /// <summary>
            /// Starts or stops the rotation from updating.
            /// </summary>
            public override void UpdateRotation(bool update)
            {
                navMeshAgent.updateRotation = update;
            }

            /// <summary>
            /// Stops the agent from moving.
            /// </summary>
            public override void Stop()
            {
                if (navMeshAgent.hasPath) {
                    navMeshAgent.Stop();
                    destinationSet = false;
                }
            }

            /// <summary>
            /// The task has ended. Perform any cleanup.
            /// </summary>
            public override void End()
            {
                Stop();
                navMeshAgent.updateRotation = true;
                navMeshAgent.velocity = Vector3.zero;
                navMeshAgent.enabled = false;
            }
        }

        /// <summary>
        /// Adds the agent to the agent list.
        /// </summary>
        /// <param name="agent">The agent to add.</param>
        protected override void AddAgentToGroup(Transform agent)
        {
            agents.Add(new NavMeshTacticalAgent(agent));
        }
    }
}