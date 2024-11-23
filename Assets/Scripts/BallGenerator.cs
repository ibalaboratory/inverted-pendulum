using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// 左クリックでボールを生成
public class BallGenerator : MonoBehaviour
{
    [SerializeField] private GameObject ballPrefab; // 生成するボールのPrefab
    [SerializeField] private float NormForce = 500; // ボールに加える力の大きさ

    private Vector3 cameraPosition; // ボールの発射位置としてカメラの位置を利用

    void Start(){
        cameraPosition = this.transform.position;
    }

    void Update()
    {
        if(Input.GetMouseButtonDown(0)){ // 左クリックしたそのフレーム間だけtrue
            GameObject ball = Instantiate(ballPrefab) as GameObject; // Prefabをもとにボールのインスタンスを作成 

            // カメラの位置を初期位置として, 作成したインスタンスに対し, クリック位置方向に力を加える
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            Vector3 dir = ray.direction;
            ball.transform.position = cameraPosition;
            ball.GetComponent<Rigidbody>().AddForce(dir.normalized * NormForce);
        }
    }
}
