using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.Mathematics;
using Unity.VisualScripting;
using Unity.VisualScripting.Antlr3.Runtime.Tree;
using UnityEngine;
using UnityEngine.Assertions.Must;
using UnityEngine.UIElements;

public class Board : MonoBehaviour {
	// const
	const int row = 16, column = 30, bombCount = 10;
	const float boomDelay = 0.5f; // ��ź ���� ������

	// ������ �������� true �ƴϸ� false (default: false)
	bool finish;

	// ���ڰ� �ִ� ĭ�� ������ ���� �Ķ� ���� ����
	int safezoneCount;

	// ����, ���, ����, �Ͽ�, ��, ��, ��, ��
	readonly int[] dr = { -1, -1, 1, 1, -1, 1, 0, 0 }, dc = { -1, 1, -1, 1, 0, 0, -1, 1 };

	// �� ������
	public GameObject[] grayBlocks = new GameObject[11], blueBlocks = new GameObject[3];

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
	}

	void Init() {
		finish = false;
		safezoneCount = row * column - bombCount;

		isBomb = new bool[row, column];
		statusBlueBlocks = new int[row, column];
		grayBoard = new GameObject[row, column];
		blueBoard = new GameObject[row, column];

		RenderGrayBlocks();
		RenderBlueBlocks();
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

	public void GameStart() {
		Init();
	}

	void GameClear() {
		if (safezoneCount == 0) {
			finish = true;

			RemoveAllBlueBlocks();
			RemoveAllGrayBlocks();

			Invoke("Init", 3);

			Debug.Log("Game Clear");
		}
	}

	// ȸ�� �� ������
	void RenderGrayBlocks() {
		int cnt = 0;

		// bombCount����ŭ�� ���ڸ� ������ ��ǥ�� ����
		while (cnt < bombCount) {
			int r = UnityEngine.Random.Range(0, row), c = UnityEngine.Random.Range(0, column);

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
					Destroy(blueBoard[r, c]);
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
				if (statusBlueBlocks[r, c] != 1 && isBomb[r, c]) {
					ExplosionBomb(r, c);

					yield return new WaitForSeconds(boomDelay);
				}
			}
		}
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

			Destroy(blueBoard[cur.r, cur.c]);
			safezoneCount--;

			Debug.Log(safezoneCount);

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

	// Ŭ�� �̺�Ʈ

	// ��Ŭ��
	void ClickedLeft() {
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
					RemoveBlueBlocks();
					
					StartCoroutine(ExplosionAllBombs());
				}

				else if (curGray.CompareTag("BlankGrayBlock")) {
					ClickedBlankGrayBlock(r, c);
				}

				else {
					Destroy(blueBoard[r, c]);
					safezoneCount--;

					Debug.Log(safezoneCount);
				}
			}
		}
	}

	// ��Ŭ��
	void ClickedRight() {
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