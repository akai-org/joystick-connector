using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
internal class IncomingMessageDto
{
    public string code;
}

[Serializable]
internal class GameDataDto
{
    public string gui;
    public int max_players;
}

[Serializable]
internal class RoomCodeDto
{
    public string code;
}

[Serializable]
internal class EventDto
{
    public string event_name;
    public string nickname;
    public int id;
}
