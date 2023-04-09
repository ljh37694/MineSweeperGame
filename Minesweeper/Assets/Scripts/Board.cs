using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class Board : MonoBehaviour {
	// 블럭 프리펩
	public GameObject[] grayBlocks = new GameObject[11], blueBlocks = new GameObject[3];

	// UI
	public GameObject finishMenu;
	public TMP_Text menuText;

	// const
	const int row = 16, column = 30, bombCount = 80;
	const float boomDelay = 0.5f; // 폭탄 폭발 딜레이

	readonly string[] text = { "Game Over", "Clear" };

	// 게임이 끝났으면 true 아니면 false (default: false)
	bool finish;

	// 게임을 클리어했으면 true, 아니면 false
	bool isClear;

	// 지뢰가 있는 칸을 제외한 남은 파란 블럭의 개수
	int safezoneCount;

	// 상좌, 상우, 하좌, 하우, 상, 하, 좌, 우
	readonly int[] dr = { -1, -1, 1, 1, -1, 1, 0, 0 }, dc = { -1, 1, -1, 1, 0, 0, -1, 1 };

	// 각 좌표에 폭탄이 있으면 true 아니면 false
	bool[,] isBomb;

	// 파란 블럭의 상태 저장
	int[,] statusBlueBlocks;

	// 각 좌표의 블럭을 저장
	GameObject[,] grayBoard, blueBoard;

	// 폭발음 사운드
	AudioSource boomSound;

	void Awake() {
		boomSound = GetComponent<AudioSource>();
	}

	void Start() {
		Init();
	}

	void Update() {
		if (!finish) {
			ClickedRight();
			ClickedLeft();

			GameClear();
		}

		else {
			if (Input.GetMouseButtonDown(0) && !finishMenu.activeSelf) {
				finishMenu.SetActive(true);

				boomSound.volume /= 3;

				isClear = true;
			}
		}
	}

	public int GetRow() { return row; }

	public int GetColumn() { return column; }

	void Init() {
		safezoneCount = row * column - bombCount;
		finishMenu.SetActive(false);
		menuText.text = text[0];
		boomSound.volume = 80;

		isBomb = new bool[row, column];
		statusBlueBlocks = new int[row, column];
		grayBoard = new GameObject[row, column];
		blueBoard = new GameObject[row, column];

		RenderGrayBlocks();
		RenderBlueBlocks();

		finish = false;
		isClear = false;
	}

	public void GameRestart() {
		RemoveAllBlueBlocks();
		RemoveAllGrayBlocks();

		Init();
	}

	public void Exit() {
		Application.Quit();
	}

	void GameClear() {
		if (safezoneCount == 0) {
			RemoveAllBlueBlocks();
			RemoveAllGrayBlocks();

			menuText.text = text[1];

			finishMenu.SetActive(true);

			finish = true;
			isClear = true;
		}
	}

	void RemoveAllBlueBlocks() {
		for (int r = 0; r < row; r++) {
			for (int c = 0; c < column; c++) {
				Destroy(blueBoard[r, c]);
			}
		}
	}

	void RemoveAllGrayBlocks() {
		for (int r = 0; r < row; r++) {
			for (int c = 0; c < column; c++) {
				Destroy(grayBoard[r, c]);
			}
		}
	}

	// 회색 블럭 렌더링
	void RenderGrayBlocks() {
		int cnt = 0;

		// bombCount개만큼의 지뢰를 랜덤한 좌표에 지정
		while (cnt < bombCount) {
			int r = Random.Range(0, row), c = Random.Range(0, column);

			if (isBomb[r, c] == false) {
				cnt++;
				isBomb[r, c] = true;
			}
		}

		// 각 좌표가 지뢰이면 지뢰를 아니면 빈 칸
		for (int r = 0; r < row; r++) {
			for (int c = 0; c < column; c++) {
				int idx = 0;

				if (isBomb[r, c]) idx = 9;

				grayBoard[r, c] = Instantiate(grayBlocks[idx], new Vector3(c, r, 0), Quaternion.identity);
			}
		}

		// 각 좌표에서 인접한 지뢰에 따라 숫자 블럭으로 바꾸기
		for (int r = 0; r < row; r++) {
			for (int c = 0; c < column; c++) {
				if (!grayBoard[r, c].CompareTag("Bomb")) {
					int idx = CountBomb(r, c);

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
	int CountBomb(int r, int c) {
		int cnt = 0;

		for (int i = 0; i < 8; i++) {
			int nr = r + dr[i], nc = c + dc[i];

			if (IsValidIdx(nr, nc) && isBomb[nr, nc]) {
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

	// 각 폭탄 좌표에 깃발 블럭이 있는 경우를 제외한 모든 파란 블럭 제거
	void RemoveBlueBlocks() {
		for (int r = 0; r < row; r++) {
			for (int c = 0; c < column; c++) {
				if (!(statusBlueBlocks[r, c] == 1 && isBomb[r, c])) {
					blueBoard[r, c].SetActive(false);
				}

				if (statusBlueBlocks[r, c] == 1 && !isBomb[r, c]) {
					statusBlueBlocks[r, c] = 0;
				}
			}
		}
	}

	// 일정 주기로 1개씩 모든 폭탄 폭발
	IEnumerator ExplosionAllBombs() {
		for (int r = 0; r < row; r++) {
			for (int c = 0; c < column; c++) {
				if (isClear) break;

				if (statusBlueBlocks[r, c] != 1 && isBomb[r, c]) {
					ExplosionBomb(r, c);

					yield return new WaitForSeconds(boomDelay);
				}
			}
		}

		finishMenu.SetActive(true);
	}

	// 폭발
	void ExplosionBomb(int r, int c) {
		Destroy(grayBoard[r, c]);

		grayBoard[r, c] = Instantiate(grayBlocks[10], new Vector2(c, r), Quaternion.identity);

		boomSound.Play();
	}

	// 좌표 구조체
	struct Pos {
		public int r, c;

		public Pos(int r, int c) { this.r = r;  this.c = c; }
	};

	// 빈칸 회색 블럭이 있는 위치의 파란 블럭을 클릭했을 때 주변 블럭을 밝힘
	void ClickedBlankGrayBlock(int r, int c) {
		Queue<Pos> q = new Queue<Pos>();
		bool[,] visit = new bool[row, column];

		q.Enqueue(new Pos(r, c));
		visit[r, c] = true;

		while (q.Count != 0) {
			Pos cur = q.Peek(); q.Dequeue();
			GameObject curBlock = grayBoard[cur.r, cur.c];

			blueBoard[cur.r, cur.c].SetActive(false);
			safezoneCount--;

			if (curBlock.CompareTag("GrayBlock")) continue;

			for (int i = 0; i < 8; i++) {
				Pos next = new Pos(cur.r + dr[i], cur.c + dc[i]);
				GameObject nextBlock;

				if (!IsValidIdx(next.r, next.c) || 
					visit[next.r, next.c]) continue;

				nextBlock = grayBoard[next.r, next.c];

				if (curBlock.CompareTag("BlankGrayBlock")) {
					q.Enqueue(next);

					visit[next.r, next.c] = true;
				}
			}
		}
	}

	/*
	클릭 이벤트
	*/

	// 좌클릭
	void ClickedLeft() {
		if (finish) return;

		if (Input.GetMouseButtonDown(0)) {
			Vector2 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
			RaycastHit2D hit = Physics2D.Raycast(mousePos, Vector2.zero);

			// 파란색 칸을 왼쪽 클릭했을 때
			if (hit.collider != null) {
				int r = (int)hit.transform.position.y, c = (int)hit.transform.position.x;
				GameObject curBlue = blueBoard[r, c], curGray = grayBoard[r, c];

				// 파란 빈칸 블록이 아니면 return
				if (!curBlue.CompareTag("BlankBlueBlock")) return;

				// 누른 칸의 위치의 회색 블록이 폭탄일 때
				if (curGray.CompareTag("Bomb")) {
					finish = true;

					RemoveBlueBlocks();
					
					StartCoroutine(ExplosionAllBombs());
				}

				else if (curGray.CompareTag("BlankGrayBlock")) {
					ClickedBlankGrayBlock(r, c);
				}

				else {
					blueBoard[r, c].SetActive(false);
					safezoneCount--;
				}
			}
		}
	}

	// 우클릭
	void ClickedRight() {
		if (finish) return;

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
}