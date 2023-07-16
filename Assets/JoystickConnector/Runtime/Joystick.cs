using NativeWebSocket;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using UnityEngine;

[Serializable]
public class IncomingMessageDto
{
    public string code;
}

[Serializable]
public class GameDataDto
{
    public string gui;
    public int max_players;
}

[Serializable]
public class RoomCodeDto
{
    public string code;
}

[Serializable]
public class EventDto
{
    public string event_name;
    public string nickname;
    public int id;
}

public static class Joystick
{
    public delegate void OnCodeAcquired(string code);
    public delegate void OnError(Exception e);

    public static OnCodeAcquired onCodeAcquired;
    public static OnError onError;

    static WebSocket _websocket;
    static readonly HttpClient client = new();

    public static async Task Begin()
    {
        var gameData = new GameDataDto() { gui = "CrossArrows", max_players = 6 };
        var serializedData = JsonUtility.ToJson(gameData);
        var data = new StringContent(serializedData);
        try
        {
            var res = await client.PostAsync("http://localhost:8081/create", data);
            var resData = await res.Content.ReadAsStringAsync();
            var roomCode = JsonUtility.FromJson<IncomingMessageDto>(resData).code;

            onCodeAcquired(roomCode);
        }
        catch (Exception e)
        {
            onError(e);
        }
    }
}
