using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Cartの動作を操作するコントローラー
public class CartController3D : MonoBehaviour
{
    // Cartに加えることのできる最大の力
    [SerializeField] private double maxForce = 1.0;

    // 入力. Cartに加える力. [-1.0, 1.0]
    // x, y, z軸方向
    public double cartInputX { get; set; } = 0.0;
    public double cartInputY { get; set; } = 0.0;
    public double cartInputZ { get; set; } = 0.0;

    // 操作するCartのオブジェクト
    private Rigidbody cartRb;

    void Start(){
        // 操作するCartオブジェクトの設定
        cartRb = GetComponent<Rigidbody>();
    }

    // 入力を[-1.0, 1.0]の範囲にする
    void clipInput(){
        cartInputX = Mathf.Clamp((float)cartInputX, -1.0f, 1.0f);
        cartInputY = Mathf.Clamp((float)cartInputY, -1.0f, 1.0f);
        cartInputZ = Mathf.Clamp((float)cartInputZ, -1.0f, 1.0f);
        return;
    }

    void FixedUpdate()
    {
        clipInput();
        // 外部入力に合わせてCartに力を加える
        Vector3 force = new Vector3 ((float)(maxForce * cartInputX), (float)(maxForce * cartInputY), (float)(maxForce * cartInputZ));
        cartRb.AddForce(force, ForceMode.Impulse);
    }
}
