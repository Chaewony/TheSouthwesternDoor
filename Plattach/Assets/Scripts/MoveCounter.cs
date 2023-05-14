using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MoveCounter : MonoBehaviour
{
	[SerializeField]
	private int limitMoves; // �ִ� �̵� ���� Ƚ��
	private int leftMoves; // ���� ���� �̵� ���� Ƚ��

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
	}

	void OnGUI()
	{
		int x = 20;
		int y = 200;
		GUI.color = Color.black;
		this.print_value(x + 20, y, "���� �̵� Ƚ��", this.leftMoves);
	}

	public void print_value(int x, int y, string label, int value)
	{
		// label�� ǥ��.
		GUI.Label(new Rect(x, y, 100, 20), label, guistyle);
		y += 15;
		// ���� �࿡ value�� ǥ��.
		GUI.Label(new Rect(x + 20, y, 100, 20), value.ToString(), guistyle);
		y += 15;
	}
	public void minusLeftMoves()
	{
		this.leftMoves--;
		moves++;
	}
	public void plusLeftMoves()
	{
		this.leftMoves++;
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
