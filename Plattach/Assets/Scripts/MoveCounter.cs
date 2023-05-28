using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MoveCounter : MonoBehaviour
{
	[SerializeField]
	private int limitMoves; // �ִ� �̵� ���� Ƚ��
	public int leftMoves; // ���� ���� �̵� ���� Ƚ��

	public GUIStyle guistyle; // ��Ʈ ��Ÿ��.

	private BlockRoot block_root = null;
	public GameObject scoreManagerObject;
	private ScoreManager scoreManager;
	private int moves;
	void Start()
	{
		scoreManagerObject = GameObject.Find("ScoreManager");
		this.scoreManager = scoreManagerObject.GetComponent<ScoreManager>();
		this.block_root = this.gameObject.GetComponent<BlockRoot>();
		this.leftMoves = this.limitMoves + this.scoreManager.GetCurrentMoves();
		moves = 0;
		this.guistyle.fontSize = 30;
	}

	void OnGUI()
	{
		this.guistyle.fontSize = 30;
		int x = 0;
		int y = 390;
		GUI.color = Color.red;
		this.print_value(x + 140, y, "���� �̵� Ƚ��: ", this.leftMoves);
	}

	public void print_value(int x, int y, string label, int value)
	{
		// label�� ǥ��.
		GUI.Label(new Rect(x, y, 100, 20), label, guistyle);
		//y += 30;
		// ���� �࿡ value�� ǥ��.
		GUI.Label(new Rect(x + 170, y, 100, 20), value.ToString(), guistyle);
		y += 30;
	}
	public void minusLeftMoves()
	{
		this.leftMoves--;
		moves++;
	}
	public void plusLeftMoves(int plusMove)
	{
		this.leftMoves += plusMove;
	}
	public int getLeftMoves() //���� �̵� ���� Ƚ���� ����
	{
		return this.leftMoves;
	}
	public int getMoves() //���� ���忡�� �� �Լ��� ����� ���� ������ �� �̵��� Ƚ���� ����
	{
		return moves;
	}

	public bool isLeftMovesZero()
	{
		
		if (this.leftMoves > 0)
			return false;
		// ���� ���� �̵� Ƚ���� 0�̸� 
		return true;
	}
}
