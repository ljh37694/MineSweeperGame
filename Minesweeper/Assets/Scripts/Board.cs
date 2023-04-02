using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting.Antlr3.Runtime.Tree;
using UnityEngine;
using UnityEngine.UIElements;

public class Board : MonoBehaviour
{
	// public
	public GameObject[] grayBlocks = new GameObject[10];
	public GameObject[] blueBlocks = new GameObject[3];

	// private
	const int row = 33, column = 21, boobCount = 50;
	const int rowStart = -(row / 2), rowEnd = row / 2, colStart = -(column / 2), colEnd = column / 2;
	GameObject[,] grayBoard = new GameObject[row, column], blueBoard = new GameObject[row, column];

    void Start() {
		RenderGrayBlocks();
		RenderBlueBlocks();
    }

	void RenderGrayBlocks() {
		int cnt = 0;
		bool[,] isBoob = new bool[row, column]; // ��� ��ǥ�� ��������

		// boobCount����ŭ�� ���ڸ� ������ ��ǥ�� ����
		while (cnt <= boobCount) {
			int r = Random.Range(0, row), c = Random.Range(0, column);

			if (isBoob[r, c] == false) {
				cnt++;
				isBoob[r, c] = true;
			}
		}

		// �� ��ǥ�� �����̸� ���ڸ� �ƴϸ� �� ĭ
		for (int i = rowStart; i <= rowEnd; i++) {
			for (int j = colStart; j <= colEnd; j++) {
				int idx = 0, r = i + row / 2, c = j + column / 2;

				if (isBoob[r, c]) idx = 9;

				grayBoard[r, c] = Instantiate(grayBlocks[idx], new Vector3(i, j, 0), Quaternion.identity);
			}
		}

		// �� ��ǥ���� ������ ���ڿ� ���� ���� ������ �ٲٱ�
		for (int i = 0; i < row; i++) {
			for (int j = 0; j < column; j++) {
				if (!grayBoard[i, j].CompareTag("Boob")) {
					int idx = CountBoob(i, j);

					Destroy(grayBoard[i, j]);

					grayBoard[i, j] = Instantiate(grayBlocks[idx], new Vector3(i - (row / 2), j - (column / 2), 0), Quaternion.identity);
				}
			}
		}
	}

	// ��ȿ�� ��ǥ���� Ȯ��
	bool IsValidIdx(int r, int c) {
		return (0 <= r && r < row) && (0 <= c && c < column);
	}

	// (c,r)�� ������ ���� ���� return
	int CountBoob(int r, int c) {
		int cnt = 0;

		// ����, ���, ����, �Ͽ�, ��, ��, ��, ��
		int[] dy = { -1, -1, 1, 1, -1, 1, 0, 0 }, dx = { -1, 1, -1, 1, 0, 0, -1, 1 };

		for (int i = 0; i < 8; i++) {
			int nr = r + dy[i], nc = c + dx[i];

			if (IsValidIdx(nr, nc) && grayBoard[nr, nc].CompareTag("Boob")) {
				cnt++;
			}
		}

		return cnt;
	}

	void RenderBlueBlocks() {
		for (int i = rowStart; i <= rowEnd; i++) {
			for (int j = colStart; j <= colEnd; j++) {
				int r = i + row / 2, c = j + column / 2;

				blueBoard[r, c] = Instantiate(blueBlocks[0], new Vector3(i, j, 0), Quaternion.identity);
			}
		}
	}
}
