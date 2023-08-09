using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Test : MonoBehaviour
{
    public Player prefab;
    public List<Player> players = new();
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
        Joystick.onPlayerJoined += (id, nickname) =>
        {
            print($"Player with nickname: {nickname} and id {id} joined the game");
            var player = Instantiate(prefab, Vector3.zero, Quaternion.identity);
            player.id = id;
            player.userName = nickname;
            players.Add(player);
        };
        Joystick.onPlayerMoved += (id, action) =>
        {
            print($"Player with id {id} performed action: {action}");
        };
        Joystick.onPlayerRemoved += (id, nickname) =>
        {
            print($"Player {nickname} with id {id} was removed");
            var playerIndex = players.FindIndex(p => p.id == id);
            var player = players[playerIndex];
            players.RemoveAt(playerIndex);
            Destroy(player.gameObject);
        };
        Joystick.onWebsocketError += (string msg) => {
            print($"Websocket error: {msg}");
        };
        await Joystick.Begin(new JoystickConfig() { isSecure = false, port = "8081", gui = "SingleJoystick"});
    }

    // Update is called once per frame
    void Update()
    {
        Joystick.Update();
        if (Input.GetKeyDown(KeyCode.D))
        {
            SceneManager.LoadScene("second_scene");
        }
    }

    void Awake()
    {
        DontDestroyOnLoad(transform.gameObject);
    }

    private async void OnApplicationQuit()
    {
        await Joystick.GameOver();
    }
}
