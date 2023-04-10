using UnityEngine;
using UnityEngine.SceneManagement;

public class ChangeScene : MonoBehaviour {
	public void LoadLevelScene() {
		SceneManager.LoadScene("LevelScene");
	}

	public void LoadGameScene() {
		SceneManager.LoadScene("GameScene");
	}
}
