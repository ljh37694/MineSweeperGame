using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting.Antlr3.Runtime.Tree;
using UnityEngine;
using UnityEngine.UIElements;

public class Board : MonoBehaviour {
	// const
	public const int row = 16, column = 30, boobCount = 99;

	// static
	public static Board instance;

	// public
	public GameObject[] grayBlocks = new GameObject[10], blueBlocks = new GameObject[3];
	public int[,] statusBlueBlocks = new int[row, column];
	bool[,] isBoob = new bool[row, column];

	// private
	public GameObject[,] grayBoard = new GameObject[row, column], blueBoard = new GameObject[row, column];

	void Start() {
		instance = this;
		RenderGrayBlocks();
		RenderBlueBlocks();

		//GameObject tmp1 =  Instantiate(blueBlocks[0], new Vector2(0, 0), Quaternion.identity);
		//GameObject tmp2 = Instantiate(blueBlocks[1], new Vector2(2, 2), Quaternion.identity);
	}

	void Update() {
		ClickedMouseRightButtonOnBlueBlock();
		ClickedMouseLeftButtonOnBlueBlock();
	}

	void RenderGrayBlocks() {
		int cnt = 0;

		// boobCount개만큼의 지뢰를 랜덤한 좌표에 지정
		while (cnt <= boobCount) {
			int r = Random.Range(0, row), c = Random.Range(0, column);

			if (isBoob[r, c] == false) {
				cnt++;
				isBoob[r, c] = true;
			}
		}

		// 각 좌표가 지뢰이면 지뢰를 아니면 빈 칸
		for (int r = 0; r < row; r++) {
			for (int c = 0; c < column; c++) {
				int idx = 0;

				if (isBoob[r, c]) idx = 9;

				grayBoard[r, c] = Instantiate(grayBlocks[idx], new Vector3(c, r, 0), Quaternion.identity);
			}
		}

		// 각 좌표에서 인접한 지뢰에 따라 숫자 블럭으로 바꾸기
		for (int r = 0; r < row; r++) {
			for (int c = 0; c < column; c++) {
				if (!grayBoard[r, c].CompareTag("Boob")) {
					int idx = CountBoob(r, c);

					Destroy(grayBoard[r, c]);

					grayBoard[r, c] = Instantiate(grayBlocks[idx], new Vector3(c, r), Quaternion.identity);
				}
			}
		}
	}

	// 유효한 좌표인지 확인
	bool IsValidIdx(int r, int c) {
		return (0 <= r && r < row) && (0 <= c && c < column);
	}

	// (c,r)와 인접한 지뢰 개수 return
	int CountBoob(int r, int c) {
		int cnt = 0;

		// 상좌, 상우, 하좌, 하우, 상, 하, 좌, 우
		int[] dy = { -1, -1, 1, 1, -1, 1, 0, 0 }, dx = { -1, 1, -1, 1, 0, 0, -1, 1 };

		for (int i = 0; i < 8; i++) {
			int nr = r + dy[i], nc = c + dx[i];

			if (IsValidIdx(nr, nc) && isBoob[nr, nc]) {
				cnt++;
			}
		}

		return cnt;
	}

	// 파란 블록 렌더링
	void RenderBlueBlocks() {
		for (int r = 0; r < row; r++) {
			for (int c = 0; c < column; c++) {
				blueBoard[r, c] = Instantiate(blueBlocks[0], new Vector3(c, r), Quaternion.identity);
			}
		}
	}

	// 우클릭
	void ClickedMouseRightButtonOnBlueBlock() {
		if (Input.GetMouseButtonDown(1)) {
			Vector2 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
			RaycastHit2D hit = Physics2D.Raycast(mousePos, Vector2.zero);

			if (hit.collider != null) {
				int r = (int)hit.transform.position.y, c = (int)hit.transform.position.x;

				statusBlueBlocks[r, c] = (statusBlueBlocks[r, c] + 1) % 3;
				Destroy(blueBoard[r, c]);

				blueBoard[r, c] = Instantiate(blueBlocks[statusBlueBlocks[r, c]], new Vector3(c, r, 0), Quaternion.identity);
			}
		}
	}

	void ClickedMouseLeftButtonOnBlueBlock() {
		if (Input.GetMouseButtonDown(0)) {
			Vector2 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
			RaycastHit2D hit = Physics2D.Raycast(mousePos, Vector2.zero);

			if (hit.collider != null) {
				int r = (int)hit.transform.position.y, c = (int)hit.transform.position.x;

				if (!blueBoard[r, c].CompareTag("BlankBlueBlock")) return;

				Destroy(blueBoard[r, c]);
			}
		}
	}
}