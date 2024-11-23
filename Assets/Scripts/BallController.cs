using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// 時間経過でボールを表示, 破壊
public class BallController : MonoBehaviour
{
    [SerializeField] private int MaxFrame = 100;    // このフレーム数を超えたらボールを破壊
    [SerializeField] private int visibleFrame = 10; // このフレーム数まではボールを非表示
    private int frameCount;

    void Start(){
        frameCount = 0;
        this.GetComponentInChildren<MeshRenderer>().enabled = false; // 最初は非表示(Cameraに近すぎて見づらくなるため)
    }

    void FixedUpdate(){
        frameCount ++ ;
        // visibleFrameを超えたら表示
        if(frameCount > visibleFrame){
            this.GetComponentInChildren<MeshRenderer>().enabled = true;
        }

        // MaxFrameを超えたら破壊(処理を軽くするため)
        if(frameCount > MaxFrame){
            Destroy(this.gameObject);
        }
    }
}
