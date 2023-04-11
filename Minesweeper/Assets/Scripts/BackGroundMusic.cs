using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

public class BackGroundMusic : MonoBehaviour {
	BackGroundMusic[] backgroundMusic;

	void Awake() {
		backgroundMusic = FindObjectsOfType<BackGroundMusic>();

		if (backgroundMusic.Length == 1) {
			DontDestroyOnLoad(gameObject);
		}

		else {
			Destroy(gameObject);
		}
	}
}
