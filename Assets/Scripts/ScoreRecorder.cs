using System;
using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;

// score(振子が立ち続けた時間[s])をcsvファイルに記録するためのclass
// episode数, episodeのscore, 全episodeのbest score
public class ScoreRecorder : MonoBehaviour
{
    [SerializeField] private bool recordScore = false;
    [SerializeField] private string scoreDir = "Assets/Records/Score/"; // scoreを記録するdirectory
    private string scorePath; // csvファイルのパス

    // episodeCount番目のepisode後に呼び出し, 記録を更新する
    public void UpdateRecord(int episodeCount, float episodeScore, float bestScore) {
        if (!recordScore) return;

        // 初めてscoreを記録する場合, ファイルを作成しheaderを書きこむ
        if (scorePath == null) {
            if (!Directory.Exists(scoreDir)) {
                throw new DirectoryNotFoundException("The directory does not exist.");
            } else {
                SetScorePath(scoreDir);
                using (StreamWriter sw = new StreamWriter(scorePath)) {
                    sw.WriteLine("Episodes,EpisodeScore,BestScore");   // headerを書きこむ
                }
            }
        }

        // 更新内容を追加書きこみする
        using (StreamWriter sw = new StreamWriter(scorePath, /* appendするか */ true)) {
            sw.WriteLine($"{episodeCount},{episodeScore},{bestScore}");
        }
    }

    // パスを生成しscorePathに代入
    private void SetScorePath(string scoreDir) {
        string sceneName = SceneManager.GetActiveScene().name;  // 開いているシーン名
        string time = DateTime.Now.ToString("yyyyMMddHHmmss");  // 現在の時刻
        string fileName = sceneName + "_" + time + ".csv";      // eg. Stage1_20220412123456.csv
        scorePath = Path.Combine(scoreDir, fileName);
    }
}