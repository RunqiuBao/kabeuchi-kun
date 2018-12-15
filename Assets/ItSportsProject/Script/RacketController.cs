using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class RacketController : MonoBehaviour {

    //ボールがラケットに接触し続ける時間（設定で変更）
    public float BallAttachPeriod = 0.05f;

    // ラケットの反発係数 0..1
    public float BallReboundRatio = 0.5f;

    // トレース時に使うラケットとボールのPrefab
    public GameObject RacketTraceHitPrefab;
    public GameObject BallTraceHitPrefab;

    // GameControllerへの参照
    GameController gameController;

    // Racketでボールがあった相対位置
    Vector3 ballContactPoint;

    // ラケットに当たった時のボールの絶対位置
    Vector3 ballPositionOnHit;

    // ボールがラケットに接触しているか？この間ボールはラケットとともに動く
    bool isBallAttached;

    // ボールがラケットに接触した時刻
    float ballAttachedTime;

    // ラケットに接触した時のボールの速度
    Vector3 ballSpeedOnHit;

    // 初期のラケットとコントローラーの位置の差分。
    // 今トーラーの位置にこれを足すとラケットの位置が決まる。
    Vector3 gapPositionRacketController = new Vector3(0,0,0);
    Quaternion gapRotationRacketController;

    // Use this for initialization
    void Start () {
        // GameControllerへの参照を取得
        gameController = FindObjectOfType<GameController>();

        // RacketにBallは接触していない状態に初期化
        isBallAttached = false;

    }

    // Update is called once per frame
    void Update () {

        /*
        // Controllerの位置に合わせてラケットを動かす。Controllerは同期するまで登場しないので、こういう記述
        GameObject controller = GameObject.Find("Controller (left)");
        if (controller != null)
        {
            if (gapPositionRacketController == new Vector3(0, 0, 0))
            // 初期位置の計算が済んでいない
            {
                Transform tr = GetComponent<Transform>();
                // ラケットの位置とコントローラの位置の差分。引き算で計算
                gapPositionRacketController = tr.position - controller.GetComponent<Transform>().position;
                // ラケットの方向とこんとろーらの方向の差分。ラケットの方向にコントローラの方向の逆数をかけることで計算  
                gapRotationRacketController = tr.rotation * Quaternion.Inverse(controller.GetComponent<Transform>().rotation);

            }
            else
            {
                // Controllerの位置に合わせてラケットを動かす。

                Transform controllerTransform = controller.GetComponent<Transform>();

                Vector3 newPos = controllerTransform.position + gapPositionRacketController;
                Quaternion newAngle = controllerTransform.rotation * gapRotationRacketController;

                Rigidbody rb = GetComponent<Rigidbody>();
                // 位置の移動はTransformではなくRigidbodyのMoveを利用する
                // これで細かい衝突が検知される。
                rb.MovePosition(newPos);
                rb.MoveRotation(newAngle);
           
            }
        }

        */

        // ボールが接触している場合、ラケットの位置に合わせてボールを移動。
        if (isBallAttached)
        {
            // RacketのTransformを取得
            Transform tr = gameObject.GetComponent<Transform>();
            // Racket上のBallの相対位置を現在のRacketの位置に合わせて絶対座標へ変換
            Vector3 newBallPos = tr.TransformPoint(ballContactPoint);
            // 新しい場所へBallを移動(Speedは0)
            gameController.setBallPos(newBallPos, new Vector3(0, 0, 0));
            // 「ボールがラケットに接触し続ける時間」が過ぎたら、ボールをリリースする
            if (Time.time > ballAttachedTime + BallAttachPeriod)
            {
                // HitLastRacketはリリース時のRacketの絶対座標
                // 当たった時からリリース時までのBallの距離をRacketの速度とみなし、Ballの速度に設定
                Vector3 newBallSpeed = (newBallPos - ballPositionOnHit) / BallAttachPeriod;

                // ラケットに当たった時のボールの速度に反発係数をかけ、ラケットの向きに。
                // Racketの法線ベクトルを取得(RacketVector, RacketCenterはそれぞれRacket上に配置）
                Vector3 racketVector =
                    GameObject.Find("RacketVector").GetComponent<Transform>().position -
                    GameObject.Find("RacketCenter").GetComponent<Transform>().position;

                // Racketの法線ベクトルと衝突時のBallの速度の内積が負の場合、ユーザはRacketを逆に持っている。法線ベクトルを逆に
                if (Vector3.Dot(racketVector, ballSpeedOnHit) > 0)
                    racketVector *= -1;

               // Debug.Log("Racket:Racket Vector:" + racketVector);

                // Racketの法線方向に、衝突時の速度と同じ大きさの速度と反発係数をかけ、ラケットの速度と足して、新しいボールの速度に
                newBallSpeed += racketVector.normalized * ballSpeedOnHit.magnitude * BallReboundRatio;

                Debug.Log("Racket:releaes Ball speed 2:" + newBallSpeed);
                // BallはRacketを離れた
                isBallAttached = false;
                // GameController経由でBallの速度を設定
                gameController.setBallPos(newBallPos, newBallSpeed);
            }
        }
    }

    // 物理演算エンジンにより、Racketが何かと衝突した際に呼ばれる。
    private void OnCollisionEnter(Collision collision)
    {
        // すでにあたっていたら、以下は実行しない
        if (isBallAttached)
            return;

        // Collision.gameObjectにはあたった対象物が入っている。
        GameObject ball = collision.gameObject;
        Transform ballTr = ball.GetComponent<Transform>();

        Debug.Log("Racket:OnCollisionEnter. Collide with cp "+ ballTr.position);

        // GameControllerにボールが当たったサウンドの再生を指示
        gameController.PlayBallHitSound();

        // Collision.contacts[]には衝突した場所が格納されている。
        ContactPoint cp = collision.contacts[0];

        // ラケットに当たった時のボールの位置を記憶（後でボールの跳ね返り速度の計算に利用）
        ballPositionOnHit = cp.point;

        // Racket座標系におけるボールとの衝突位置を計算し、ballContactPointに格納
        Transform tr = GetComponent<Transform>();
        ballContactPoint = tr.InverseTransformPoint(ballPositionOnHit );

        // BallはRacketと接触している(Flagをセット）
        isBallAttached = true;

        // BallがRacketと接触した時刻とBallの速度を記憶
        ballAttachedTime = Time.time;
        ballSpeedOnHit = ball.GetComponent<BallController>().GetLastVelocity();

        //　ボールをいったん止める
        gameController.setBallPos( cp.point, new Vector3(0, 0, 0));

        // ScoreBoardにヒット位置を表示と記録
        gameController.TraceHitRacket(ballContactPoint);

        // ふりぬいたラケットに再度ボールが当たることを避けるため、Colliderを無効に（後で戻す必要あり）
        //        gameObject.GetComponent<MeshCollider>().enabled = false;
        BoxCollider [] colliders;
        colliders = gameObject.GetComponents<BoxCollider>();
        foreach (BoxCollider col in colliders) {
            col.enabled = false;
        }
       

    }

    /************************************************
     *                                              *
     *      ここから外部向けMethod                  *
     *                                              *
     ************************************************/


    // Racketの座標と向きを返す
    public void GetPosition(ref Vector3 pos, ref Quaternion dir)
    {
        Transform transform = GetComponent<Transform>();

        pos = transform.position;
        dir = transform.rotation;

        return;
    }

    // Racketを再起動（いったん無効になった衝突を再度有効に）
    public void RestartRacket()
    {
        //        gameObject.GetComponent<MeshCollider>().enabled = true;
        gameObject.GetComponent<BoxCollider>().enabled = true;

// RacketのColliderを復活
        BoxCollider[] colliders;
        colliders = gameObject.GetComponents<BoxCollider>();
        foreach (BoxCollider col in colliders)
        {
            col.enabled = true;
        }

        var MessageTextObj = GameObject.Find("ScoreBoardText2");
        Text txt = MessageTextObj.GetComponent<Text>();
        txt.text = "--------";

    }

    // デバッグ用メソッド
    public void setPosition(Vector3 pos, Quaternion dir)
    {
        Transform transform = GetComponent<Transform>();

        transform.position = pos;
        transform.rotation = dir;
    }

}
