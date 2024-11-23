using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Const;

// 環境の状態を観測し, コントローラーを通じて行動を行う
public class CartPoleAgent3D : Agent
{
    private CartController3D controller;

    // CartPoleの各パーツ
    private const int RbNum = 3;
    private Rigidbody[] rbs;
    private Rigidbody cartRb;
    private Rigidbody poleRb;
    private Rigidbody jointRb;

    // CartPoleの各パーツの初期位置・回転のリスト
    private Vector3[] startPositions = new Vector3[RbNum];
    private Quaternion[] startRotations = new Quaternion[RbNum];

    // CartPoleの前ステップでの位置・角度
    private float lastPositionX;
    private float lastPositionY;
    private float lastPositionZ;
    private float lastAngleX;
    private float lastAngleZ;

    // CartPoleが動くことのできる最大範囲
    public float maxDistance = 2.0f;
    
    // Poleの初期角度の最大値
    public float maxStartAngle = 20.0f;

    // Poleが倒れた判定になる角度．単位は度
    public float maxAngle = 90.0f;

    // 現在のステップ数
    private int currentStep;

    // 最大のステップ数．超えたら終了
    [SerializeField] int maxStep = 1000;

    // 現在のCartPoleの状態
    // [0]: Cartのx座標
    // [1]: Cartのy座標
    // [2]: Cartのz座標
    // [3]: Cartのx軸方向の速度
    // [4]: Cartのy軸方向の速度
    // [5]: Cartのz軸方向の速度
    // [6]: Poleのyz平面に射影したときの角度
    // [7]: Poleのxy平面に射影したときの角度
    // [8]: Poleのyz平面に射影したときの角速度
    // [9]: Poleのxy平面に射影したときの角速度
    private const int StateNum = 10;
    private double[] currentState = new double[StateNum];

    // 現在の状態を表示するためのText
    [SerializeField] private Text currentStateText;

    void Awake(){
        // エージェントの状態のサイズ
        StateSize = 10;
        // エージェントに入力できるアクションのサイズ
        ActionSize = 3;
    }

    void Start(){
        // CartPoleの各パーツ, コントローラーの登録
        controller = transform.Find("Cart").GetComponent<CartController3D>();
        cartRb = transform.Find("Cart").GetComponent<Rigidbody>();
        poleRb = transform.Find("Pole").GetComponent<Rigidbody>();
        jointRb = transform.Find("Joint").GetComponent<Rigidbody>();
        rbs = new Rigidbody[RbNum] {cartRb, poleRb, jointRb};
        for(int i = 0; i < RbNum; i++){
            startPositions[i] = rbs[i].position;
            startRotations[i] = rbs[i].rotation;
        }

        lastPositionX = 0.0f;
        lastPositionY = 0.0f;
        lastPositionZ = 0.0f;

        // Poleをランダムに傾ける
        SetCartPoleKinematic(true);     // CartPoleを直接動かすとき安全のためにKinematicをtrueにする
        RotatePole(Random.Range(-maxStartAngle, maxStartAngle));
        SetCartPoleKinematic(false);    // 動かし終わったらfalseに戻す
        (lastAngleX, lastAngleZ) = GetAngle();

        currentState = new double[StateNum] {
            lastPositionX,
            lastPositionY,
            lastPositionZ,
            0.0,            // VelX
            0.0,            // VelY
            0.0,            // VelZ
            lastAngleX,
            lastAngleZ,
            0.0,            // AngVelX
            0.0             // AngVelZ
        };
        
        currentStep = 0;
        IsDone = false;
        Score = 0.0f;   
    }

    void FixedUpdate(){
        if(!IsDone){
            Score += Time.deltaTime;
            currentStep++;
            GetObservation();
            UpdateText();
        }
    }

