using UnityEngine;
using System.Collections;
using BehaviorDesigner.Runtime;

public class Inhibitor : MonoBehaviour
{
    public float cooldown = 10f;
    public GameObject unitPrefab;
    public GameObject archerPrefab;
    public Transform destination;
    public bool hasWP = false;
    public Transform waypoint = null;

    private float cdleft = 0f;
    private bool archerspawn = false;


    void Update()
    {
        cdleft -= Time.deltaTime;
        if (cdleft <= cooldown - 5 && !archerspawn && !(cdleft <= 0))
        {
            archerspawn = true;
            GameObject bulletGo = (GameObject)Instantiate(archerPrefab, this.transform.position, this.transform.rotation);
            SharedVector3 vec = new SharedVector3();
            SharedBool bll = new SharedBool();
            SharedVector3 vec2 = new SharedVector3();
            vec2.Value = waypoint.transform.position;
            bll.Value = hasWP;
            vec.Value = destination.transform.position;
            bulletGo.GetComponent<BehaviorTree>().SetVariable("target", vec);
            bulletGo.GetComponent<BehaviorTree>().SetVariable("hasWaypoint", bll);
            bulletGo.GetComponent<BehaviorTree>().SetVariable("waypoint", vec2);
        }
        if(cdleft <= 0)
        {
            cdleft = cooldown;
            archerspawn = false;
            GameObject bulletGo = (GameObject)Instantiate(unitPrefab, this.transform.position, this.transform.rotation);
            SharedVector3 vec = new SharedVector3();
            SharedBool bll = new SharedBool();
            SharedVector3 vec2 = new SharedVector3();
            vec2.Value = waypoint.transform.position;
            bll.Value = hasWP;
            vec.Value = destination.transform.position;
            bulletGo.GetComponent<BehaviorTree>().SetVariable("target", vec);
            bulletGo.GetComponent<BehaviorTree>().SetVariable("hasWaypoint", bll);
            bulletGo.GetComponent<BehaviorTree>().SetVariable("waypoint", vec2);
        }

    }
}