using UnityEngine;
using UnityEngine.Networking;

public class NetworkAction : NetworkBehaviour
{
    [SyncVar]
    private bool finished = false;
    [SyncVar]
    private int now_player = 1;
    private SyncListInt status = new SyncListInt();
    private GUIStyle title, back, info, sign;
    public Texture2D img;

    private void Start()
    {
        ResetStatus();
    }

    private void OnGUI()
    {
        title = new GUIStyle();
        title.fontSize = 30;
        title.normal.textColor = Color.black;
        back = new GUIStyle();
        back.normal.background = img;
        info = new GUIStyle();
        info.fontSize = 20;
        title.normal.textColor = Color.black;
        sign = new GUIStyle();
        sign.fontSize = 30;
        sign.alignment = TextAnchor.MiddleCenter;

        GUI.Label(new Rect(0, 0, Screen.width, Screen.height), "", back);
        GUI.Label(new Rect(330, 30, 100, 100), "井字棋", title);

        if (GUI.Button(new Rect(335, 280, 80, 40), "重置")) ResetStatus();
        if (!finished)
        {
            if (now_player == 1) GUI.Label(new Rect(480, 60, 50, 50), "现在轮到: Player1(Server)", info);
            else GUI.Label(new Rect(480, 60, 50, 50), "现在轮到: Player2(Client)", info);
        }

        //输出胜负信息
        switch (CheckStatus())
        {
            case 1:
                GUI.Label(new Rect(480, 100, 50, 50), "Player1(Server) 胜利！", info);
                GameFinish();
                break;
            case 2:
                GUI.Label(new Rect(480, 100, 50, 50), "Player2(Client) 胜利！", info);
                GameFinish();
                break;
            case 3:
                GUI.Label(new Rect(480, 100, 50, 50), "平局！", info);
                GameFinish();
                break;
        }

        //输出棋盘按钮
        int[,] status = GetStatus();
        for (int i = 0; i < 3; i++)
        {
            for (int j = 0; j < 3; j++)
            {
                if (status[i, j] == 1) GUI.Button(new Rect(i * 60 + 285, j * 60 + 80, 60, 60), "O", sign);
                else if (status[i, j] == 2) GUI.Button(new Rect(i * 60 + 285, j * 60 + 80, 60, 60), "X", sign);
                else if (GUI.Button(new Rect(i * 60 + 285, j * 60 + 80, 60, 60), ""))
                {
                    if (CanAction()) continue;
                    else if (now_player == 1) SetStatus(i, j, 1);
                    else SetStatus(i, j, 2);
                }
            }
        }
    }

    public int CheckStatus()
    {
        for (int i = 0; i < 3; i++)
        {
            if (status[i * 3 + 0] != 0 && status[i * 3 + 0] == status[i * 3 + 1] && status[i * 3 + 1] == status[i * 3 + 2]) return status[i * 3 + 1];
            if (status[0 * 3 + i] != 0 && status[0 * 3 + i] == status[1 * 3 + i] && status[1 * 3 + i] == status[2 * 3 + i]) return status[1 * 3 + i];
        }

        if (status[1 * 3 + 1] != 0)
        {
            if (status[0 * 3 + 2] == status[1 * 3 + 1] && status[1 * 3 + 1] == status[2 * 3 + 0]) return status[1 * 3 + 1];
            if (status[0 * 3 + 0] == status[1 * 3 + 1] && status[1 * 3 + 1] == status[2 * 3 + 2]) return status[1 * 3 + 1];
        }


        for (int i = 0; i < 3; i++)
        {
            for (int j = 0; j < 3; j++)
            {
                if (status[i * 3 + j] == 0) return 0;
            }
        }

        return 3;
    }

    public int[,] GetStatus()
    {
        int[,] temp = new int[3, 3];
        for (int i = 0; i < 3; i++)
        {
            for (int j = 0; j < 3; j++)
            {
                temp[i, j] = status[i * 3 + j];
            }
        }
        return temp;
    }

    [Command]
    public void CmdSetStatus(int posX, int posY, int value)
    {
        SetStatus(posX, posY, value);
    }

    public void SetStatus(int posX, int posY, int value)
    {
        if (isClient) CmdSetStatus(posX, posY, value);
        else
        {
            status[posX * 3 + posY] = value;
            now_player = -now_player;
        }
    }

    [Command]
    public void CmdReset()
    {
        ResetStatus();
    }

    public void ResetStatus()
    {
        finished = false;
        now_player = 1;
        status.Clear();
        for (int i = 0; i < 3; i++)
        {
            for (int j = 0; j < 3; j++)
            {
                status.Add(0);
            }
        }
        if (isClient) CmdReset();
    }

    public void GameFinish() { finished = true; }
    public bool CanAction()
    {
        if (finished) return finished;
        return (now_player == 1 && isClient) || (now_player != 1 && isServer);
    }
}
