using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TargetCounter : MonoBehaviour
{
	[SerializeField]
	private int InitYarn; // 최초 털실 숫자
	private int leftYarn; // 현재 남은 이동 가능 횟수

	public GUIStyle guistyle; // 폰트 스타일.

	private BlockRoot block_root = null;
	public int goalKeyBlock; //key block을 없앨 목표치
	private SceneControl scene_control = null;
	public bool isIgniting = false;

	public int timer = 0;

	void Start()
	{
		this.block_root = this.gameObject.GetComponent<BlockRoot>();
		this.scene_control = this.gameObject.GetComponent<SceneControl>();
		this.leftYarn = this.InitYarn;
		this.guistyle.fontSize = 30;
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
			if (!block.isIdle())
			{
				igniteCount++;
				isIgniting = true;
			}
		}
		timer++;
		if (igniteCount == 0 && timer > 1200)  //발화중인 블럭이 없을때마다
		{
			//scene_control.checkClearOrOver(); // scene_control의 게임 상태를 체크하는 함수를 호출
			isIgniting = false;
			timer = 0;
		}
		Debug.Log(isIgniting);
		//Debug.Log(igniteCount);

		goalKeyBlock = keyCount;
		leftYarn = yarnCount;
	}

	void OnGUI()
	{
		int x = 10;
		int y = 180;
		GUI.color = Color.red;
		this.print_value(x + 150, y, "남은 털실: ", this.leftYarn);
		y += 75;
		if (this.block_root.KeyMode)
		{
			//남은 키 블럭 UI에 표시
			this.print_value(x + 150, y, "남은 키: ", this.goalKeyBlock);
			y += 55;
		}
	}

	public void print_value(int x, int y, string label, int value)
	{
		// label을 표시.
		GUI.Label(new Rect(x, y, 100, 20), label, guistyle);
		//y += 25;
		// 다음 행에 value를 표시.
		GUI.Label(new Rect(x + 120, y, 100, 20), value.ToString(), guistyle);
		y += 25;
	}

	public bool isTargetClear()
	{
		if (this.leftYarn > 0)
			return false;
		if (this.block_root.KeyMode && this.goalKeyBlock > 0)
			return false;
		return true;
	}
}
