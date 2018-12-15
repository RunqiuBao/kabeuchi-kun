using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    GameController gameController;

    // Use this for initialization
    void Start()
    {
        // GameControllerオブジェクトを取得する。
        gameController = FindObjectOfType<GameController>();

        // 手元でのデバッグ用．Viveとの同期がなくてもラケットを強制的に表示
        // GameObject controller = gameObject.transform.Find("Controller (left)");
        // print(controller);
        

    }

    // Update is called once per frame
    void Update()
    {
        // プレイヤーの移動と回転。VRではCameraRigは
        //　動かせないので、それを含むGameObjectを作成し、
        // それを移動させる。

        Transform transform = GetComponent<Transform>();

        // 上下左右キーが押されたら、その方向へ移動
        if (Input.GetKey(KeyCode.LeftArrow))
        {
            MovePlayer(new Vector3(0.03f, 0, 0));
        }
        else if (Input.GetKey(KeyCode.RightArrow))
        {
            MovePlayer(new Vector3(-0.03f, 0, 0));
        }
        else if (Input.GetKey(KeyCode.UpArrow))
        {
            MovePlayer(new Vector3(0, 0, -0.03f));
        }
        else if (Input.GetKey(KeyCode.DownArrow))
        {
            MovePlayer(new Vector3(0, 0, 0.03f));
        }
        // A,Sキーが押されたら、Y軸を中心に回転
        else if (Input.GetKey(KeyCode.A))
        {
            transform.Rotate(Vector3.up);
        }
        else if (Input.GetKey(KeyCode.S))
        {
            transform.Rotate(-Vector3.up);
        }
    }

    // プレイヤーをoffset分移動する
    void MovePlayer(Vector3 offset)
    {
        // Transformコンポーネントを取得
        Transform transform = GetComponent<Transform>();

        Vector3 position = transform.position;
        position += offset;
        transform.position = position;
        return;
    }

    /************************************************
     *                                              *
     *      ここから外部向けMethod                  *
     *                                              *
     ************************************************/

    // プレイヤーの位置を取得する。ほかのObject(基本はGameController)から呼ばれる。
    public void GetPosition(ref Vector3 pos, ref Quaternion dir)
    {
        // Transformコンポーネントを取得
        Transform transform = GetComponent<Transform>();

        pos = transform.position;
        dir = transform.rotation;

        return;
    }

    // プレイヤーの位置を変更する。ほかのObject(基本はGameController)から呼ばれる。
    public void SetPosition(Vector3 pos, Quaternion dir)
    {
        // Transformコンポーネントを取得
        Transform transform = GetComponent<Transform>();

        transform.position = pos;
        transform.rotation = dir;

        return;
    }
}
 
 
 