using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
    public int id { get; set; }
    public string userName { get; set; }

    [SerializeField]
    float speed = .1f;

    void FixedUpdate()
    {
        var movement = Vector3.zero;

        if (Joystick.GetButton(id, GameControls.ArrowUp))
        {
            movement.y = 1;
        }

        if (Joystick.GetButton(id, GameControls.ArrowDown))
        {
            movement.y = -1;
        }

        if (Joystick.GetButton(id, GameControls.ArrowLeft))
        {
            movement.x = -1;
        }

        if (Joystick.GetButton(id, GameControls.ArrowRight))
        {
            movement.x = 1;
        }

        transform.Translate(movement * speed * Time.deltaTime);
    }

}
