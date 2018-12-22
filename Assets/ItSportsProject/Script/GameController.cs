using System.IO;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.PostProcessing;

public class GameController : MonoBehaviour {

    // Traingの種類
    public enum TrainingType
    {
        FixedOrder, Random, FirstOnly
    }

    //　一回のサーブのセット
    public class TrainingSet
    {
        // ボールの初速（場所は固定）
        public Vector3 initialBallSpeed;
        // プレイヤーの場所（方向は不変）
        public Vector2 initialPlayerPosition;
    }

    // Traingの種類
    public TrainingType trainType;

    // ボールの初速（場所は固定）
    public Vector3[] initialBallSpeed;

    // プレイヤーの場所（方向は不変）
    public Vector2[] initialPlayerPosition;

    // Playに利用するBallのPrefab
    public GameObject BallPrefab;

    // Ballの軌跡表示に利用するPrefab
    public GameObject BallTracePrefab;

    // PlayBack中ボールとRacketの間の線の表示に利用するPrefab
    public GameObject DebugLinePrefab;

    // Racket GameObject
    public GameObject Racket;

    // Player GameObject
    public GameObject Player;

    // Racketの中央
    public GameObject RacketCenter;
    // Racketの軌跡表示用Prefab
    public GameObject RacketTracePrefab;
    // Racketのボールヒット位置表示用Prefab
//    public GameObject RacketTraceHitPrefab;


//    public float TraceDisplayTime;
    public UnityEngine.Video.VideoPlayer VideoController;

    public GameObject ScoreBoard;

    // BallのGameObject。毎回作成するため、publicでは設定しない
    GameObject BallInstance;
    
    BallController ballController;
    RacketController racketController;
    PlayerController playerController;

    ScoreBoardController scoreBoardController;

    // DATAを記録するディレクトリの名前
    string directoryName;
    // File Stream for DATA writing
    StreamWriter dataLog;

    // サーブビデオの再生を始めた時刻
    float startTime;
    // プレイ履歴の再生を始めた時刻
    float startTraceTime;
    // プレイ履歴の再生を始めてからの，再生速度も考慮した経過時間
    float traceTimeElapsed;
    // 最後にプレイ履歴を更新した時刻。
    float lastPlaybackTime;

    // プレイ履歴の再生速度。コントローラーから変更される
    float replaySpeed = 1;

    // InPlay状態が始まった(ボールがサーブされた）時刻
    float inplayStartTime;

    // InPlay状態が始まってから経過したゲーム内時間
    float timeElapsed;
    float lastFrameTime;

    // トレーニング（ボールの速度、プレイヤーの位置を格納）
    TrainingSet currentTraining;
    // トレーニングが複数ある場合、現在のトレーニングの番号
    int currentTrainingIndex;

    // Racketのボールがぶつかった回数。複数回の衝突を防ぐために利用
//    int racket_collision_count;

    // プレイ履歴を保存するClass。ボールの位置、ラケットの位置と方向および中心位置、時刻を含む
    class MyTrace
    {
        public Vector3 ballPos; // ボールの位置（ボールは球なので方向は不要）
        public Vector3 racketPos; // ラケットの位置
        public Quaternion racketDir; // ラケットの方向
        public Vector3 racketCenterPos; // ラケットの中心の位置
        public float time; // InPlay状態が始まってからの秒数
    }

    // 1球分ぼプレイ履歴（ボール、ラケット、自国）を記録
    List<MyTrace> Trace;

    // プレイバック用のGameObject
    GameObject playbackRacket;　// ラケット
    GameObject playbackBall;    // ボール
    GameObject playbackLine;    // ラケットとボールを結ぶライン
    List<GameObject> playbackBallTrace = new List<GameObject>();
    public int playbackMaxNumberOfBallTrace = 150;

    public float zoneTimeRate = 0.2f;
    
