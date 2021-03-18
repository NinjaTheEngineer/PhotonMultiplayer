using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class LogListItem : MonoBehaviour
{
    [SerializeField]
    TMP_Text text;

    const int YOU_GOT_KILLED = 0;
    const int YOURE_THE_KILLER = 1;
    const int SOMEONE_DIED = 2;

    public void SetUp(string nickName, string killerName, int scenario)
    {
        switch (scenario)
        {
            case YOU_GOT_KILLED:
                text.text = killerName + " killed you!";
                break;
            case YOURE_THE_KILLER:
                text.text = "You killed " + nickName + "!";
                break;
            case SOMEONE_DIED:
                text.text = nickName + " was killed by " + killerName + "!";
                break;
        }
    }

}
