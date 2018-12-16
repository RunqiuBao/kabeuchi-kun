using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BallController : MonoBehaviour
{
// Reference to GameController
    GameController gameController;

    public GameObject ballSpotPrefab;

    // この領域を外れるとボールを無効にする
    public float xMin = 6;
    public float xMax = 22;
    public float yMin = -10;
    public float yMax = 10;
    public float zMin = -13;
    public float zMax = 12;

    // ボールが有効な最大時間。これを超えたら無効にする
    public float maxTimeToLive = 6.0f;

    // Play中何個の軌跡を表示するか。
    public int maxNumberOfBallTrace = 5;

    // Ballが有効か？
    bool ballIsLive;

    // Ballと衝突したかどうかのFlag。このFlagが立つ直前のRacketの速度を記録するため利用
    bool ballCollideWithRacket = false;
    // Racketと衝突する直前のBallの速度
    Vector3 lastVelocity;

    // Ballの軌跡を管理するList
    List<GameObject> ballTrace;

    // Ballが飛び始めた時刻。タイムアウトによりBallは消える。
    float birthTime;
    
    // Audioを鳴らすためのComponent
    AudioSource sound=null;

    //　Racket Object.衝突相手の判断に利用
    GameObject racket;

    // Use this for initialization
    void Start()
    {
        Debug.Log("New ball is instantiated");

        // GameControllerオブジェクトを取得する。
        gameController = FindObjectOfType<GameController>();

        // GameControllerを呼び、RacketのGameObjectを取得
        racket = gameController.GetRacket();

        // 音を嗄声するためのAudioSource Component
        sound = GetComponent<AudioSource>();

        StartBall();

    }

    // Update is called once per frame
    void Update()
    {
        float x, y, z;
        Rigidbody rb = GetComponent < Rigidbody > ();

        // Ballが無効になっていたら処理終わり。
        if (!ballIsLive)
        {
            return;
        }

        // Racketにぶつかっていなかったら速度を記録。
        if (!ballCollideWithRacket)
        {
            lastVelocity = rb.velocity;
        }

        Transform trans = GetComponent<Transform>();
        x = trans.position.x;
        y = trans.position.y;
        z = trans.position.z;

        // Ballの軌跡を表示
        ballTrace.Add(gameController.CreateBallTrace(new Vector3(x, y, z)));

        // 画面内には最大maxNumberOfBallTraceの軌跡残す。それ以上になったら、最も先頭（古い）のものを消す
        if (ballTrace.Count > maxNumberOfBallTrace)
        {
            Destroy(ballTrace[0]);
            ballTrace.RemoveAt(0);
        }

        // 範囲から外に出るか、maxTimeToLiveを過ぎたらBallを無効にする。
        if (( Time.time > birthTime + maxTimeToLive ) || (x < xMin) || (x > xMax) || (y < yMin) || (y > yMax) || (z < zMin) || (z > zMax))
        {
// GameControllerに通知
            gameController.SetBallOut();

            ballIsLive = false;

            Destroy(gameObject, .5f);
        }

    }

    // 物理演算エンジンにより、Ballが何かと衝突した際に呼ばれる。
    private void OnCollisionEnter(Collision collision)
    {
        Transform transform = GetComponent<Transform>();
        Vector3 pos = transform.position;

        //        gameController.GetComponent<GameController>().RecordData("220 BALL_COL: " + collision.gameObject.name +","+ pos.x + "," + pos.y + "," + pos.z);
        gameController.RecordData("220 BALL_COL: " + collision.gameObject.name + "," + pos.x + "," + pos.y + "," + pos.z);

        // Racketと衝突したFlagを立てる（このFlagが立つとSpeedの記録を中止する）
        if (collision.gameObject == racket)
        {
            ballCollideWithRacket = true;

        }
        
    }

    void OnCollisionStay(Collision collision)
    {
        Vector3 speed = new Vector3();
        GetSpeed(ref speed);
        print(speed);
        float speedThreshold = 0.5f;
        // ボールの跡をつける
        if(collision.gameObject.tag.Contains("Court") && Mathf.Abs(speed.y) > speedThreshold) 
        {
            // BUG 床との衝突判定が2回起こっているらしい
            Instantiate (ballSpotPrefab, collision.contacts[0].point, new Quaternion(0,0,0,0));
        }
    }

    /************************************************
     *                                              *
     *      ここから外部向けMethod                  *
     *                                              *
     ************************************************/


    public void StartBall()
    {
        Debug.Log("Ball:Start ");

        //　ボールの軌跡を記録する
        ballTrace = new List<GameObject>();

        birthTime = Time.time;
        ballIsLive = true;
        ballCollideWithRacket = false;
    }

    // Ballの衝突音を再生
    public void PlayHitSound()
    {
        if ( sound == null)
            sound = GetComponent<AudioSource>();

        sound.PlayOneShot(sound.clip);
    }

    // 記録していたBallの軌跡をすべて削除する
    public void destroyTraces()
    {
        foreach (GameObject go in ballTrace)
        {
            Destroy( go);
        }
    }

    // Ballの位置を取得
    public void GetPosition(ref Vector3 pos, ref Quaternion dir)
    {
        Transform transform = GetComponent<Transform>();

        pos = transform.position;
        dir = transform.rotation;

        return;
    }

    // Ballの位置を設定
    public void SetPosition(Vector3 pos, Quaternion dir)
    {
        Transform transform = GetComponent<Transform>();

        transform.position = pos;
        transform.rotation = dir;
    }

    // Ballの速度を設定
    public void SetSpeed(Vector3 speed)
    {
        Rigidbody rb = GetComponent<Rigidbody>();
        rb.velocity = speed;
    }

    // Ballの速度を取得
    public void GetSpeed(ref Vector3 speed)
    {
        Rigidbody rb = GetComponent<Rigidbody>();
        speed = rb.velocity;
    }

    // Racketに衝突する直前の速度を取得(Updateの中で記録）
    public Vector3 GetLastVelocity()
    {
        return lastVelocity;
    }
}

