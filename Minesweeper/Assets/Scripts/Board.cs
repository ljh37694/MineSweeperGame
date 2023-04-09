using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class Board : MonoBehaviour {
	// �� ������
	public GameObject[] grayBlocks = new GameObject[11], blueBlocks = new GameObject[3];

	// UI
	public GameObject finishMenu;
	public TMP_Text menuText;

	// const
	const int row = 16, column = 30, bombCount = 80;
	const float boomDelay = 0.5f; // ��ź ���� ������

	readonly string[] text = { "Game Over", "Clear" };

	// ������ �������� true �ƴϸ� false (default: false)
	bool finish;

	// ������ Ŭ���������� true, �ƴϸ� false
	bool isClear;

	// ���ڰ� �ִ� ĭ�� ������ ���� �Ķ� ���� ����
	int safezoneCount;

	// ����, ���, ����, �Ͽ�, ��, ��, ��, ��
	readonly int[] dr = { -1, -1, 1, 1, -1, 1, 0, 0 }, dc = { -1, 1, -1, 1, 0, 0, -1, 1 };

	// �� ��ǥ�� ��ź�� ������ true �ƴϸ� false
	bool[,] isBomb;

	// �Ķ� ���� ���� ����
	int[,] statusBlueBlocks;

	// �� ��ǥ�� ���� ����
	GameObject[,] grayBoard, blueBoard;

	// ������ ����
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

	// ȸ�� �� ������
	void RenderGrayBlocks() {
		int cnt = 0;

		// bombCount����ŭ�� ���ڸ� ������ ��ǥ�� ����
		while (cnt < bombCount) {
			int r = Random.Range(0, row), c = Random.Range(0, column);

			if (isBomb[r, c] == false) {
				cnt++;
				isBomb[r, c] = true;
			}
		}

		// �� ��ǥ�� �����̸� ���ڸ� �ƴϸ� �� ĭ
		for (int r = 0; r < row; r++) {
			for (int c = 0; c < column; c++) {
				int idx = 0;

				if (isBomb[r, c]) idx = 9;

				grayBoard[r, c] = Instantiate(grayBlocks[idx], new Vector3(c, r, 0), Quaternion.identity);
			}
		}

		// �� ��ǥ���� ������ ���ڿ� ���� ���� ������ �ٲٱ�
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

	// ��ȿ�� ��ǥ���� Ȯ��
	bool IsValidIdx(int r, int c) {
		return (0 <= r && r < row) && (0 <= c && c < column);
	}

	// (c,r)�� ������ ���� ���� return
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

	// �Ķ� ��� ������
	void RenderBlueBlocks() {
		for (int r = 0; r < row; r++) {
			for (int c = 0; c < column; c++) {
				blueBoard[r, c] = Instantiate(blueBlocks[0], new Vector3(c, r), Quaternion.identity);
			}
		}
	}

	// �� ��ź ��ǥ�� ��� ���� �ִ� ��츦 ������ ��� �Ķ� �� ����
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

	// ���� �ֱ�� 1���� ��� ��ź ����
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

	// ����
	void ExplosionBomb(int r, int c) {
		Destroy(grayBoard[r, c]);

		grayBoard[r, c] = Instantiate(grayBlocks[10], new Vector2(c, r), Quaternion.identity);

		boomSound.Play();
	}

	// ��ǥ ����ü
	struct Pos {
		public int r, c;

		public Pos(int r, int c) { this.r = r;  this.c = c; }
	};

	// ��ĭ ȸ�� ���� �ִ� ��ġ�� �Ķ� ���� Ŭ������ �� �ֺ� ���� ����
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
	Ŭ�� �̺�Ʈ
	*/

	// ��Ŭ��
	void ClickedLeft() {
		if (finish) return;

		if (Input.GetMouseButtonDown(0)) {
			Vector2 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
			RaycastHit2D hit = Physics2D.Raycast(mousePos, Vector2.zero);

			// �Ķ��� ĭ�� ���� Ŭ������ ��
			if (hit.collider != null) {
				int r = (int)hit.transform.position.y, c = (int)hit.transform.position.x;
				GameObject curBlue = blueBoard[r, c], curGray = grayBoard[r, c];

				// �Ķ� ��ĭ ����� �ƴϸ� return
				if (!curBlue.CompareTag("BlankBlueBlock")) return;

				// ���� ĭ�� ��ġ�� ȸ�� ����� ��ź�� ��
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

	// ��Ŭ��
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