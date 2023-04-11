using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EscapeWindow : MonoBehaviour {
	public GameObject escapeMenu;

	void Update() {
		if (Input.GetKeyDown(KeyCode.Escape)) {
			PressEscape();
		}
	}

	public void GameExit() {
		Application.Quit();
	}

	public void PressEscape() {
		escapeMenu.SetActive(!escapeMenu.activeSelf);
	}
}
