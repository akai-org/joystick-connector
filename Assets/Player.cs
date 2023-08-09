using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
    public int id { get; set; }
    public string userName { get; set; }

    [SerializeField]
    float speed = 0.25f;

    void FixedUpdate()
    {
        var movement = Vector3.zero;

        //if (Joystick.GetButton(id, GameControls.ArrowUp))
        //{
        //    movement.y = 1;
        //}

        //if (Joystick.GetButton(id, GameControls.ArrowDown))
        //{
        //    movement.y = -1;
        //}

        //if (Joystick.GetButton(id, GameControls.ArrowLeft))
        //{
        //    movement.x = -1;
        //}

        //if (Joystick.GetButton(id, GameControls.ArrowRight))
        //{
        //    movement.x = 1;
        //}

        movement.x = Joystick.GetAnalogHorizontal(id, AnalogControls.Left) * speed;
        movement.y = Joystick.GetAnalogVertical(id, AnalogControls.Left) * speed;

        //print(movement.x);
        //print(movement.y);

        transform.Translate(movement * speed * Time.deltaTime);
    }

}
