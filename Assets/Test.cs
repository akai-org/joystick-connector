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
        await Joystick.Begin();
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
