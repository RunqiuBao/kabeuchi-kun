using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ControllerController : MonoBehaviour
{
    GameController gameController;

    private void Start()
    {
        // GameControllerオブジェクトを取得する。
        gameController =　FindObjectOfType<GameController>();

        if (gameController == null)
            Debug.Log("ControllerController: Can't find GameController");

    }

    void Update()
    {
        // コントローラーを取得
        SteamVR_TrackedObject trackedObject = GetComponent<SteamVR_TrackedObject>();
        var device = SteamVR_Controller.Input((int)trackedObject.index);

        // Triggerの値を取得(0..1)
        var value = device.GetAxis(Valve.VR.EVRButtonId.k_EButton_SteamVR_Trigger ).x;

        //        Debug.Log("Trigger index =" + trackedObject.index +" value = " + value );

        // Triggerの値からSpeedを変更。
        // Trigger =0(離されている) -> Speed = 2(2倍速）
        // Trigger =1(握られている) -> Speed ≒ 0.03(30分の一倍速）
        float speed = (float)( - value * 2 + 2.03);

        // Scoreボードにスピードを表示（デバッグ用）
        var MessageTextObj = GameObject.Find("ScoreBoardText1");
        Text txt = MessageTextObj.GetComponent<Text>();
        txt.text = "Speed = "+  speed;

        // GameControllerにSpeedを設定
        gameController.SetPlayBackSpeed( speed );
    }
}
