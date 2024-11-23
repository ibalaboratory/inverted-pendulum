using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

// 抽象的なエージェントクラス．
public abstract class Agent : MonoBehaviour
{
    // 終了したか
    public bool IsDone { get; set; }

    // エージェントのスコア．例えば，CartPoleでは倒れずにいられた時間
    public float Score { get; set; }

    // エージェントの状態のサイズ
    public int StateSize { get; set; }

    // エージェントに入力できるアクションのサイズ
    public int ActionSize { get; set; }

    // 学習を行うかどうか
    [SerializeField] private bool isLearning = true;
    public bool IsLearning { get { return isLearning; } }

    // エージェントの状態を初期状態に戻す
    public abstract void AgentReset();

    // 現在のエージェントの状態を返す
    public abstract double[] GetState();

    // エージェントに入力に応じた行動をさせる
    public abstract void AgentAction(double[] action);
}