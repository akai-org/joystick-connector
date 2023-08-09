using Codice.CM.Common;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

internal class PlayersManager
{
    readonly Dictionary<int, PlayerData> _players = new();

    public void AddPlayer(int playerId, string PlayerNick)
    {
        _players.Add(playerId, new PlayerData(playerId, PlayerNick));
    }

    public void RemovePlayer(int playerId)
    {
        _players.Remove(playerId);
    }

    PlayerData GetPlayer(int playerId)
    {
        if (!_players.TryGetValue(playerId, out PlayerData player))
        {
            throw new Exception("Could not find a user with such an id");
        }
        return player;
    }

    /*
     This one requires some explanation. 
    Every button's pressed & unpressed state is unique, 
    so we only need to check max two combinations to get proper button
     */
    public void SetButton(int playerId, byte action)
    {
        var player = GetPlayer(playerId);
        if (!player.controls.TryGetValue((GameControls)action, out _))
        {
            var modifiedAction = action - 1;
            if (!player.controls.TryGetValue((GameControls)modifiedAction, out _))
            {
                throw new Exception($"Could not find a button with the value of: {action}");
            }
            player.SetButton((GameControls)modifiedAction, true);
            return;
        }
        player.SetButton((GameControls)action, false);
    }

    public void SetAnalog(int playerId, AnalogControls analog, byte action)
    {
        var player = GetPlayer(playerId);
        player.SetAnalog(analog, action);
    }

    public byte GetAnalog(int playerId, AnalogControls analog)
    {
        var player = GetPlayer(playerId);
        return player.GetAnalog(analog);
    }

    public bool GetButton(int playerId, GameControls gameControl)
    {
        var player = GetPlayer(playerId);

        if (!player.controls.TryGetValue(gameControl, out bool buttonState))
        {
            throw new Exception("Could not find a button with such an id");
        }
        return buttonState;
    }
}
