using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    // Start is called before the first frame update
    public Transform player;
    private Vector3 offset;
    void Start()
    {
        offset = new Vector3(0, 0, -10);
    }

    // Update is called once per frame
    void LateUpdate()
    {
        transform.position = player.position + offset;
    }
}