    // エージェントの状態を初期状態に戻す
    // Poleの初期角度はランダムに設定しなおす
    public override void AgentReset(){
        SetCartPoleKinematic(true);// CartPoleを直接動かすとき安全のためにKinematicをtrueにする
        StopCartPole();
        ResetCartPole();
        lastPositionX = 0.0f;
        lastPositionY = 0.0f;
        lastPositionZ = 0.0f;
        RotatePole(Random.Range(-maxStartAngle, maxStartAngle));
        (lastAngleX, lastAngleZ) = GetAngle();
        SetCartPoleKinematic(false);// 動かし終わったらfalseに戻す

        currentState = new double[StateNum] {
            lastPositionX,
            lastPositionY,
            lastPositionZ,
            0.0,
            0.0,
            0.0,
            lastAngleX,
            lastAngleZ,
            0.0,
            0.0
        };
        currentStep = 0;
        IsDone = false;
        Score = 0.0f;
    }

    private void RotatePole(float angle){
        Vector3 center = jointRb.position;
        Quaternion rotateQuaternion = Quaternion.AngleAxis(Random.Range(-90.0f, 90.0f), Vector3.up);
        rotateQuaternion *= Quaternion.AngleAxis(angle, Vector3.forward);
        // Jointを原点からスタートさせるためにPoleの位置を調整する
        poleRb.position = rotateQuaternion * (poleRb.position - center) + center; 
        poleRb.rotation = rotateQuaternion;
    }

    // Poleの角度を測る. 単位は度
    // Poleが垂直に立っているときが0度でそこからの傾きを正負付きで返す
    // 返す値はPoleをyz平面とxy平面に射影してみたときのそれぞれの角度
    private (float, float) GetAngle(){
        Vector3 poleVec = poleRb.position - jointRb.position;
        Vector3 poleVecX = new Vector3(poleVec.x, poleVec.y, 0.0f     );
        Vector3 poleVecZ = new Vector3(0.0f     , poleVec.y, poleVec.z);
        Vector3 refVec   = new Vector3(0.0f     , 1.0f     , 0.0f     );
        Vector3 axisX    = new Vector3(0.0f     , 0.0f     , 1.0f     );
        Vector3 axisZ    = new Vector3(1.0f     , 0.0f     , 0.0f     );
        float angleX =  Vector3.SignedAngle(poleVecX, refVec, axisX);
        float angleZ = -Vector3.SignedAngle(poleVecZ, refVec, axisZ);
        return (angleX, angleZ);
    }

    // CartPoleの状態を[-1,1]の範囲に収まるように正規化する
    // ただし，速度と角速度は，位置と角度と同じスケールで必ずしも[-1,1]の範囲には収まらない
    // 入力：CartPoleの状態 doubleの配列
    // 出力：[-1,1]の範囲に収まるように正規化した配列 doubleの配列
    private double[] NormalizeState(double[] state){
        double[] normalized = new double[10];
        normalized[CO.StateX]     = state[CO.StateX]     / maxDistance;
        normalized[CO.StateY]     = state[CO.StateY]     / maxDistance;
        normalized[CO.StateZ]     = state[CO.StateZ]     / maxDistance;
        normalized[CO.StateVelX]    = state[CO.StateVelX]    / maxDistance;
        normalized[CO.StateVelY]    = state[CO.StateVelY]    / maxDistance;
        normalized[CO.StateVelZ]    = state[CO.StateVelZ]    / maxDistance;
        normalized[CO.StateAngX]  = state[CO.StateAngX]  / maxAngle;
        normalized[CO.StateAngZ]  = state[CO.StateAngZ]  / maxAngle;
        normalized[CO.StateAngVelX] = state[CO.StateAngVelX] / maxAngle;
        normalized[CO.StateAngVelZ] = state[CO.StateAngVelZ] / maxAngle;
        return normalized;
    }

    // 現在のCartPoleの状態を返す
    public override double[] GetState(){
        return NormalizeState(currentState);
    }

