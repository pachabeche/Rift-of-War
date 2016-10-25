using UnityEngine;

namespace BehaviorDesigner.Runtime.Tactical
{
    /// <summary>
    /// The Steering Behavior Agent acts independent of any pathfinding implementation and is a basic set of steering behaviors.
    /// </summary>
    public class SteeringBehaviorAgent : MonoBehaviour
    {
        // The speed to move the agent
        public float moveSpeed;
        // The speed to rotate the agent
        public float rotationSpeed;
        // The raidus of the agent
        public float radius;
        // How close to the destination the agent should stop
        public float stoppingDistance;
    }
}