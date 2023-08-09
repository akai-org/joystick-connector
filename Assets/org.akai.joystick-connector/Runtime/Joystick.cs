using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public class JoystickConfig
{
    public string domain = "localhost";
    public string port = "";
    public bool isSecure = false;
    public int maxPlayers = 4;
    public string gui = "CrossArrows";
}

public static class Joystick
{
    static CommsBase comms = new Comms();
    public static OnCodeAcquired onCodeAcquired = delegate { };
    public static OnError onError = delegate { };
    public static OnWebsocketOpen onWebsocketOpen = delegate { };
    public static OnWebsocketMessage onWebsocketMessage = delegate { };
    public static OnPlayerJoined onPlayerJoined = delegate { };
    public static OnPlayerMoved onPlayerMoved = delegate { };
    public static OnWebsocketError onWebsocketError = delegate { };
    public static OnPlayerRemoved onPlayerRemoved = delegate { };

    // to change comms for testing purpose - i hope that it'll work
    internal static void SetCustomComms(CommsBase customComms)
    {
        comms = customComms;
    }

    static void SetupExternalComs()
    {
        comms.onError += (Exception e) => { 
            onError(e);
        };

        comms.onCodeAcquired += (string code) => {
            onCodeAcquired(code);
        };

        comms.onWebsocketOpen += () => {
            onWebsocketOpen();
        };

        comms.onWebsocketMessage += (byte[] message) => { 
            onWebsocketMessage(message);
        };

        comms.onPlayerJoined += (int playerId, string playerNick) =>
        {
            onPlayerJoined(playerId, playerNick);
        };

        comms.onPlayerMoved += (int id, byte action) => { 
            onPlayerMoved(id, action);
        };

        comms.onWebsocketError += (string error) => { 
            onWebsocketError(error);
        };

        comms.onPlayerRemoved += (int playerId, string playerNick) => { 
            onPlayerRemoved(playerId, playerNick);
        };
    }

    public static async Task Begin(JoystickConfig config)
    {
        SetupExternalComs();
        var gameData = new GameDataDto() { gui = config.gui, max_players = config.maxPlayers };
        var serializedData = JsonUtility.ToJson(gameData);
        var data = new StringContent(serializedData);

        string httpType = config.isSecure ? "https" : "http";
        var httpUrl = $"{httpType}://{config.domain}:{config.port}/create";
        string socketType = config.isSecure ? "wss" : "ws";
        var wsUrl = $"{socketType}://{config.domain}:{config.port}/room/socket";
        try
        {
            await comms.Connect(httpUrl, data, wsUrl);
        }
        catch (Exception e)
        {
            onError(e);
        }
    }

    public static bool GetButton(int playerId, GameControls gameControl)
    {
        return comms.Controls.GetButton(playerId, gameControl);
    }

    public static float GetAnalogHorizontal(int playerId, AnalogControls analog)
    {
        return comms.Controls.GetAnalogHorizontal(playerId, analog);
    }
    public static float GetAnalogVertical(int playerId, AnalogControls analog)
    {
        return comms.Controls.GetAnalogVertical(playerId, analog);
    }

    public static void Update()
    {
        comms.Update();
    }

    public async static Task GameOver()
    {
        await comms.Disconnect();
    }
}
