using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public abstract class NNBrain : MonoBehaviour
{
    // ニューラルネットワーク
    protected NN nn;
    private int inputSize;
    private int outputSize;
    [SerializeField] private int hiddenSize = 40;
    [SerializeField] private int hiddenLayers = 1;

    // ニューラルネットワークの学習率
    [SerializeField] private double learningRate = 0.1;

    //　実際にエージェントを動かす前の事前学習の有無
    [SerializeField] private bool enablePreTrain = true;

    // 事前学習に使うデータ数
    [SerializeField] protected int preTrainNum = 100000;

    // ニューラルネットの初期化(と事前学習)
    public void initialize(int stateSize, int actionSize){
        inputSize = stateSize;
        outputSize = actionSize;
        nn = new NN(inputSize, hiddenSize, hiddenLayers, outputSize);
        nn.LearningRate = learningRate;

        if(enablePreTrain) PreTrain();
    }

    // ニューラルネットの事前学習
    public abstract void PreTrain();

    // ニューラルネットの学習
    public void Learn(double[] prevAction, double[] state){
        double[] trainData = MakeTrainData(prevAction, state);
        nn.BackPropagate(trainData);
    }

    // 教師信号の生成
    public abstract double[] MakeTrainData(double[] prevAction, double[] state);

    // ニューラルネットにエージェントの状態を入力し，とるべきアクションを決定する
    public double[] GetAction(double[] inputs){
        return nn.Predict(inputs);
    }
}