    // ゲームの状態を表す。
    enum State  {
        Served, // サーブのビデオ再生中
        InPlay, // ボールが生きている状態
        TraceDisplaying, // ボール、ラケットの軌跡を表示している状態
    };    
    State state ;

    // Use this for initialization
    void Start () {
    
        // 一球分のプレイ履歴を記録する
        Trace = new List<MyTrace>();

        // RacketController, PlayerControllerを取得
        racketController = Racket.GetComponent<RacketController>();
        playerController = Player.GetComponent<PlayerController>();
        scoreBoardController = ScoreBoard.GetComponent<ScoreBoardController>();

        // まずは初めのトレーニングセットから始める
        currentTrainingIndex = 0;

        // DATAはPLAYDATA/の配下に日付ごとにディレクトリを作成する。
        // 起動時にしかディレクトリを制御しないため、アプリ起動中に日付が変わっても同じディレクトリ使われる。

        // /PLAYDATA ディレクトリがなければ作成
        if (!Directory.Exists("PLAYDATA"))
            Directory.CreateDirectory("PLAYDATA");

        // 日付から作った ディレクトリ(例: PLAYDATA/20181019/ がなければ作成
        directoryName = System.DateTime.Now.ToString("PLAYDATA/yyyyMMdd");
        if (!Directory.Exists(directoryName))
            Directory.CreateDirectory(directoryName);

        // 初期状態である Servedへ移行
        StartServe();
    }

    // Update is called once per frame
    void Update()
    {
        //        Debug.Log("Update time :" + Time.deltaTime);

        if (state == State.Served)         {
            servedUpdate();
        }
        else if (state == State.InPlay) // ボールが動いている状態
        {
            inPlayUpdate();
        }
        else if (state == State.TraceDisplaying) 
        {
            TraceDisplayingUpdate();
        }
    }

    void servedUpdate() {
            // サーブのビデオを再生中。時間がたったらInPlay状態へ
            float time = Time.time - startTime;
            if (time >= 2.6f)　// サーブのビデオ内でサーブされる時刻。
            {
                // ボールを動かしだす。InPlay状態へ遷移
                StartInPlay();
            }
    }

    void inPlayUpdate() {
        //　InPlayの状態の時はBallControllerからSetBallOutを呼び出されると状態が変わる。この中では状態は変わらない。

        // ボールの位置を取得
        Vector3 ballPos = new Vector3(0,0,0);
        Quaternion ballDir = new Quaternion(0,0,0,0);
        ballController.GetPosition(ref ballPos, ref ballDir);
        // ボールの速度を取得
        Vector3 ballSpeed = new Vector3();
        BallInstance.GetComponent<BallController>().GetSpeed(ref ballSpeed);

        // ラケットの位置と方向を取得
        Vector3 racketPos = new Vector3(0, 0, 0);
        Quaternion racketDir = new Quaternion(0, 0, 0, 0);;
        racketController.GetPosition(ref racketPos, ref racketDir);

        // ラケットの中央位置を取得
        Vector3 racketCenterPos = RacketCenter.GetComponent<Transform>().position;

        // デバッグ用．ボールの前にラケットを転移
        if(Input.GetKeyDown("space"))
        {
            print("space pressed");
            float timeConst = 0.03f;
            Vector3 offset = racketCenterPos - racketPos;
            Vector3 newRacketPos = ballPos + ballSpeed * timeConst - offset;
            print(racketPos);
            racketController.setPosition(newRacketPos, racketDir);
        }

        // デバッグ用．zキーでゾーンモード
        if(Input.GetKeyDown("z"))
        {
            enterZone();
        }
        if(Input.GetKeyUp("z"))
        {
            exitZone();
        }

        // プレイ履歴に記録
        MyTrace tr = new MyTrace();
        tr.ballPos= ballPos;
        tr.racketPos = racketPos;
        tr.racketDir = racketDir;
        tr.racketCenterPos = racketCenterPos;
        tr.time = timeElapsed;
        Trace.Add(tr);
        timeElapsed += Time.deltaTime;

        //　BallをDATAへ記録
        RecordData("200 BALL_POS: " + ballPos.x + "," + ballPos.y + "," + ballPos.z + "," + ballSpeed.x + "," + ballSpeed.y + "," + ballSpeed.z);

        //　RacketをDATAへ記録
        RecordData("210 RCKT_POS: " + racketCenterPos.x + "," + racketCenterPos.y + "," + racketCenterPos.z + "," + racketDir.eulerAngles.x + "," + racketDir.eulerAngles.y + "," + racketDir.eulerAngles.z);
    }

