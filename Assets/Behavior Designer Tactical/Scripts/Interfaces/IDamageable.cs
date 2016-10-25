using UnityEngine;

namespace BehaviorDesigner.Runtime.Tactical
{
    /// <summary>
    /// Interface for objects that can take damage.
    /// </summary>
    public interface IDamageable
    {
        /// <summary>
        /// Take damage by the specified amount.
        /// </summary>
        /// <param name="amout">The amount of damage to take.</param>
        void Damage(float amout);

        /// <summary>
        /// Is the object currently alive?
        /// </summary>
        /// <returns>True if the object is alive.</returns>
        bool IsAlive();
    }
}