using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class BallController : MonoBehaviour
{
// Reference to GameController
    GameController gameController;

    public GameObject ballSpotPrefab;

    // この領域を外れるとボールを無効にする
    public float xMin;
    public float xMax;
    public float yMin;
    public float yMax;
    public float zMin;
    public float zMax;

    // ボールが有効な最大時間。これを超えたら無効にする
    public float maxTimeToLive;

    // Play中何個の軌跡を表示するか。
    public int maxNumberOfBallTrace;

    // Ballが有効か？
    bool ballIsLive;
    bool flagBallKill;

    // Ballが打たれてからワンバウンドしたか
    bool ballBounded;

    // Ballと衝突したかどうかのFlag。このFlagが立つ直前のRacketの速度を記録するため利用
    bool ballCollideWithRacket = false;
    bool playersBall = false;
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
        if (flagBallKill)
        {
            ballIsLive = false;
            flagBallKill = false;
            StartCoroutine(delaykillBall(1.5f));
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
            flagBallKill = true;
        }

    }

    IEnumerator delaykillBall(float delay){
        yield return new WaitForSeconds(delay);

        // GameControllerに通知
        gameController.SetBallOut();
        Destroy(gameObject);
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
            ballBounded = false;
            playersBall = true;
        }
        else if(collision.gameObject.tag.Contains("Court") && ballIsLive) // コートと接触時
        {
            // ワンバウンド目
            if(! ballBounded) {
                // ボールの跡をつける
                Instantiate (ballSpotPrefab, collision.contacts[0].point, new Quaternion(0,0,0,0));

                // 球の判定
                bool validShot;
                if(playersBall) { // 自分が打った球なら
                    validShot = collision.gameObject.tag.Contains("OpponentCourt");
                }
                else{ // 相手が打った球なら
                    validShot = collision.gameObject.tag.Contains("MyCourt");
                }

                // ジャッジのエフェクト
                Color effectColor = validShot ? Color.green : Color.red;
                var effectController = collision.gameObject.GetComponent<ColorEffectController>();
                effectController.startColorEffect(effectColor);
                ballBounded = true;

                if(validShot == false){
                    flagBallKill = true;
                }
            } else {
                flagBallKill = true;
            }
        }
        else if(collision.gameObject.tag.Contains("Net")) {
            // ネットと接触時の処理
        }
        else if(collision.gameObject.tag.Contains("Server")) {
        }
        else if(collision.gameObject.tag.Contains("Kabeuchikun") && ballIsLive) {
            Vector3 initialSpeed = new Vector3(0,7,12);
            SetSpeed(initialSpeed);

            ballBounded = false;
            playersBall = false;

            var KabeuchikunController = collision.gameObject.GetComponent<KabeuchikunController>();
            KabeuchikunController.hit();

        }
        else{
            // アウトとみなす
            flagBallKill = true;
            print(collision.gameObject);
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
        ballBounded = false;
        playersBall = false;
    }

    // Ballの衝突音を再生
    public void PlayHitSound(float volume)
    {
        if ( sound == null)
            sound = GetComponent<AudioSource>();
        
        sound.volume = volume;　//音量を操作
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