    void TraceDisplayingUpdate(){
        // プレイ履歴を再生している状態。

        // 履歴がなくなったら、再生を終了。初期状態(Served)へ戻る。
        if (Trace.Count == 0)
        {
            FinishPlayback();
        }
        else
        {
            // 操作履歴の先頭のデータ(最も古い）を取り出す。
            MyTrace tr = Trace[0];

            // 前回履歴を表示してからの時刻に再生スピードをかけたものと
            // 履歴の中でたった時間を比較。
            while ( traceTimeElapsed >= tr.time && Trace.Count > 0) // time to go?
            {
                tr = Trace[0];
                // Playback用のラケットを移動
                MoveGameObject(playbackRacket, tr.racketPos);
                // Playback用のラケットの方向を変更
                RotateGameObject(playbackRacket, tr.racketDir);
                // Playback用のボールを移動
                MoveGameObject(playbackBall, tr.ballPos);

                // 軌跡の表示
                playbackBallTrace.Add( CreateBallTrace(tr.ballPos) );
                if (playbackBallTrace.Count > playbackMaxNumberOfBallTrace)
                {
                    Destroy(playbackBallTrace[0]);
                    playbackBallTrace.RemoveAt(0);
                }
                foreach(GameObject trace in playbackBallTrace)
                {
                    Color colorFade = new Color(0,0,0,1.0f/playbackMaxNumberOfBallTrace);
                    Renderer renderer = trace.GetComponent<Renderer>();
                    if(renderer.material.color.a > 0)
                    {
                        renderer.material.color -= colorFade; 
                    }
                }

                // ボールとラケットを結ぶ線
                // LineRenderer lr = playbackLine.GetComponent<LineRenderer>();
                // lr.SetPosition(0, tr.ballPos);
                // lr.SetPosition(1, tr.racketCenterPos);

                //　履歴の先頭を削除
                Trace.RemoveAt(0);
            }
            traceTimeElapsed += Time.deltaTime * GetPlayBackSpeed();
        }

    }

    // GameObjectを移動する
    void MoveGameObject(GameObject obj, Vector3 pos)
    {
        Transform tr = obj.GetComponent<Transform>();
        tr.position = pos;
        return;
    }

    // GameObjectの方向を設定する
    void RotateGameObject(GameObject obj, Quaternion dir)
    {
        Transform tr = obj.GetComponent<Transform>();
        tr.rotation = dir;
        return;
    }

    // Serverのビデオを先頭から再生
    void PlayVideo()
    {
        VideoController.frame = 0;
        VideoController.Play();
    }


    // 次のトレーニングセット（ボールの速度、プレイヤーの位置）を取得。モードは３つ
    TrainingSet GetNextTraining()
    {
        TrainingSet training = new TrainingSet();
        training.initialBallSpeed = initialBallSpeed[currentTrainingIndex];
        training.initialPlayerPosition = initialPlayerPosition[currentTrainingIndex];

        switch (trainType)
        {
            // 固定順。順番に繰り替えす
            case TrainingType.FixedOrder:
                if (++currentTrainingIndex == initialBallSpeed.Length)
                    currentTrainingIndex = 0;
                break;

            // ランダム
            case TrainingType.Random:
                currentTrainingIndex = Random.Range(0, initialBallSpeed.Length);
                break;

            // 初めのものだけ利用。
            case TrainingType.FirstOnly:
                currentTrainingIndex = 0;
                break;
        }

        return training;
    }

