using UnityEngine;

public class CameraController : MonoBehaviour {
	void Start() {
		Vector3 v = new Vector3(Board.column / 2f - 0.5f, Board.row / 2f, -10);

		transform.position = v;
	}
}
