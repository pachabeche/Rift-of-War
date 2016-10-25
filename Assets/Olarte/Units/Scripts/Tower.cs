using UnityEngine;
using System.Collections.Generic;

public class Tower : MonoBehaviour
{
    public float range = 25;
    public GameObject bulletPrefab;
    public float firecd = 1.5f;
    public string tag1 = "None";
    public string tag2 = "None";

    private float firecdleft = 0;

    GameObject FindNearestEnemy()
    {
        GameObject nearestEnemy = null;
        var colliders = Physics.OverlapSphere(transform.position, range);
        for (int i = 0; i < colliders.Length; ++i)
            if (colliders[i].tag == tag1 || colliders[i].tag == tag2)
                    nearestEnemy = colliders[i].gameObject;
        return nearestEnemy;
    }

    void Update()
    {
        firecdleft -= Time.deltaTime;
        if (firecdleft <= 0)
        {
            firecdleft = firecd;
            if (FindNearestEnemy() != null)
                shootat(FindNearestEnemy());
        }

    }
    void shootat(GameObject e)
    {
        try
        {
            Vector3 desiredpos = this.transform.position;
            desiredpos.y += 8;
            GameObject bulletGo = (GameObject)Instantiate(bulletPrefab, desiredpos, this.transform.rotation);

            Bullet b = bulletGo.GetComponent<Bullet>();
            b.target = e;
        }
        catch { }
    }
}
