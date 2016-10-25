using UnityEngine;
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
    [TaskDescription("Tells the leader that the current agent is ready to follow its orders")]
    [HelpURL("http://www.opsive.com/assets/BehaviorDesigner/Tactical/documentation.php?id=14")]
    [TaskIcon("Assets/Behavior Designer Tactical/Editor/Icons/{SkinColor}FollowOrdersIcon.png")]
    public class FollowOrders : Action
    {
        [Tooltip("The leader to follow")]
        public SharedGameObject leader;

        private GameObject prevLeader;
        private BehaviorTree[] leaderTrees;
        private bool sendListenerEvent;
        private TaskStatus runStatus;

        public override void OnStart()
        {
            var currentLeader = GetDefaultGameObject(leader.Value);
            if (currentLeader != prevLeader) {
                prevLeader = currentLeader;
                leaderTrees = leader.Value.GetComponents<BehaviorTree>();
                if (leaderTrees.Length == 0) {
                    Debug.LogError("Error: The leader doesn't have a behavior tree component.");
                }
            }

            if (leaderTrees == null || leaderTrees.Length == 0) {
                runStatus = TaskStatus.Failure;
            } else {
                Owner.RegisterEvent<TaskStatus>("OrdersFinished", OrdersFinished);
                runStatus = TaskStatus.Running;
                sendListenerEvent = true;
            }
        }

        public override TaskStatus OnUpdate()
        {
            // Send within OnUpdate to ensure the at least one leader behavior tree is active. If registered within OnStart there is a chance that the behavior tree
            // isn't active yet and will never receive the event.
            if (sendListenerEvent) {
                // Listen to orders from any of the behavior trees on the leader
                for (int i = 0; i < leaderTrees.Length; ++i) {
                    if (leaderTrees[i].ExecutionStatus == TaskStatus.Running) {
                        leaderTrees[i].SendEvent<GameObject>("StartListeningForOrders", gameObject);
                        sendListenerEvent = false;
                    }
                }
            }
            return runStatus;
        }

        private void OrdersFinished(TaskStatus status)
        {
            runStatus = status;
            for (int i = 0; i < leaderTrees.Length; ++i) {
                leaderTrees[i].SendEvent<GameObject>("StopListeningToOrders", gameObject);
            }
            Owner.UnregisterEvent<TaskStatus>("OrdersFinished", OrdersFinished);
        }

        public override void OnEnd()
        {
            for (int i = 0; i < leaderTrees.Length; ++i) {
                leaderTrees[i].SendEvent<GameObject>("StopListeningToOrders", gameObject);
            }
            Owner.UnregisterEvent<TaskStatus>("OrdersFinished", OrdersFinished);
        }

        public override void OnReset()
        {
            leader = null;
        }
    }
}