using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TargetCounter : MonoBehaviour
{
	[SerializeField]
	private int InitYarn; // ���� �н� ����
	private int leftYarn; // ���� ���� �̵� ���� Ƚ��

	public GUIStyle guistyle; // ��Ʈ ��Ÿ��.

	private BlockRoot block_root = null;
	public int goalKeyBlock; //key block�� ���� ��ǥġ
	private SceneControl scene_control = null;

	void Start()
	{
		this.block_root = this.gameObject.GetComponent<BlockRoot>();
		this.scene_control = this.gameObject.GetComponent<SceneControl>();
		this.leftYarn = this.InitYarn;
		this.guistyle.fontSize = 27;
	}

    private void Update()
    {
		int keyCount = 0;
		int yarnCount = 0;
		int igniteCount = 0;
		foreach (BlockControl block in this.block_root.blocks)
		{
			if (block.isKeyBlock())
				keyCount++;
			if (block.isYarn())
				yarnCount++;
			if (block.isVanishing())
				igniteCount++;
		}
		if (igniteCount == 0) //��ȭ���� ���� ����������
			scene_control.checkClearOrOver(); // scene_control�� ���� ���¸� üũ�ϴ� �Լ��� ȣ��

		goalKeyBlock = keyCount;
		leftYarn = yarnCount;
	}

    void OnGUI()
	{
		int x = 10;
		int y = 50;
		GUI.color = Color.red;
		this.print_value(x + 20, y, "���� �н��� ��� ���ּ���, ���� �н�", this.leftYarn);
		y += 55;
		if (this.block_root.KeyMode)
		{
			//���� Ű �� UI�� ǥ��
			this.print_value(x + 20, y, "���� Ű ���� ��� ���ּ���, ���� Ű ��", this.goalKeyBlock);
			y += 55;
		}
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

	public bool isTargetClear()
	{
		if (this.leftYarn > 0)
			return false;
		if (this.block_root.KeyMode && this.goalKeyBlock>0)
			return false;
		return true;
	}
}
