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
    [TaskDescription("Attacks the target and moves position after a short amount of time")]
    [HelpURL("http://www.opsive.com/assets/BehaviorDesigner/Tactical/documentation.php?id=6")]
    [TaskIcon("Assets/Behavior Designer Tactical/Editor/Icons/{SkinColor}ShootAndScootIcon.png")]
    public class ShootAndScoot : SteeringBehaviorTacticalGroup
    {
        [Tooltip("The number of agents that should be in a row")]
        public SharedInt agentsPerRow = 2;
        [Tooltip("The separation between agents")]
        public SharedVector2 separation = new Vector2(2, 2);
        [Tooltip("The amount of time that should elapse before moving to the next attack point")]
        public SharedFloat timeStationary = 2;
        [Tooltip("When moving positions the agents will move based on a new random angle. The mimium move angle specifies the minimum random angle")]
        public SharedFloat minMoveAngle = 10;
        [Tooltip("When moving positions the agents will move based on a new random angle. The maximum move angle specifies the maximum random angle")]
        public SharedFloat maxMoveAngle = 20;
        [Tooltip("When moving positions the agents will move based on a new random radius. The minimum attack radius specifies the minimum radius")]
        public SharedFloat minAttackRadius = 5;
        [Tooltip("When moving positions the agents will move based on a new random radius. The maximum attack radius specifies the maximum radius")]
        public SharedFloat maxAttackRadius = 10;

        private List<Vector3> offset = new List<Vector3>();
        private float currentAngle;
        private float arrivalTime;
        private bool inPosition;
        private float attackRadius;
        private bool determinePosition;

        public override void OnStart()
        {
            base.OnStart();

            arrivalTime = -timeStationary.Value;
            determinePosition = true;
        }

        protected override void AddAgentToGroup(Transform agent)
        {
            base.AddAgentToGroup(agent);

            var row = offset.Count / agentsPerRow.Value;
            var column = offset.Count % agentsPerRow.Value;

            // Each agent will always move to their respective offset when attacking.
            if (column == 0) {
                offset.Add(new Vector3(0, 0, -separation.Value.y * row));
            } else {
                offset.Add(new Vector3(separation.Value.x * (column % 2 == 0 ? -1 : 1) * (((column - 1) / 2) + 1), 0, -separation.Value.y * row));
            }
        }

        protected override void StartGroup()
        {
            base.StartGroup();

            // Determine a starting attack angle.
            var attackCenter = CenterAttackPosition();
            var diff = transform.position - attackCenter;
            diff.y = 0;
            // Get an angle in the range of 0 - 360.
            currentAngle = Mathf.Sign(Vector3.Dot(diff, Vector3.right)) * Vector3.Angle(diff, Vector3.forward);
            inPosition = true;
        }

        public override TaskStatus OnUpdate()
        {
            var baseStatus = base.OnUpdate();
            if (baseStatus != TaskStatus.Running || agents.Count == 0) {
                return baseStatus;
            }

            // Move to the attack position if the agents currently are not in position or if it's time to move to a new position.
            if (!inPosition || arrivalTime + timeStationary.Value < Time.time) {
                var attackCenter = CenterAttackPosition();
                var attackRotation = ReverseCenterAttackRotation(attackCenter);

                // Only determine a new angle and radius once while moving into position.
                if (determinePosition) {
                    currentAngle += Random.Range(minMoveAngle.Value, maxMoveAngle.Value) * (Random.value > 0.5f ? 1 : -1);
                    attackRadius = Random.Range(minAttackRadius.Value, maxAttackRadius.Value);
                    determinePosition = false;
                }

                // Position on the circumference of the circle.
                attackCenter.x += Mathf.Sin(currentAngle * Mathf.Deg2Rad) * attackRadius;
                attackCenter.z += Mathf.Cos(currentAngle * Mathf.Deg2Rad) * attackRadius;

                inPosition = true;
                for (int i = 0; i < agents.Count; ++i) {
                    agents[i].SetDestination(TransformPoint(attackCenter, offset[i], attackRotation));

                    if (agents[i].HasArrived()) {
                        // The agents are not in position until they are looking at the target.
                        FindAttackTarget(i);
                        if (!agents[i].RotateTowardsPosition(agents[i].TargetTransform.position)) {
                            inPosition = false;
                        }
                    } else {
                        // The agents still need to move to the position.
                        inPosition = false;
                    }
                }
                if (inPosition) {
                    // The agnets are in position. Set the arrival time so they will move to a new position after timeStationary.
                    arrivalTime = Time.time;
                    determinePosition = true;
                }
            } else {
                // The agents are in position and looking at their target. Attack.
                for (int i = 0; i < agents.Count; ++i) {
                    agents[i].TryAttack();
                }
            }

            return TaskStatus.Running;
        }

        public override void OnReset()
        {
            base.OnReset();

            agentsPerRow = 2;
            separation = new Vector2(2, 2);
            timeStationary = 2;
            minMoveAngle = 10;
            maxMoveAngle = 20;
            minAttackRadius = 5;
            maxAttackRadius = 10;
        }
    }
}