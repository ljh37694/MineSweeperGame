using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.VFX;

public class CameraController : MonoBehaviour {
	public Board board;

	void Start() {
		Vector3 v = new Vector3(board.GetColumn() / 2f - 0.5f, board.GetRow() / 2f, -10);

		transform.position = v;
	}
}
