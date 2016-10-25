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
    /// Base class for all Tactical tasks. This class knows about all of the agents that can attack and the attack targets.
    /// </summary>
    public abstract class TacticalGroup : Action
    {
        [Tooltip("The objects to attack. If blank the targetTag will be used")]
        public SharedGameObjectList targetGroup;
        [Tooltip("The tag of the objects to attack. Will be used if targetGroup has no elements")]
        public SharedString targetTag;
        [Tooltip("The amount of time to wait until the group starts to form")]
        public SharedFloat waitTime = 0;
        [Tooltip("Does this agent act independently?")]
        public SharedBool independent;

        protected List<TacticalAgent> agents = new List<TacticalAgent>();
        protected List<IDamageable> targets = new List<IDamageable>();
        protected List<Transform> targetTransforms = new List<Transform>();
        private List<GameObject> pendingGroupPlacement = new List<UnityEngine.GameObject>();

        /// <summary>
        /// Listen for any agents that want to join the group.
        /// </summary>
        public override void OnAwake()
        {
            if (!independent.Value) {
                Owner.RegisterEvent<GameObject>("StartListeningForOrders", StartListeningForOrders);
                Owner.RegisterEvent<GameObject>("StopListeningToOrders", StopListeningToOrders);
            }
        }

        /// <summary>
        /// Start forming the group immediately on start or after a set amount of time.
        /// </summary>
        public override void OnStart()
        {
            if (waitTime.Value == 0) {
                StartGroup();
            } else {
                StartCoroutine(WaitForGroup());
            }
        }

        /// <summary>
        /// Wait a small amount of time before the group is formed.
        /// </summary>
        private IEnumerator WaitForGroup()
        {
            yield return new WaitForSeconds(waitTime.Value);

            StartGroup();
        }

        /// <summary>
        /// Start forming the group.
        /// </summary>
        protected virtual void StartGroup()
        {
            // Clear the old group.
            agents.Clear();

            AddAgentToGroup(transform);
            for (int i = 0; i < pendingGroupPlacement.Count; ++i) {
                AddAgentToGroup(pendingGroupPlacement[i].transform);
            }

            targets.Clear();
            targetTransforms.Clear();
            if (targetGroup.Value.Count > 0) {
                for (int i = 0; i < targetGroup.Value.Count; ++i) {
#if UNITY_4_3 || UNITY_4_4 || UNITY_4_5
                    var damagable = (targetGroup.Value[i].GetComponent(typeof(IDamageable)) as IDamageable);
#else
                    var damagable = (targetGroup.Value[i].GetComponentInParent(typeof(IDamageable)) as IDamageable);
#endif
                    if (damagable != null) {
                        targets.Add(damagable);
                        targetTransforms.Add(targetGroup.Value[i].transform);
                    }
                }
            } else {
                var foundAttackGroup = GameObject.FindGameObjectsWithTag(targetTag.Value);
                for (int i = 0; i < foundAttackGroup.Length; ++i) {
#if UNITY_4_3 || UNITY_4_4 || UNITY_4_5
                    var damagable = (foundAttackGroup[i].GetComponent(typeof(IDamageable)) as IDamageable);
#else
                    var damagable = (foundAttackGroup[i].GetComponentInParent(typeof(IDamageable)) as IDamageable);
#endif
                    if (damagable != null) {
                        targets.Add(damagable);
                        targetTransforms.Add(foundAttackGroup[i].transform);
                    }
                }
            }

            if (targets.Count == 0) {
                Debug.LogError("Error: no target GameObjects have been found.");
            }
        }

        /// <summary>
        /// An agent wants to join the formation. Add them to the pending group placement list if the group hasn't formed yet, otherwise directly add them to the existing formation.
        /// </summary>
        /// <param name="obj">The agent that wants to join the group.</param>
        protected void StartListeningForOrders(GameObject obj)
        {
            // Add the agent to the pending group placement list if the group hasn't formed yet.
            if (agents == null || agents.Count == 0) {
                pendingGroupPlacement.Add(obj);
            } else { // The group is already in formation so add the new agent.
                AddAgentToGroup(obj.transform);
            }
        }

        /// <summary>
        /// Adds the agent to the agent list.
        /// </summary>
        /// <param name="agent">The agent to add.</param>
        protected abstract void AddAgentToGroup(Transform agent);

        /// <summary>
        /// Base OnUpdate method. Return success if no targets are alive, otherwise return running.
        /// </summary>
        public override TaskStatus OnUpdate()
        {
            // There won't be any agents in the group if the group hasn't formed yet.
            if (agents.Count == 0) {
                return TaskStatus.Running;
            }

            // Remove any targets that are no logner alive.
            for (int i = targets.Count - 1; i > -1; --i) {
                if (!targets[i].IsAlive()) {
                    targets.RemoveAt(i);
                    targetTransforms.RemoveAt(i);
                }
            }

            // The task succeeded if no more targets are alive.
            if (targets.Count == 0) {
                return TaskStatus.Success;
            }

            return TaskStatus.Running;
        }

        /// <summary>
        /// Returns the center position of all of the target transforms.
        /// </summary>
        /// <returns>The center position of all of the target transforms.</returns>
        protected Vector3 CenterAttackPosition()
        {
            var position = Vector3.zero;
            for (int i = 0; i < targetTransforms.Count; ++i) {
                position += targetTransforms[i].position;
            }
            return position / targetTransforms.Count;
        }

        /// <summary>
        /// Returns the look rotation from the target to the center position.
        /// </summary>
        /// <param name="centerPosition">The position of the attack center.</param>
        /// <returns>The look rotation from the target to the center position.</returns>
        protected Quaternion CenterAttackRotation(Vector3 centerPosition)
        {
            var direction = transform.position - centerPosition;
            direction.y = 0;
            return Quaternion.LookRotation(direction);
        }

        /// <summary>
        /// Returns the look rotation from the center position to the target.
        /// </summary>
        /// <param name="centerPosition">The position of the center position.</param>
        /// <returns>The look roation from the center position to the target.</returns>
        protected Quaternion ReverseCenterAttackRotation(Vector3 centerPosition)
        {
            var direction = centerPosition - transform.position;
            direction.y = 0;
            return Quaternion.LookRotation(direction);
        }

        /// <summary>
        /// Finds the closest target transform to the agent transform.
        /// </summary>
        /// <param name="agentTransform">The transform of the agent.</param>
        /// <param name="targetTransform">The returned target transform.</param>
        /// <param name="targetDamagable">The returned IDamagable reference.</param>
        protected void ClosestTarget(Transform agentTransform, ref Transform targetTransform, ref IDamageable targetDamagable)
        {
            var distance = float.MaxValue;
            var localDistance = 0f;
            for (int i = targetTransforms.Count - 1; i > -1; --i) {
                if (targets[i].IsAlive()) {
                    if ((localDistance = (targetTransforms[i].position - agentTransform.position).sqrMagnitude) < distance) {
                        distance = localDistance;
                        targetTransform = targetTransforms[i];
                        targetDamagable = targets[i];
                    }
                } else {
                    targets.RemoveAt(i);
                    targetTransforms.RemoveAt(i);
                }
            }
        }

        /// <summary>
        /// Finds a target transform closest to the agent.
        /// </summary>
        protected void FindAttackTarget(int index)
        {
            if (agents[index].TargetTransform == null || !agents[index].TargetDamagable.IsAlive()) {
                Transform target = null;
                IDamageable damageable = null;
                ClosestTarget(agents[index].Transform, ref target, ref damageable);
                agents[index].TargetTransform = target;
                agents[index].TargetDamagable = damageable;
            }
        }

        /// <summary>
        /// Moves the agent towards and rotates towards the target transform.
        /// </summary>
        protected bool MoveToAttackPosition(int index)
        {
            FindAttackTarget(index);
            if (agents[index].TargetTransform == null) {
                return false;
            }
            if (!agents[index].CanSeeTarget() ||
                    Vector3.Distance(agents[index].TargetTransform.position, agents[index].Transform.position) > agents[index].AttackAgent.AttackDistance()) {
                agents[index].SetDestination(agents[index].TargetTransform.position);
                agents[index].AttackPosition = true;
            } else {
                agents[index].Stop();

                return agents[index].RotateTowardsPosition(agents[index].TargetTransform.position);
            }
            return false;
        }

        /// <summary>
        /// The task has ended. Stop any active agents.
        /// </summary>
        public override void OnEnd()
        {
            for (int i = agents.Count - 1; i > -1; --i) {
                var count = agents.Count;
                if (!agents[i].HasArrived()) {
                    agents[i].Transform.GetComponent<BehaviorTree>().SendEvent<TaskStatus>("OrdersFinished", TaskStatus.Failure);
                }
                // The individual agent may remove themselves from the group with the OrdersFinished event so do a check beforehand to ensure
                // the count hasn't changed (which means the agent needs to be removed from the group here).
                if (count == agents.Count) {
                    RemoveAgentFromGroup(i);
                }
            }

            pendingGroupPlacement.Clear();
        }

        /// <summary>
        /// An agent has dropped out of the group so it should be removed.
        /// </summary>
        /// <param name="obj">The agent to remove.</param>
        protected void StopListeningToOrders(GameObject obj)
        {
            // The agent may drop before the group is formed so in that case just remove it from the pending list.
            for (int i = 0; i < pendingGroupPlacement.Count; ++i) {
                if (pendingGroupPlacement[i].Equals(obj)) {
                    pendingGroupPlacement.RemoveAt(i);
                    return;
                }
            }

            // The group has been formed, remove it from the group list.
            RemoveAgentFromGroup(obj.transform);
        }

        /// <summary>
        /// Removes the agent from the group.
        /// </summary>
        /// <param name="agent">The agent to remove.</param>
        /// <returns>The index of the agent removed from the group.</returns>
        protected virtual int RemoveAgentFromGroup(Transform agent)
        {
            for (int i = 0; i < agents.Count; ++i) {
                if (agents[i].Transform.Equals(agent)) {
                    RemoveAgentFromGroup(i);
                    return i;
                }
            }
            return -1;
        }

        /// <summary>
        /// Removes the agent from the group based on group index.
        /// </summary>
        /// <param name="index">The index to remove.</param>
        private void RemoveAgentFromGroup(int index)
        {
            agents[index].End();
            agents.RemoveAt(index);
        }

        /// <summary>
        /// The behavior tree is complete so the task should stop listening for the events.
        /// </summary>
        public override void OnBehaviorComplete()
        {
            Owner.UnregisterEvent<GameObject>("StartListeningForOrders", StartListeningForOrders);
            Owner.UnregisterEvent<GameObject>("StopListeningToOrders", StopListeningToOrders);
        }

        /// <summary>
        /// Reset the public variables back to their defaults.
        /// </summary>
        public override void OnReset()
        {
            targetGroup = null;
            targetTag = "";
            waitTime = 0;
        }

        /// <summary>
        /// Transforms position from local space to world space.
        /// </summary>
        protected static Vector3 TransformPoint(Vector3 worldPosition, Vector3 localOffset, Quaternion rotation)
        {
            return worldPosition + rotation * localOffset;
        }

        /// <summary>
        /// Transforms position from world space to local space.
        /// </summary>
        protected static Vector3 InverseTransformPoint(Vector3 position1, Vector3 position2, Quaternion rotation)
        {
            return Quaternion.Inverse(rotation) * (position1 - position2);
        }
    }
}