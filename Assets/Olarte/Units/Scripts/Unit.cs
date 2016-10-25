using UnityEngine;
using System;
using System.Collections.Generic;
using BehaviorDesigner.Runtime;

public class Unit : MonoBehaviour
{

    public int nUnits = 9;
    public GameObject unitPrefab;
    public string tag1 = "None";
    public string tag2 = "None";
    public int button = 3;
    public float range = 1f;

    public List<GameObject> _unit = new List<GameObject>();
    public GameObject _targetUnit;
    public GameObject _targetLeader;

    private int units;

    void Start()
    {
        units = nUnits;
        var agent = (GameObject)Instantiate(unitPrefab, this.transform.position, this.transform.rotation);
        SharedBool isLeader = new SharedBool();
        SharedGameObject leader = new SharedGameObject();
        SharedTransform unit = new SharedTransform();
        SharedInt columns = new SharedInt();

        columns.Value = GetColumns();
        isLeader.Value = true;
        leader.Value = agent;
        unit.Value = this.gameObject.transform;

        agent.GetComponent<BehaviorTree>().SetVariable("_isLeader",isLeader);
        agent.GetComponent<BehaviorTree>().SetVariable("_leader", leader);
        agent.GetComponent<BehaviorTree>().SetVariable("_unit", unit);
        agent.GetComponent<BehaviorTree>().SetVariable("_columns", columns);
        agent.GetComponent<Life>()._unit = gameObject;

        _unit.Add(agent);

        isLeader.Value = false;
        for (int i = 1; i < units; ++i)
        {
            agent = (GameObject)Instantiate(unitPrefab, this.transform.position, this.transform.rotation);
            agent.GetComponent<BehaviorTree>().SetVariable("_isLeader", isLeader);
            agent.GetComponent<BehaviorTree>().SetVariable("_leader", leader);
            agent.GetComponent<BehaviorTree>().SetVariable("_unit", unit);
            agent.GetComponent<BehaviorTree>().SetVariable("_columns", columns);
            agent.GetComponent<Life>()._unit = gameObject;
            _unit.Add(agent);
        }

        this.transform.position = this.transform.position + new Vector3(5,0,0);
        refresh();

    }


    void Update()
    {
        if (button!=3)
            if (Input.GetMouseButton(button))
            {

                Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
                RaycastHit hit;

                if (Physics.Raycast(ray, out hit, 100))
                    if (hit.collider.CompareTag(tag1) || hit.collider.CompareTag(tag2))
                    {

                        _targetUnit = hit.transform.gameObject.GetComponent<Life>()._unit;
                        _targetLeader = _targetUnit.GetComponent<Unit>()._unit[0];
                    }
                    else
                    {
                        _targetUnit = null;
                        this.transform.position = hit.point;
                    }
                refresh();
            }
        if(_targetUnit !=null)
        {
            if (_targetLeader != null)
            {
                var temp = Vector3.Distance(_unit[0].transform.position, _targetLeader.transform.position);

                if (temp > range)
                {


                    transform.position = _targetLeader.transform.position;
                }
                else
                {

                    transform.position = _unit[0].transform.position;
                }
            }
            else
                _targetLeader = _targetUnit.GetComponent<Unit>()._unit[0];
        }
        
    }

    public void AddAgent()
    {
        units++;

        if (units > nUnits)
        {
            units--;
            return;
        }

       
        var agent = (GameObject)Instantiate(unitPrefab, this.transform.position, this.transform.rotation);
        SharedBool isLeader = new SharedBool();
        SharedGameObject leader = new SharedGameObject();
        SharedTransform unit = new SharedTransform();
        SharedInt columns = new SharedInt();

        columns.Value = GetColumns();
        isLeader.Value = false;
        leader.Value = _unit[0];
        unit.Value = this.gameObject.transform;

        agent.GetComponent<BehaviorTree>().SetVariable("_isLeader", isLeader);
        agent.GetComponent<BehaviorTree>().SetVariable("_leader", leader);
        agent.GetComponent<BehaviorTree>().SetVariable("_unit", unit);
        agent.GetComponent<BehaviorTree>().SetVariable("_columns", columns);
        agent.GetComponent<Life>()._unit = gameObject;

        _unit.Add(agent);

        columns.Value = GetColumns();
        for (int i = 0; i < units; ++i)
        {
            agent.GetComponent<BehaviorTree>().SetVariable("_columns", columns);
        }

        refresh();
    }

    public void AgentDied(GameObject agent)
    {
        units--;

        if (units == 0)
        {
            Destroy(gameObject);
        }

        SharedVariable isLeader = agent.GetComponent<BehaviorTree>().GetVariable("_isLeader");
        if (Convert.ToBoolean(isLeader.GetValue()))
        {
            SharedBool tmpbool = new SharedBool();
            SharedGameObject tmpobj = new SharedGameObject();
            tmpbool.Value = true;

            _unit.Remove(agent);

            tmpobj.Value = _unit[0];
            _unit[0].GetComponent<BehaviorTree>().SetVariable("_isLeader", tmpbool);
            for (int i = 0; i < units; ++i)
            {
                _unit[i].GetComponent<BehaviorTree>().SetVariable("_leader", tmpobj);
            }

        }
        else
            _unit.Remove(agent);

        SharedInt columns = new SharedInt();
        columns.Value = GetColumns();
        for (int i = 0; i < units; ++i)
        {
            agent.GetComponent<BehaviorTree>().SetVariable("_columns", columns);
        }

        refresh();
    }

    private void refresh()
    {
        for (int i = 0; i < units; ++i)
            _unit[i].GetComponent<BehaviorTree>().DisableBehavior();
        for (int i = 0; i < units; ++i)
            _unit[i].GetComponent<BehaviorTree>().EnableBehavior();
    }

    private int GetColumns()
    {
        double divisior = Math.Sqrt(units);
        return Convert.ToInt16(Math.Ceiling((units / divisior)));
    }
}
