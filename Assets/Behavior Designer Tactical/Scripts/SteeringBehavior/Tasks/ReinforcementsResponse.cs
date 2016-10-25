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
    [TaskDescription("Responds to a reinforcement request. Will move towards the requesting agent and start attacking as soon as the target is within distance")]
    [HelpURL("http://www.opsive.com/assets/BehaviorDesigner/Tactical/documentation.php?id=13")]
    [TaskIcon("Assets/Behavior Designer Tactical/Editor/Icons/{SkinColor}ReinforcementsResponseIcon.png")]
    public class ReinforcementsResponse : SteeringBehaviorTacticalGroup
    {
        [Tooltip("A list of agents that may call for reinforcements")]
        public SharedGameObjectList listenForReinforcements;

        private Transform requestTransform; 

        public override void OnAwake()
        {
            base.OnAwake();

            // Listen to any behavior trees that could request reinforcements.
            for (int i = 0; i < listenForReinforcements.Value.Count; ++i) {
                var behaviorTrees = listenForReinforcements.Value[i].GetComponents<BehaviorTree>();
                for (int j = 0; j < behaviorTrees.Length; ++j) {
                    behaviorTrees[j].RegisterEvent<GameObject>("RequestReinforcements", OnReinforcementsRequest);
                }
            }
        }

        public override TaskStatus OnUpdate()
        {
            base.OnUpdate();

            if (requestTransform != null) {
                for (int i = 0; i < agents.Count; ++i) {
                    // Start attacking as soon as the agent has arrived close to the reinforcement position.
                    if (agents[i].AttackPosition || Vector3.Distance(agents[i].Transform.position, requestTransform.position) <= agents[i].AttackAgent.AttackDistance()) {
                        FindAttackTarget(i);
                        if (MoveToAttackPosition(i)) {
                            agents[i].TryAttack();
                        }
                    } else {
                        // Move to the agent requesting reinforcements.
                        agents[i].SetDestination(requestTransform.position);
                    }
                }
            }
            return TaskStatus.Running;
        }

        private void OnReinforcementsRequest(GameObject requestGameObject)
        {
            requestTransform = requestGameObject.transform;
        }

        public override void OnReset()
        {
            base.OnReset();

            listenForReinforcements.Value.Clear();
        }
    }
}