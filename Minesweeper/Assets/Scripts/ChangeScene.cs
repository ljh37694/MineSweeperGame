using UnityEngine;
using UnityEngine.SceneManagement;

public class ChangeScene : MonoBehaviour {
	void Update() {
		if (SceneManager.GetActiveScene().name == "LevelScene" && Input.GetKeyDown(KeyCode.Escape)) {
			LoadStartScene();
		}
	}

	public void LoadStartScene() {
		SceneManager.LoadScene("IntroScene");
	}

	public void LoadLevelScene() {
		SceneManager.LoadScene("LevelScene");
	}

	public void LoadGameScene() {
		SceneManager.LoadScene("GameScene");
	}
}
