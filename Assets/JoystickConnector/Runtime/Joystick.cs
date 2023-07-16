using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IncomingMessageDto
{
    public string code { get; set; }
}

[Serializable]
public class GameDataDto
{
    public string gui { get; set; }
    public int max_players { get; set; }
}

public class RoomCodeDto
{
    public string code { get; set; }
}

public class EventDto
{
    public string event_name { get; set; }
    public string nickname { get; set; }
    public int id { get; set; }
}

public static class Joystick
{
    public delegate void OnCodeAcquired(string code);
    public static OnCodeAcquired onCodeAcquired;

}
