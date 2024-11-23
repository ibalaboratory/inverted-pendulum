using System;
using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;

// 振子の位置をcsvファイルに記録するためのclass
// episode数, x, y, z
public class PositionRecorder3D : MonoBehaviour
{
    [SerializeField] private bool recordPosition = false;
    [SerializeField] private string positionDir = "Assets/Records/Position";    // positionを記録するdirectory
    private string positionPath;                                        // csvファイルのパス

    // episodeCount番目のepisode後に呼び出し, 記録を更新する
    public void UpdateRecord(int episodeCount, double[] state) {
        if (!recordPosition) return;

        // 初めて位置を記録する場合, ファイルを作成しheaderを書きこむ
        if (positionPath == null) {
            if (!Directory.Exists(positionDir)) {
            throw new DirectoryNotFoundException("The directory does not exist.");
            } else {
                SetPositionPath(positionDir);
                using (StreamWriter sw = new StreamWriter(positionPath)) {
                    sw.WriteLine("Episodes,X,Y,Z");   // headerを書きこむ
                }
            }
        }

        // 更新内容を追加書き込みする
        using (StreamWriter sw = new StreamWriter(positionPath, /* appendするか */ true)) {
            sw.WriteLine($"{episodeCount},{state[0]},{state[1]},{state[2]}");
        }
    }

    // パスを生成しpositionPathに代入
    private void SetPositionPath(string positionDir) {
        string sceneName = SceneManager.GetActiveScene().name; // 開いているシーン名
        string time = DateTime.Now.ToString("yyyyMMddHHmmss");  // 現在の時刻
        string fileName = sceneName + "_" + time + ".csv";      // eg. Stage1_20220412123456.csv
        positionPath = Path.Combine(positionDir, fileName);
    }
}