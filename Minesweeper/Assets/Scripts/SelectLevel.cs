using UnityEngine;
using UnityEngine.UI;

public class SelectLevel : MonoBehaviour {
	public Button easyBtn, usualBtn, hardBtn;

	void Start() {
		easyBtn.onClick.AddListener(() => SetBoard(9, 9, 10));

		usualBtn.onClick.AddListener(() => SetBoard(16, 16, 40));

		hardBtn.onClick.AddListener(() => SetBoard(16, 30, 99));
	}

	void SetBoard(int row, int column, int bombCount) {
		Board.row = row;
		Board.column = column;
		Board.bombCount = bombCount;
	}
}