    // ここから状態を変化させるときの処理

    // Servedの状態が始まる。
    void StartServe()
    {
        Debug.Log("GameController:StartServe() ");
        state = State.Served;

        // 開始時刻を記録
        startTime = Time.time;

        // 出力用のファイルを作成
        string filename = directoryName +"/" + "data" + System.DateTime.Now.ToString("yyyyMMdd-HHmmss") + ".txt";
        dataLog = new StreamWriter(filename);
        RecordData("110 Ball Start");

        // トレーニングセットを取得
        currentTraining = GetNextTraining();

        // トレーニングセットに従い、プレイヤーの位置を移動
        SetPlayerPosition(currentTraining.initialPlayerPosition);

        // ラケットを初期化
        racketController.RestartRacket();

        // ヒット用のラケットとボールを隠す（下の方へ移動
        GameObject hitPointBall = GameObject.Find("BallHitInCourt");
        Transform tr = hitPointBall.GetComponent<Transform>();
        tr.position = new Vector3( 0 -100, 0);

        GameObject hitPointRacket = GameObject.Find("RacketHitInCourt");
        tr = hitPointRacket.GetComponent<Transform>();
        tr.position = new Vector3(0 - 100, 0);

        scoreBoardController.enterPlay();
        // ビデオを再生開始
        PlayVideo();

    }

    //　InPlayの状態が始まる。
    void StartInPlay()
    {
        state = State.InPlay;

        // ボールをPrefabから生成。初期位置が定数なのはかっこ悪い。
        // ボールは自分で自分をdestroyするので、ここからの削除は不要
        BallInstance = Instantiate(BallPrefab, new Vector3(18.38f, 2.71f, -12.8f), new Quaternion(0, 0, 0, 0));

        ballController = BallInstance.GetComponent<BallController>();

        // 初期スピードを設定   
        ballController.SetSpeed(currentTraining.initialBallSpeed);

        PlayBallHitSound(0.8f);
       
        // 開始時刻を記録
        inplayStartTime = Time.time;
        timeElapsed = 0;

    }

    //　TraceDisplayingの状態が始まる。
    void StartTraceDisplay()
    {
        exitZone();
        traceTimeElapsed = 0;
        scoreBoardController.enterReplay();

        state = State.TraceDisplaying;

        playbackBallTrace = new List<GameObject>();
        
        // トレース用のラケット、ボール、ラインを生成
        playbackRacket = Instantiate(RacketTracePrefab);
        playbackBall = Instantiate(BallTracePrefab);
        playbackLine = Instantiate(DebugLinePrefab);

        // ボールのトレース（後ろに５つつくもの）を削除
        ballController.destroyTraces();

        // 時刻を記録
        startTraceTime = Time.time;
        lastPlaybackTime = 0;

    }

    // TraceDisplayingの状態から Servedへ移行
    void FinishPlayback()
    {
        // トレース用のオブジェクトを削除
        Destroy(playbackRacket);
        Destroy(playbackBall);
        Destroy(playbackLine);

        foreach(GameObject trace in playbackBallTrace){
            Destroy(trace);
        }

        // close data file
        RecordData("150 Ball finished");
        dataLog.Close();
    
        // 初期状態（ビデオ生成から）へ
        StartServe();

    }

    // ここまで状態を変化させるときの処理


    /************************************************
     *                                              *
     *      ここから外部向けMethod                  *
     *                                              *
     ************************************************/

    // InPlay時にBallControllerより、Ballが無効になったことを受ける。
    public void SetBallOut()
    {
        Debug.Log("GameController:SetBallOut()");
        // トレース表示状態へ
        StartTraceDisplay();
    }

