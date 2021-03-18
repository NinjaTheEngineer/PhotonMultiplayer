using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;


public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    [SerializeField]
    Transform logListContent;

    [SerializeField]
    GameObject logListItemPrefab;

    int numberOfLogs = 0;

    private void Awake()
    {
        Instance = this;
    }

    public void UserConnected(string nickName)
    {
        foreach (Transform child in logListContent)
        {
            Destroy(child.gameObject);
        }
    }

    public void PlayerDied(string nickName, string killerName)
    {
        Debug.Log("Player - " + nickName + " > was killed by " + killerName);
        foreach (Transform child in logListContent)
        {
            if(numberOfLogs > 4)
            {
                Destroy(child.gameObject);
                numberOfLogs = 4;
            }
        }
        numberOfLogs += 1;

        if (PhotonNetwork.NickName.Equals(nickName))
        {
            Instantiate(logListItemPrefab, logListContent).GetComponent<LogListItem>().SetUp(nickName, killerName, 0);
        }
        else if (PhotonNetwork.NickName.Equals(killerName))
        {
            Instantiate(logListItemPrefab, logListContent).GetComponent<LogListItem>().SetUp(nickName, killerName, 1);
        }
        else
        {
            Instantiate(logListItemPrefab, logListContent).GetComponent<LogListItem>().SetUp(nickName, killerName, 2);
        }
    }

}
