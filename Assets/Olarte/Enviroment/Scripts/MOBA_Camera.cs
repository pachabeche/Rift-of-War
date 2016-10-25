using UnityEngine;

public class MOBA_Camera : MonoBehaviour
{
    public float xMax = 470;
    public float xMin = 10;
    public float yMax = 50;
    public float yMin = 45;
    public float zMax = 470;
    public float zMin = -20;
    public float moveZone = 50;
    public float moveSpeed = 80;
    public float scrollSpeed = 40;
    public GameObject target;

    private float x = 0, y = 0, z = 0;
    private float speed;

    void Update()
    {
        if (Input.GetKey(KeyCode.Space))
        {
            y = 0;
            y -= Input.GetAxis("Mouse ScrollWheel") * scrollSpeed;
            Vector3 move = target.GetComponent<Unit>()._unit[0].transform.position;

            move.y = transform.position.y +y;
            move.y = Mathf.Clamp(move.y, yMin, yMax);
            move.z -= move.y;
            transform.position = move;
        }
        else
        {
            speed = moveSpeed * Time.deltaTime;
            x = z = y = 0;

            if (Input.mousePosition.x < moveZone)
                x -= speed;
            else if (Input.mousePosition.x > Screen.width - moveZone)
                x += speed;
            if (Input.mousePosition.y < moveZone)
                z -= speed;
            else if (Input.mousePosition.y > Screen.height - moveZone)
                z += speed;
            y -= Input.GetAxis("Mouse ScrollWheel") * scrollSpeed;

            Vector3 move = new Vector3(x, y, z) + transform.position;
            move.y = Mathf.Clamp(move.y, yMin, yMax);
            move.x = Mathf.Clamp(move.x, xMin, xMax);

            move.z = Mathf.Clamp(move.z, zMin, zMax);
            transform.position = move;
        }
    }
}
