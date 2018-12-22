using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ControllerController : MonoBehaviour
{
    GameController gameController;
    ushort biv_duration = 2000;
    int biv_count = 0;
    float value_prev = 0;

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


        if (value < 0.5 && value_prev >=0.5) {
            gameController.enterZone();
        }
        else if(value >= 0.5 && value_prev < 0.5){
            gameController.exitZone();
        }
        value_prev = value;

        
        float speed = (float)( - value * 2 + 2.03);

        // Scoreボードにスピードを表示（デバッグ用）
        var MessageTextObj = GameObject.Find("ScoreBoardText1");
        Text txt = MessageTextObj.GetComponent<Text>();
        txt.text = "Speed = "+  speed;

        // GameControllerにSpeedを設定
        gameController.SetPlayBackSpeed( speed );

        if(biv_count > 0) {
            device.TriggerHapticPulse(biv_duration);
            biv_count--;
        }
        
    }


    public void start_vibration(int msec, ushort _biv_duration) {
        int fps = 90;
        biv_duration = _biv_duration;
        biv_count = fps * msec / 1000;
    }
}