    // 現在のCartPoleの状態を観測
    // CurrentStateを更新する
    private void GetObservation(){
        // CurrentStateの更新
        currentState[CO.StateX]         = cartRb.position.x - startPositions[0].x; // startPositions[0]: CartRbの初期位置
        currentState[CO.StateY]         = cartRb.position.y - startPositions[0].y;
        currentState[CO.StateZ]         = cartRb.position.z - startPositions[0].z;
        currentState[CO.StateVelX]      = (currentState[CO.StateX] - lastPositionX) / Time.deltaTime;
        currentState[CO.StateVelY]      = (currentState[CO.StateY] - lastPositionY) / Time.deltaTime;
        currentState[CO.StateVelZ]      = (currentState[CO.StateZ] - lastPositionZ) / Time.deltaTime;
        (currentState[CO.StateAngX], currentState[CO.StateAngZ]) = GetAngle();
        currentState[CO.StateAngVelX]   = (currentState[CO.StateAngX] - lastAngleX)    / Time.deltaTime;
        currentState[CO.StateAngVelZ]   = (currentState[CO.StateAngZ] - lastAngleZ)    / Time.deltaTime;

        // 終了条件をみたしているかチェック
        if(currentState[CO.StateX]    < -maxDistance || maxDistance < currentState[CO.StateX])    IsDone = true;
        if(currentState[CO.StateY]    < -maxDistance || maxDistance < currentState[CO.StateY])    IsDone = true;
        if(currentState[CO.StateZ]    < -maxDistance || maxDistance < currentState[CO.StateZ])    IsDone = true;
        if(currentState[CO.StateAngX] < -maxAngle    || maxAngle    < currentState[CO.StateAngX]) IsDone = true;
        if(currentState[CO.StateAngZ] < -maxAngle    || maxAngle    < currentState[CO.StateAngZ]) IsDone = true;
        if(currentStep > maxStep) IsDone = true;

        // 直前の状態の更新
        lastPositionX = (float)currentState[CO.StateX];
        lastPositionY = (float)currentState[CO.StateX];
        lastPositionZ = (float)currentState[CO.StateZ];
        lastAngleX    = (float)currentState[CO.StateAngX];
        lastAngleZ    = (float)currentState[CO.StateAngZ];

    }

    // 表示テキストの更新
    private void UpdateText(){
        currentStateText.text = $"step: {currentStep}\n"
                              + $"position: {currentState[CO.StateX]:F2}, {currentState[CO.StateY]:F2}, {currentState[CO.StateZ]:F2}\n"
                              + $"velocity: {currentState[CO.StateVelX]:F2}, {currentState[CO.StateVelY]:F2},{currentState[CO.StateVelZ]:F2}\n"
                              + $"angle: {currentState[CO.StateAngX]:F2}, {currentState[CO.StateAngZ]:F2}\n"
                              + $"angular velocity: {currentState[CO.StateAngVelX]:F2}, {currentState[CO.StateAngVelZ]:F2}\n"
                              + $"duration: {Score:F2} sec.\n"
                              + $"IsDone: {IsDone}\n";
    }

    // コントローラーを通じてアクションを行う
    // 可能なアクションはCartにx軸方向, z軸方向に任意の大きさの力を加えること
    // 出力可能な最大の力はコントローラー側で決まっている
    public override void AgentAction(double[] action){
        controller.cartInputX = action[CO.ACTION_X];
        controller.cartInputY = action[CO.ACTION_Y];
        controller.cartInputZ = action[CO.ACTION_Z];
    }

    // CartPoleを強制的に止める
    private void StopCartPole(){
        for(int i = 0; i < RbNum; i++){
            rbs[i].velocity = Vector3.zero;
            rbs[i].angularVelocity = Vector3.zero;
        }
        controller.cartInputX = 0.0;
        controller.cartInputY = 0.0;
        controller.cartInputZ = 0.0;
    }

    // CartPoleを初期位置・回転に戻す
    private void ResetCartPole(){
        for(int i = 0; i < RbNum; i++){
            rbs[i].position = startPositions[i];
            rbs[i].rotation = startRotations[i];
        }
    }

    // CartPoleの全パーツのKinematicを一括で設定する
    private void SetCartPoleKinematic(bool isKinematic){
        for(int i = 0; i < RbNum; i++){
            rbs[i].isKinematic = isKinematic;
        }
    }
}
