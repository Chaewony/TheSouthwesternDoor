using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MoveCounter : MonoBehaviour
{
	[SerializeField]
	private int limitMoves; // �ִ� �̵� ���� Ƚ��
	private int leftMoves; // ���� ���� �̵� ���� Ƚ��

	public int leftMoveScore;  //���� �̵� Ƚ���� ������ ȯ�� 
	public GUIStyle guistyle; // ��Ʈ ��Ÿ��.

	private BlockRoot block_root = null;

	void Start()
	{
		this.block_root = this.gameObject.GetComponent<BlockRoot>();
		this.leftMoves = this.limitMoves;
		this.guistyle.fontSize = 25;
	}

	void OnGUI()
	{
		int x = 10;
		int y = 330;
		GUI.color = Color.black;
		this.print_value(x + 20, y, "���� �̵� Ƚ��", this.leftMoves);
	}

	public void print_value(int x, int y, string label, int value)
	{
		// label�� ǥ��.
		GUI.Label(new Rect(x, y, 100, 20), label, guistyle);
		y += 25;
		// ���� �࿡ value�� ǥ��.
		GUI.Label(new Rect(x + 20, y, 100, 20), value.ToString(), guistyle);
		y += 25;
	}
	public void minusLeftMoves()
	{
		this.leftMoves--;
	}
	public void plusLeftMoves()
	{
		this.leftMoves++;
	}
	public int getLeftMoves() //���� �̵� ���� Ƚ���� ����
	{
		return this.leftMoves;
	}
	public int leftMoveCount()                            //���� �̵� Ƚ���� 10�� ���ؼ� ������ ����.
    {
		leftMoveScore = leftMoves * 10;
		return leftMoveScore;
    }
	public bool isLeftMovesZero()
	{
		
		if (this.leftMoves > 0)
			return false;
		// ���� ���� �̵� Ƚ���� 0�̸� 
		return true;
	}
}
