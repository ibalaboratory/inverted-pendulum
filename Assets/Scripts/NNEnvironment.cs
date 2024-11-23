using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

// エージェントの管理をするクラス
public class NNEnvironment : MonoBehaviour
{
    // 制御するエージェントのオブジェクト
    [SerializeField] private GameObject agentGameObject;
    private Agent learningAgent;

    // エピソードの情報を表示するためのテキストオブジェクト
    [SerializeField] private Text episodeCountText;

    // ニューラルネットワーク
    [SerializeField] private GameObject brainGameObject;
    private NNBrain nnBrain;

    // 学習したエピソードの数
    private int episodeCount = 0;

    // 最高スコア
    private float bestScore = 0.0f;

    // 直前のエージェントの状態ととったアクション
    private double[] prevState;
    private double[] prevAction;

    // スコアを記録するオブジェクト
    [SerializeField] private GameObject scoreRecorderGameObject;
    private ScoreRecorder scoreRecorder;

    // カートの座標を記録するオブジェクト
    [SerializeField] private GameObject positionRecorderGameObject;
    private PositionRecorder3D positionRecorder;

    void Start()
    {
        // エージェントの設定
        learningAgent = agentGameObject.GetComponent<Agent>();

        // ニューラルネットワークの設定と初期化
        nnBrain = brainGameObject.GetComponent<NNBrain>();
        nnBrain.initialize(learningAgent.StateSize, learningAgent.ActionSize);

        scoreRecorder = scoreRecorderGameObject.GetComponent<ScoreRecorder>();
        positionRecorder = positionRecorderGameObject.GetComponent<PositionRecorder3D>();

        // 初期状態の設定
        prevState = learningAgent.GetState();
        prevAction = new double[learningAgent.ActionSize];

        // テキスト更新
        UpdateText();
    }

    void FixedUpdate()
    {
        // エージェントの1ステップ
        AgentUpdate();

        positionRecorder.UpdateRecord(episodeCount, prevState);

        // エピソードの終了でエージェントをリセットし新しいエピソードの開始
        if(learningAgent.IsDone){
            bestScore = Mathf.Max(bestScore, learningAgent.Score);
            scoreRecorder.UpdateRecord(episodeCount, learningAgent.Score, bestScore);
            episodeCount++;
            learningAgent.AgentReset();
            prevState = learningAgent.GetState();
            UpdateText();
        }
    }

    // テキストの更新
    private void UpdateText(){
        episodeCountText.text = $"episode: {episodeCount}\n"
                              + $"best score: {bestScore:F2}\n";
    }

    // エージェントの1ステップ
    // エージェントの状態（座標など）の取得，NNの学習，とるアクションの決定
    private void AgentUpdate(){
        // エージェントの状態（座標など）の取得
        double[] currentState = learningAgent.GetState();

        // 前回とったアクションとその結果（現在の状態）を用いてNNを学習する
        // isLearningがfalseなら学習はしない
        if(learningAgent.IsLearning){
            nnBrain.Learn(prevAction, currentState);
        }

        // 現在の状態をNNに入力してとるアクションを決定する
        double[] action = nnBrain.GetAction(currentState);
        // Debug.Log($"{currentState[0]}, {action[0]}");
        learningAgent.AgentAction(action);

        // 状態とアクションの記録
        prevState = currentState;
        Array.Copy(action, prevAction, action.Length);
    }
}
