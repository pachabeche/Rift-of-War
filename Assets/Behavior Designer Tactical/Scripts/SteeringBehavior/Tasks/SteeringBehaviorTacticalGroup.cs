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

namespace BehaviorDesigner.Runtime.Tactical.Tasks.SteeringBehavior
{
    /// <summary>
    /// Base class for all steering behavior Tactical tasks.
    /// </summary>
    public abstract class SteeringBehaviorTacticalGroup : TacticalGroup
    {
        /// <summary>
        /// The SteeringBehaviorTacticalAgent class contains component references and variables for each steering behavior agent.
        /// </summary>
        protected class SteeringBehaviorTacticalAgent : TacticalAgent
        {
            private SteeringBehaviorAgent steeringBehaviorAgent;
            private Vector3 targetDestination;
            private bool destinationSet = false;
            private bool updateRotation = true;

            /// <summary>
            /// Caches the component references.
            /// </summary>
            public SteeringBehaviorTacticalAgent(Transform agent) : base(agent)
            {
                steeringBehaviorAgent = agent.GetComponent<SteeringBehaviorAgent>();
            }

            /// <summary>
            /// Sets the destination.
            /// </summary>
            public override void SetDestination(Vector3 destination)
            {
                targetDestination = destination;
                targetDestination.y = Transform.position.y;
                destinationSet = true;
            }

            /// <summary>
            /// Has the agent arrived at its destination?
            /// </summary>
            public override bool HasArrived()
            {
                return destinationSet && Vector3.Distance(Transform.position, targetDestination) <= steeringBehaviorAgent.stoppingDistance + 0.01f;
            }

            /// <summary>
            /// Move towards the target destination
            /// </summary>
            public void MoveAndRotate()
            {
                if (destinationSet) {
                    Transform.position = Vector3.MoveTowards(Transform.position, targetDestination, steeringBehaviorAgent.moveSpeed * Time.deltaTime);
                    if (updateRotation) {
                        Transform.rotation = Quaternion.RotateTowards(Transform.rotation, Quaternion.LookRotation(targetDestination - Transform.position), steeringBehaviorAgent.rotationSpeed * Time.deltaTime);
                    }
                }
            }

            /// <summary>
            /// Rotates towards the target rotation.
            /// </summary>
            public override bool RotateTowards(Quaternion targetRotation)
            {
                Transform.rotation = Quaternion.RotateTowards(Transform.rotation, targetRotation, steeringBehaviorAgent.rotationSpeed * Time.deltaTime);
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
                return steeringBehaviorAgent.radius;
            }

            /// <summary>
            /// Starts or stops the rotation from updating. Not all implementations will use this.
            /// </summary>
            public override void UpdateRotation(bool update)
            {
                updateRotation = update;
            }

            /// <summary>
            /// Stops the agent from moving.
            /// </summary>
            public override void Stop()
            {
                destinationSet = false;
            }

            /// <summary>
            /// The task has ended. Perform any cleanup.
            /// </summary>
            public override void End()
            {
                Stop();
            }
        }

        /// <summary>
        /// Adds the agent to the agent list.
        /// </summary>
        /// <param name="agent">The agent to add.</param>
        protected override void AddAgentToGroup(Transform agent)
        {
            agents.Add(new SteeringBehaviorTacticalAgent(agent));
        }

        /// <summary>
        /// Moves all of the agents.
        /// </summary>
        public override TaskStatus OnUpdate()
        {
            TaskStatus status;
            if ((status = base.OnUpdate()) != TaskStatus.Running) {
                return status;
            }

            // Move all of the agents.
            for (int i = 0; i < agents.Count; ++i) {
                if (!agents[i].HasArrived()) {
                    (agents[i] as SteeringBehaviorTacticalAgent).MoveAndRotate();
                }
            }

            return TaskStatus.Running;
        }
    }
}