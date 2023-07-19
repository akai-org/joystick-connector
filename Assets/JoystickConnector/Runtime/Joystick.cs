using NativeWebSocket;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.WebSockets;
using System.Runtime.CompilerServices;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.XR;
using static UnityEditor.Experimental.GraphView.GraphView;

[Serializable]
class IncomingMessageDto
{
    public string code;
}

[Serializable]
class GameDataDto
{
    public string gui;
    public int max_players;
}

[Serializable]
class RoomCodeDto
{
    public string code;
}

[Serializable]
class EventDto
{
    public string event_name;
    public string nickname;
    public int id;
}

public class JoystickConfig
{
    public string domain = "localhost";
    public string port = "";
    public bool isSecure = false;
    public int maxPlayers = 4;
    public string gui = "CrossArrows";
}

public enum GameControls : byte
{
    ArrowUp = 0 << 1,
    ArrowDown = 1 << 1,
    ArrowRight = 2 << 1,
    ArrowLeft = 3 << 1,
    ActionButton1 = 4 << 1,
    ActionButton2 = 5 << 1,
    ActionButton3 = 6 << 1,
    ActionButton4 = 7 << 1
}

public enum ControlState : byte
{
    KeyUp,
    KeyDown
}

public static class GameEvent
{
    public static readonly string PlayerJoined = "player_added";
}

public class PlayerData
{
    public string nickname { get; private set; }
    public int id { get; private set; }
    public PlayerData(int id, string nickname) { 
        this.nickname = nickname;
        this.id = id;
    }

    readonly Dictionary<GameControls, bool> _controls = new Dictionary<GameControls, bool>() 
    { 
        { GameControls.ArrowUp, false },
        { GameControls.ArrowDown, false },
        { GameControls.ArrowLeft, false },
        { GameControls.ArrowRight, false } 
    };

    public Dictionary<GameControls, bool> controls { get { return _controls; } }

    public bool GetButton(GameControls button) { 
        return _controls[button];
    }
    public void SetButton(GameControls button, bool value)
    {
        _controls[button] = value;
    }
}

public static class Joystick
{
    public delegate void OnCodeAcquired(string code);
    public delegate void OnError(Exception e);
    public delegate void OnWebsocketOpen();
    public delegate void OnWebsocketMessage(byte[] bytes);
    public delegate void OnPlayerJoined(int id, string nickname);
    public delegate void OnPlayerMoved(int id, byte action);
    public delegate void OnWebsocketError(string error);

    public static OnCodeAcquired onCodeAcquired;
    public static OnError onError;
    public static OnWebsocketOpen onWebsocketOpen;
    public static OnWebsocketMessage onWebsocketMessage;
    public static OnPlayerJoined onPlayerJoined;
    public static OnPlayerMoved onPlayerMoved;
    public static OnWebsocketError onWebsocketError;

    static NativeWebSocket.WebSocket _websocket;
    static readonly HttpClient client = new();

    static readonly Dictionary<int, PlayerData> _players = new();

    public static async Task Begin(JoystickConfig config)
    {
        var gameData = new GameDataDto() { gui = config.gui, max_players = config.maxPlayers };
        var serializedData = JsonUtility.ToJson(gameData);
        var data = new StringContent(serializedData);
        try
        {
            string httpType = config.isSecure ? "https" : "http";
            var res = await client.PostAsync($"{httpType}://{config.domain}:{config.port}/create", data);
            var resData = await res.Content.ReadAsStringAsync();
            var roomCode = JsonUtility.FromJson<IncomingMessageDto>(resData).code;
            onCodeAcquired(roomCode);

            string socketType = config.isSecure ? "wss" : "ws";
            _websocket = new NativeWebSocket.WebSocket($"{socketType}://{config.domain}:{config.port}/room/socket");

            _websocket.OnOpen += async () => await HandleWebsocketOpen(roomCode);

            _websocket.OnMessage += HandleWebsocketMessage;

            _websocket.OnError += (error) => onWebsocketError(error);

            await _websocket.Connect();
        }
        catch (Exception e)
        {
            onError(e);
        }
    }

    private async static Task HandleWebsocketOpen(string roomCode)
    {
        onWebsocketOpen();

        var roomCodeData = new RoomCodeDto() { code = roomCode };
        var returnCode = JsonUtility.ToJson(roomCodeData);
        await _websocket.SendText(returnCode);
    }

    private static void HandleWebsocketMessage(byte[] bytes)
    {
        if(bytes.Length == 2)
        {
            HandleIncomingControls(bytes);

        } else
        {
            HandleGameEvent(bytes);
        }
    }

    private static void HandleIncomingControls(byte[] bytes)
    {
        var userId = bytes[0];
        var action = bytes[1];

        PlayerData player;
        if (!_players.TryGetValue(userId, out player))
        {
            onError(new Exception("Could not find a user with such an id"));
            return;
        }

        if (!player.controls.TryGetValue((GameControls)action, out _))
        {
            var modifiedAction = action - 1;
            if (!player.controls.TryGetValue((GameControls)modifiedAction, out _))
            {
                onError(new Exception($"Could not find a button with the value of: {action}"));
                return;
            }
            player.SetButton((GameControls)modifiedAction, false);
            onPlayerMoved((int)userId, action);
            return;
        }
        player.SetButton((GameControls)action, true);
        onPlayerMoved((int)userId, action);
    }

    private static void HandleGameEvent(byte[] bytes)
    {
        var wsEvent = JsonUtility.FromJson<EventDto>(Encoding.UTF8.GetString(bytes));
        if (wsEvent.event_name == GameEvent.PlayerJoined)
        {
            int playerId = wsEvent.id;
            string nickname = wsEvent.nickname;
            _players.Add(playerId, new PlayerData(playerId, nickname));
            onPlayerJoined(playerId, nickname);
        }
    }

    public static void Update()
    {
        #if !UNITY_WEBGL || UNITY_EDITOR
            if (_websocket != null)
            {
                _websocket.DispatchMessageQueue();
            }
        #endif
    }

    public async static Task GameOver()
    {
        if (_websocket != null)
        {
            await _websocket.Close();
        }
    }
}
