using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerManager : SingletonMonoBehaviour<PlayerManager>
{
    [SerializeField]
    private List<Player> playerList = null;

    [SerializeField]
    private InputManager inputManager = null;

    private void Update()
    {
        if (playerList[0].GetNowStateType() != Player.StateType.MOVE)
        {
            playerList[0].SetShotParameter(inputManager.TouchPullDirection(), 250.0f);
        }
        if (Input.GetKeyUp(KeyCode.Mouse0) && playerList[0].GetNowStateType() != Player.StateType.MOVE)
        {
            Debug.Log("Off");
            playerList[0].ChangeState(Player.StateType.MOVE);
        }
    }
}
