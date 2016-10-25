using UnityEngine;
using System.Collections;

public class Bullet : MonoBehaviour
{
    public float speed = 15;
    public GameObject target;
    public int damage = 1;
    public float h = 0;

    void Update()
    {
        Vector3 dir = new Vector3();
        if (target != null)
            dir = target.transform.position - this.transform.localPosition;
        else
            Destroy(gameObject);
        float distThisFrame = speed * Time.deltaTime;
        if (dir.magnitude <= distThisFrame)
        {
            bullethit();
        }
        else
        {
            transform.Translate(dir.normalized * distThisFrame, Space.World);
            Quaternion targetRotation = Quaternion.LookRotation(dir);
            this.transform.rotation = Quaternion.Lerp(this.transform.rotation, targetRotation, Time.deltaTime * 5);
        }
    }
    void bullethit()
    {
        try {target.GetComponent<Life>().TakeDamage(damage);}catch { }
        Destroy(gameObject);
    }
}
