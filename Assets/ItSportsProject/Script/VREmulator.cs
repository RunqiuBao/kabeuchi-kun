using System.Collections;
using System.Collections.Generic;
using UnityEngine;
 
public class VREmulator : MonoBehaviour
{
    // カメラオブジェクトを格納する変数
    public Camera mainCamera;
    // カメラの回転速度を格納する変数
    public Vector3 rotationSpeed;
    // マウス移動方向とカメラ回転方向を反転する判定フラグ
    public bool reverse;
    // マウスホイールによる全身交代の速度
    public float forwardSpeed;
    // マウス座標を格納する変数
    private Vector2 lastMousePosition;
    // カメラの角度を格納する変数（初期値に0,0を代入）
    private Vector3 newAngle = new Vector2(0, 0);

    private void Start()
    {
        // メインカメラが未指定の場合、Cameraを探す。
        // これは"MainCamera"というTagが付いたカメラを探す方法
        if (mainCamera == null)
        {
            mainCamera = Camera.main;
        }
        //　それでも見つからない場合は"Main Camera"という名前を探す
        if (mainCamera == null)
        {
            GameObject obj = GameObject.Find("Main Camera");
            if (obj != null)
            {
                mainCamera = obj.GetComponent<Camera>();
            }
        }

        if (mainCamera == null)
        {
            Debug.LogError("VREmulator.cs: Can't find Main Camera. Add Tag \"MainCamera\" or name it \"Main Camera\". ");
        }
    }
    // ゲーム実行中の繰り返し処理
    void Update()
    {
        // 左クリックした時
        if (Input.GetMouseButtonDown(0))
        {
            // カメラの角度を変数"newAngle"に格納
            newAngle = mainCamera.transform.localEulerAngles;
            // マウス座標を変数"lastMousePosition"に格納
            lastMousePosition = Input.mousePosition;
        }
        // 左ドラッグしている間
        else if (Input.GetMouseButton(0))
        {
            //カメラ回転方向の判定フラグが"true"の場合
            if (!reverse)
            {
                // Y軸の回転：マウスドラッグ方向に視点回転
                // マウスの水平移動値に変数"rotationSpeed"を掛ける
                //（クリック時の座標とマウス座標の現在値の差分値）
                newAngle.y -= (lastMousePosition.x - Input.mousePosition.x) * rotationSpeed.y;
                // X軸の回転：マウスドラッグ方向に視点回転
                // マウスの垂直移動値に変数"rotationSpeed"を掛ける
                //（クリック時の座標とマウス座標の現在値の差分値）
                newAngle.x -= (Input.mousePosition.y - lastMousePosition.y) * rotationSpeed.x;
                // "newAngle"の角度をカメラ角度に格納
                mainCamera.transform.localEulerAngles = newAngle;
                // マウス座標を変数"lastMousePosition"に格納
                lastMousePosition = Input.mousePosition;
            }
            // カメラ回転方向の判定フラグが"reverse"の場合
            else if (reverse)
            {
                // Y軸の回転：マウスドラッグと逆方向に視点回転
                newAngle.y -= (Input.mousePosition.x - lastMousePosition.x) * rotationSpeed.y;
                // X軸の回転：マウスドラッグと逆方向に視点回転
                newAngle.x -= (lastMousePosition.y - Input.mousePosition.y) * rotationSpeed.x;
                // "newAngle"の角度をカメラ角度に格納
                mainCamera.transform.localEulerAngles = newAngle;
                // マウス座標を変数"lastMousePosition"に格納
                lastMousePosition = Input.mousePosition;
            }
        }

        // 右クリックはZ回転
        if (Input.GetMouseButtonDown(1))
        {
            // カメラの角度を変数"newAngle"に格納
            newAngle = mainCamera.transform.localEulerAngles;
            // マウス座標を変数"lastMousePosition"に格納
            lastMousePosition = Input.mousePosition;
        }
        // 左ドラッグしている間
        else if (Input.GetMouseButton(1))
        {
            //カメラ回転方向の判定フラグが"true"の場合
            if (!reverse)
            {
                // Z軸の回転：マウスドラッグ方向に視点回転
                // マウスの水平移動値に変数"rotationSpeed"を掛ける
                //（クリック時の座標とマウス座標の現在値の差分値）
                newAngle.z -= (lastMousePosition.x - Input.mousePosition.x) * rotationSpeed.z;
                // "newAngle"の角度をカメラ角度に格納
                mainCamera.transform.localEulerAngles = newAngle;
                // マウス座標を変数"lastMousePosition"に格納
                lastMousePosition = Input.mousePosition;
            }
            // カメラ回転方向の判定フラグが"reverse"の場合
            else if (reverse)
            {
                // Z軸の回転：マウスドラッグと逆方向に視点回転
                newAngle.z -= (Input.mousePosition.x - lastMousePosition.x) * rotationSpeed.y;
                // "newAngle"の角度をカメラ角度に格納
                mainCamera.transform.localEulerAngles = newAngle;
                // マウス座標を変数"lastMousePosition"に格納
                lastMousePosition = Input.mousePosition;
            }
        }
        float scroll = Input.GetAxis("Mouse ScrollWheel");
        mainCamera.transform.position += mainCamera.transform.forward * scroll * forwardSpeed;
    }

    // マウスドラッグ方向と視点回転方向を反転する処理
    public void DirectionChange()
    {
        // 判定フラグ変数"reverse"が"false"であれば
        if (!reverse)
        {
            // 判定フラグ変数"reverse"に"true"を代入
            reverse = true;
        }
        // でなければ（判定フラグ変数"reverse"が"true"であれば）
        else
        {
            // 判定フラグ変数"reverse"に"false"を代入
            reverse = false;
        }
    }
}
