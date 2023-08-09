using System;
using System.Collections;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using UnityEditor.Networking.PlayerConnection;
using UnityEngine;

public delegate void OnCodeAcquired(string code);
public delegate void OnError(Exception e);
public delegate void OnWebsocketOpen();
public delegate void OnWebsocketMessage(byte[] bytes);
public delegate void OnPlayerJoined(int id, string nickname);
public delegate void OnPlayerRemoved(int id, string nickname);
public delegate void OnPlayerMoved(int id, byte action);
public delegate void OnWebsocketError(string error);

internal static class GameEvent
{
    public static readonly string PlayerJoined = "player_added";
    public static readonly string PlayerRemoved = "player_removed";
}

internal class Comms
{

    public OnCodeAcquired onCodeAcquired = delegate { };
    public OnError onError = delegate { };
    public OnWebsocketOpen onWebsocketOpen = delegate { };
    public OnWebsocketMessage onWebsocketMessage = delegate { };
    public OnPlayerJoined onPlayerJoined = delegate { };
    public OnPlayerMoved onPlayerMoved = delegate { };
    public OnWebsocketError onWebsocketError = delegate { };
    public OnPlayerRemoved onPlayerRemoved = delegate { };
    NativeWebSocket.WebSocket _websocket;
    public NativeWebSocket.WebSocket Websocket => _websocket;
    readonly PlayersManager _playersManager = new();
    readonly Controls _controls;
    public Controls Controls => _controls;

    public Comms() {
        _controls = new(_playersManager);
    }

    readonly HttpClient client = new();

    public async Task Connect(string httpUrl, StringContent data, string wsUrl)
    {
        var res = await client.PostAsync(httpUrl, data);
        var resData = await res.Content.ReadAsStringAsync();
        var roomCode = JsonUtility.FromJson<IncomingMessageDto>(resData).code;
        onCodeAcquired(roomCode);

        _websocket = new NativeWebSocket.WebSocket(wsUrl);

        _websocket.OnOpen += async () => await HandleWebsocketOpen(roomCode);

        _websocket.OnMessage += HandleWebsocketMessage;

        _websocket.OnError += (error) => onWebsocketError(error);

        await _websocket.Connect();
    }

    private async Task HandleWebsocketOpen(string roomCode)
    {
        onWebsocketOpen();

        var roomCodeData = new RoomCodeDto() { code = roomCode };
        var returnCode = JsonUtility.ToJson(roomCodeData);
        await _websocket.SendText(returnCode);
    }

    private void HandleWebsocketMessage(byte[] bytes)
    {
        if (bytes.Length == 3)
        {
            try
            {
                _controls.HandleIncomingControls(bytes);
                // bytes[1] = playerId, bytes[2] = action
                onPlayerMoved(bytes[0], bytes[2]);
            }
            catch (Exception e)
            {
                onError(e);
            }
        }
        else
        {
            HandleGameEvent(bytes);
        }
    }

    private void HandleGameEvent(byte[] bytes)
    {
        var wsEvent = JsonUtility.FromJson<EventDto>(Encoding.UTF8.GetString(bytes));
        if (wsEvent.event_name == GameEvent.PlayerJoined)
        {
            int playerId = wsEvent.id;
            string nickname = wsEvent.nickname;
            _playersManager.AddPlayer(playerId, nickname);
            onPlayerJoined(playerId, nickname);
        }
        else if (wsEvent.event_name == GameEvent.PlayerRemoved)
        {
            int playerId = wsEvent.id;
            string nickname = wsEvent.nickname;
            _playersManager.RemovePlayer(playerId);
            onPlayerRemoved(playerId, nickname);
        }
    }

    public void Update()
    {
#if !UNITY_WEBGL || UNITY_EDITOR
        _websocket?.DispatchMessageQueue();
#endif
    }

    public async Task Disconnect()
    {
        if (_websocket != null)
        {
            await _websocket.Close();
            _websocket = null;
        }
    }
}
