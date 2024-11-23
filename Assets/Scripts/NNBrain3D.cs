using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Const;

public class NNBrain3D : NNBrain
{
    // 教師信号の生成に使うパラメータ
    [SerializeField] private double trainParamK1 = 1.0;
    [SerializeField] private double trainParamK2 = 0.1;
    [SerializeField] private double trainParamK3 = 0.05;

    // 事前学習
    // x, z方向は棒の倒れている向きに, y方向はy座標が0となる向きに
    // カートを押すようにランダムな入力を用いて学習する
    public override void PreTrain(){
        for(int i = 0; i < preTrainNum; i++){
            double[] inputs = new double[10];
            for(int j = 0; j < 10; j++) inputs[j] = Random.Range(-1.0f, 1.0f);
            // 値を[-0.5, 0.5]の範囲に制限(あまり大きな力を加えると棒が倒れるため)
            double[] outputs = new double[3] {
                Mathf.Clamp((float)( inputs[CO.StateAngX]), -0.5f, 0.5f),
                Mathf.Clamp((float)(-inputs[CO.StateY])   , -0.5f, 0.5f),
                Mathf.Clamp((float)( inputs[CO.StateAngZ]), -0.5f, 0.5f)
            };
            nn.BackPropagate(inputs, outputs);
        }
    }

    // 教師信号の生成
    // アクションとその結果の状態から，とるべきだったアクションを返す
    // 入力：直前のアクションと現在のCartPoleの状態
    // 出力：教師信号
    public override double[] MakeTrainData(double[] prevAction, double[] state){
        return TrainDataFunc3(prevAction, state);
    }

    // 棒の角度が垂直になるようにする
    private double[] TrainDataFunc1(double[] prevAction, double[] state){
        // 角度
        double targetAngleX = 0.0;
        double targetAngleZ = 0.0;
        double errorAngleX = state[CO.StateAngX] - targetAngleX;
        double errorAngleZ = state[CO.StateAngZ] - targetAngleZ;

        // 教師信号の生成
        double[] trainData = new double[3];
        trainData[CO.ACTION_X] = prevAction[CO.ACTION_X] + trainParamK1 * errorAngleX;
        trainData[CO.ACTION_Y] = prevAction[CO.ACTION_Y];
        trainData[CO.ACTION_Z] = prevAction[CO.ACTION_Z] + trainParamK1 * errorAngleZ;
        trainData[CO.ACTION_X] = Mathf.Clamp((float)trainData[CO.ACTION_X], -1.0f, 1.0f);
        trainData[CO.ACTION_Y] = Mathf.Clamp((float)trainData[CO.ACTION_Y], -1.0f, 1.0f);
        trainData[CO.ACTION_Z] = Mathf.Clamp((float)trainData[CO.ACTION_Z], -1.0f, 1.0f);
        return trainData;
    }

    // 棒の角度が垂直, カートの位置が0になるようにする
    private double[] TrainDataFunc2(double[] prevAction, double[] state){
        // 角度
        double targetAngleX = 0.0;
        double targetAngleZ = 0.0;
        double errorAngleX = state[CO.StateAngX] - targetAngleX;
        double errorAngleZ = state[CO.StateAngZ] - targetAngleZ;

        // 位置
        double targetX = 0.0;
        double targetY = 0.0;
        double targetZ = 0.0;
        double errorX = Mathf.Clamp((float)(state[CO.StateX] - targetX), -1.0f, 1.0f);
        double errorY = Mathf.Clamp((float)(state[CO.StateY] - targetY), -1.0f, 1.0f);
        double errorZ = Mathf.Clamp((float)(state[CO.StateZ] - targetZ), -1.0f, 1.0f);

        // 教師信号の生成
        double[] trainData = new double[3];
        trainData[CO.ACTION_X] = prevAction[CO.ACTION_X] + trainParamK1 * errorAngleX + trainParamK2 * errorX;
        trainData[CO.ACTION_Y] = prevAction[CO.ACTION_Y]                              - trainParamK2 * errorY;
        trainData[CO.ACTION_Z] = prevAction[CO.ACTION_Z] + trainParamK1 * errorAngleZ + trainParamK2 * errorZ;
        trainData[CO.ACTION_X] = Mathf.Clamp((float)trainData[CO.ACTION_X], -1.0f, 1.0f);
        trainData[CO.ACTION_Y] = Mathf.Clamp((float)trainData[CO.ACTION_Y], -1.0f, 1.0f);
        trainData[CO.ACTION_Z] = Mathf.Clamp((float)trainData[CO.ACTION_Z], -1.0f, 1.0f);
        return trainData;
    }

    // 棒の角度が垂直, カートの座標と速度が0になるようにする
    private double[] TrainDataFunc3(double[] prevAction, double[] state){
        // 角度
        double targetAngleX = 0.0;
        double targetAngleZ = 0.0;
        double errorAngleX = state[CO.StateAngX] - targetAngleX;
        double errorAngleZ = state[CO.StateAngZ] - targetAngleZ;

        // 位置
        double targetX = 0.0;
        double targetY = 0.0;
        double targetZ = 0.0;
        double errorX = Mathf.Clamp((float)(state[CO.StateX] - targetX), -1.0f, 1.0f);
        double errorY = Mathf.Clamp((float)(state[CO.StateY] - targetY), -1.0f, 1.0f);
        double errorZ = Mathf.Clamp((float)(state[CO.StateZ] - targetZ), -1.0f, 1.0f);

        // 速度
        double targetVX = 0.0;
        double targetVY = 0.0;
        double targetVZ = 0.0;
        double errorVX = Mathf.Clamp((float)(state[CO.StateVelX] - targetVX), -1.0f, 1.0f);
        double errorVY = Mathf.Clamp((float)(state[CO.StateVelY] - targetVY), -1.0f, 1.0f);
        double errorVZ = Mathf.Clamp((float)(state[CO.StateVelZ] - targetVZ), -1.0f, 1.0f);

        // 教師信号の生成
        double[] trainData = new double[3];
        trainData[CO.ACTION_X] = prevAction[CO.ACTION_X] + trainParamK1 * errorAngleX + trainParamK2 * errorX;
        trainData[CO.ACTION_Y] = prevAction[CO.ACTION_Y]                              - trainParamK2 * errorY - trainParamK3 * errorVY;
        trainData[CO.ACTION_Z] = prevAction[CO.ACTION_Z] + trainParamK1 * errorAngleZ + trainParamK2 * errorZ;
        trainData[CO.ACTION_X] = Mathf.Clamp((float)trainData[CO.ACTION_X], -1.0f, 1.0f);
        trainData[CO.ACTION_Y] = Mathf.Clamp((float)trainData[CO.ACTION_Y], -1.0f, 1.0f);
        trainData[CO.ACTION_Z] = Mathf.Clamp((float)trainData[CO.ACTION_Z], -1.0f, 1.0f);
        return trainData;
    }
}