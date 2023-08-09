using System.Collections;
using System.Collections.Generic;
using UnityEngine;

internal class PlayerData
{
    public string nickname { get; private set; }
    public int id { get; private set; }
    public PlayerData(int id, string nickname)
    {
        this.nickname = nickname;
        this.id = id;
    }

    readonly Dictionary<GameControls, bool> _controls = new Dictionary<GameControls, bool>()
    {
        { GameControls.ArrowUp, false },
        { GameControls.ArrowDown, false },
        { GameControls.ArrowLeft, false },
        { GameControls.ArrowRight, false },
        { GameControls.ActionButton1, false },
        { GameControls.ActionButton2, false },
        { GameControls.ActionButton3, false },
        { GameControls.ActionButton4, false },
    };

    readonly Dictionary<AnalogControls, byte> _analogControls = new() {
        {AnalogControls.Left, 0},
        {AnalogControls.Right, 0}
    };

    public Dictionary<GameControls, bool> controls { get { return _controls; } }

    public bool GetButton(GameControls button)
    {
        return _controls[button];
    }

    public void SetButton(GameControls button, bool value)
    {
        _controls[button] = value;
    }

    public void SetAnalog(AnalogControls analog, byte action)
    {
        _analogControls[analog] = action;
    }

    public byte GetAnalog(AnalogControls analog)
    {
        return _analogControls[analog];
    }

}
