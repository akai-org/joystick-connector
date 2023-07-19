using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Test : MonoBehaviour
{
    // Start is called before the first frame update
    async void Start()
    {
        Joystick.onCodeAcquired += (string code) =>
        {
            print(code);
        };
        Joystick.onError += (Exception e) =>
        {
            print(e);
        };
        Joystick.onPlayerJoined += (id, nickname) =>
        {
            print($"Player with nickname: {nickname} and id {id} joined the game");
        };
        Joystick.onPlayerMoved += (id, action) =>
        {
            print($"Player with id {id} performed action: {action}");
        };
        await Joystick.Begin(new JoystickConfig() { port = "8081", isSecure = false});
    }

    // Update is called once per frame
    void Update()
    {
        Joystick.Update();
    }
}