    // Ballがなにかに当たった音を再生。BallControllerを呼び出す。
    public void PlayBallHitSound(float volume)
    {
        Debug.Log("GameController: PlayBallHit()");
        ballController.PlayHitSound(volume);
    }

    // ボールがラケットに当たった。
    // 壁へ当たった位置の表示、表示位置へのボールとラケットの表示など
    public void TraceHitRacket(Vector3 hitPoint)
    {
        Debug.Log("Racket hit ball on " + hitPoint);

        GameObject racket = GameObject.Find("Racket");
       Transform racket_transform = racket.GetComponent<Transform>();

        // コート内の衝突位置にラケットを表示
        GameObject racketTrace = GameObject.Find("RacketHitInCourt");
        Transform traceTransform = racketTrace.GetComponent<Transform>();

        traceTransform.position = racket_transform.position;
        traceTransform.rotation = racket_transform.rotation;


        // コート内の衝突位置にボールを配置
        // hitPointはラケット内の相対位置のため、TransformPointで絶対座標へ変換
        GameObject ball = GameObject.Find("BallHitInCourt");
        ball.GetComponent<Transform>().position = racket_transform.TransformPoint(hitPoint);

        //壁のディスプレイへ衝突位置を表示
        GameObject hitPointBall = GameObject.Find("BallHitInCanvas");
        Transform tr = hitPointBall.GetComponent<Transform>();
        tr.localPosition = hitPoint;

    }


    public GameObject CreateBallTrace(Vector3 pos)
    {
        return ( Instantiate( BallTracePrefab, pos, new Quaternion(0, 0, 0, 0)) );
    }

    // Ballの場所と速度を設定する
    public void setBallPos(Vector3 newPos, Vector3 speed)
    {
        if (BallInstance != null)
        {
            // BallCntrollerを呼び出し、位置をセット
              BallInstance.GetComponent<BallController>().SetPosition(newPos, Quaternion.identity  );

           // 速度をセット
              BallInstance.GetComponent<BallController>().SetSpeed(speed);

        }
    }

    // Replay時の速度(1...等倍　　2..倍速）を設定する。主にControllerControllerから呼ばれる。
    public void SetPlayBackSpeed(float speed)
    {
        replaySpeed = speed;
    }

    // Replay時の速度(1...等倍　　2..倍速）を取得する。
    public float GetPlayBackSpeed()
    {
        return replaySpeed;
    }

    // Racket GameObjectを取得する。
    public GameObject GetRacket()
    {
        GameObject racket = GameObject.FindGameObjectWithTag("Racket");
        return racket;
    }

    // Playerの位置を変更する。
    public void SetPlayerPosition(Vector2 position)
    {
// PlayerControllerを呼び出し、現在の位置と方向を取得。
        Vector3 pos = new Vector3(0,0,0);
        Quaternion dir = new Quaternion(0,0,0,0);
        playerController.GetPosition(ref pos, ref dir);

// 位置を新しいものに変更。方向は以前のものをそのまま利用する
        pos.x = position.x;
        pos.z = position.y;

        playerController.SetPosition(pos, dir);

    }

    // TEXTデータを記録
    public void RecordData(string str)
    {
        dataLog.Write(System.DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss.fff - "));
        dataLog.WriteLine(str);
    }

    // ゾーンモードに突入
    public void enterZone() {
        // プレイ中以外は処理しない
        if(state != State.InPlay){
            return;
        }
        Time.timeScale = zoneTimeRate;
        var eye = GameObject.Find("Camera (eye)");
        var behaviour = eye.GetComponent<PostProcessingBehaviour>();
        behaviour.enabled = true;
    }


    // ゾーンモードを終了
    public void exitZone() {
        // プレイ中以外は処理しない
        if(state != State.InPlay){
            return;
        }
        Time.timeScale = 1.0f;
        var eye = GameObject.Find("Camera (eye)");
        var behaviour = eye.GetComponent<PostProcessingBehaviour>();
        behaviour.enabled = false;
    }
}
