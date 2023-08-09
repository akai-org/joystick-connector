using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor.PackageManager;
using UnityEngine;

public enum AnalogControls
{
    Left,
    Right
}

internal enum ControlState : byte
{
    KeyUp,
    KeyDown
}

internal enum AnalogBits : byte
{
    Angle0 = 1,
    Angle1 = 2,
    Angle2 = 4,
    Angle3 = 8,
    Angle4 = 16,
    Angle5 = 32,
    Drag0 = 64,
    Drag1 = 128,
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

internal class Controls
{
    readonly float _angleResolution = (float)(2 * Math.Round(Math.PI, 3) / ((1 << 6) - 1));
    readonly PlayersManager _playersManager;
    public Controls(PlayersManager playersManager)
    {
        _playersManager = playersManager;
    }
    public void HandleIncomingControls(byte[] bytes)
    {
        var playerId = bytes[0];
        var commandType = bytes[1];
        var action = bytes[2];


        PickProperControl(commandType, action, playerId);

    }

    void PickProperControl(byte commandType, byte action, byte playerId)
    {
        switch (commandType)
        {
            case 0:
                HandleButtonAction(playerId, action);
                break;
            case 1:
                HandleJoystickAction(playerId, AnalogControls.Left, action);
                break;
            case 2:
                HandleJoystickAction(playerId, AnalogControls.Right, action);
                break;
        }
    }

    private void HandleJoystickAction(byte playerId, AnalogControls analog, byte action)
    {
        _playersManager.SetAnalog(playerId, analog, action);
    }

    public bool GetButton(int playerId, GameControls gameControl)
    {
        return _playersManager.GetButton(playerId, gameControl);
    }

    private void HandleButtonAction(byte playerId, byte action)
    {
        _playersManager.SetButton(playerId, action);
    }

    public float GetAnalogHorizontal(int playerId, AnalogControls analog)
    {
        byte analogControls = _playersManager.GetAnalog(playerId, analog);

        var angleInRadians = CalculateAngleInRadians(analogControls);
        var parsedDrag = CalculateParsedDrag(analogControls);
        return (float)(parsedDrag * Math.Cos(angleInRadians));
    }

    public float GetAnalogVertical(int playerId, AnalogControls analog)
    {
        byte analogControls = _playersManager.GetAnalog(playerId, analog);

        var angleInRadians = CalculateAngleInRadians(analogControls);
        var parsedDrag = CalculateParsedDrag(analogControls);
        return (float)(parsedDrag * Math.Sin(angleInRadians));
    }

    int CalculateDrag(byte analogControls)
    {
        return analogControls & (byte)(AnalogBits.Drag0 | AnalogBits.Drag1);
    }

    float CalculateAngleInRadians(byte analogControls)
    {
        var drag = CalculateDrag(analogControls);
        var angle = analogControls - drag;
        return angle * _angleResolution; 
    }

    int CalculateParsedDrag(byte analogControls)
    {
        var drag = CalculateDrag(analogControls);
        return drag >> 6;
    }
}
