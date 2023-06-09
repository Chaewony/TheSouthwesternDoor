﻿using UnityEngine;
using System.Collections;

public class BlockRoot : MonoBehaviour
{
	public bool FreeSwapMode;
	public bool KeyMode;
	public bool YarnMode;
	public bool CloudMode;

	public GameObject BlockPrefab = null; // 만들어 낼 블록의 프리팹.
	public GameObject KeyBlockPrefab = null; // 만들어 낼 블록의 프리팹.
	public GameObject YarnPrefab = null; // 만들어 낼 블록의 프리팹.
	public GameObject DarkCloudPrefab = null; // 만들어 낼 블록의 프리팹.
	public BlockControl[,] blocks; // 그리드.

	private GameObject main_camera = null; // 메인 카메라.
	private BlockControl grabbed_block = null; // 잡은 블록.

	private ScoreCounter score_counter = null; // ScoreCounter.
	private MoveCounter move_counter = null; // MoveCounter.
	private TargetCounter target_counter = null; // TargetCounter.
	protected bool is_vanishing_prev = false; // 이전에 발화했었는가?.

	public TextAsset levelData = null; // 레벨 데이터의 텍스트를 저장.
	public LevelControl level_control; // LevelControl를 저장.

//	private int mGap;
	private int mCurrentRowGap;
	private int mCurrentColumnGap;
	private int mRow;
	private int mColumn;

	AudioSource audioSource;
	public AudioClip targetAudio;
	public AudioClip puzzleMatch;
	public AudioClip bombAudio;
	public AudioClip colorAudio;

	public bool isGrabbable;

	public BombBtn bombBtn;
	public ColorBtn colorBtn;

	void Start()
	{
		this.main_camera = GameObject.FindGameObjectWithTag("MainCamera");
		this.score_counter = this.gameObject.GetComponent<ScoreCounter>();
		this.move_counter = this.gameObject.GetComponent<MoveCounter>();
		this.target_counter = this.gameObject.GetComponent<TargetCounter>();
		audioSource = GetComponent<AudioSource>();
		isGrabbable = true;
	}


	void Update()
	{
		Vector3 mouse_position; // 마우스 위치.
		this.unprojectMousePosition( // 마우스 위치를 획득.
									out mouse_position, Input.mousePosition);
		// 획득한 마우스 위치를 X와 Y만으로 한다.
		Vector2 mouse_position_xy =
			new Vector2(mouse_position.x, mouse_position.y);
		if (this.grabbed_block == null)
		{ // 블록을 잡지 않았을 때.
			if (!this.is_has_falling_block())
			{
				if (Input.GetMouseButtonDown(0) && this.isGrabbable == true)
				{ // 마우스 버튼이 눌렸다면.
				  // blocks 배열의 모든 요소를 차례로 처리한다.
					foreach (BlockControl block in this.blocks)
					{
						if (!block.isGrabbable() || block.isYarn() || block.isDarkCloud()) //털실은 grab 자체를 못하게 함
						{ // 블록을 잡을 수 없으면.
							continue; // 다음 블록으로.
						}
						// 마우스 위치가 블록 영역 안에 없으면.
						if (!block.isContainedPosition(mouse_position_xy))
						{
							continue; // 다음 블록으로.
						}
						// 처리 중인 블록을 grabbed_block에 등록.
						this.grabbed_block = block;
						// 잡았을 때의 처리를 실행.
						this.grabbed_block.beginGrab();

						//폭탄 아이템 사용으로 없애기
						if (bombBtn.usingBombItemState&& !block.isKeyBlock())
                        {
							this.grabbed_block.toVanishing();
							bombBtn.UseBombItem();
							audioSource.clip = bombAudio;
							audioSource.Play();
						}
							
						break;
					}
				}
			}
		}
		else
		{ // 블록을 잡고 있을 때.


			do
			{
				// 슬라이드할 곳의 블록을 가져온다.
				BlockControl swap_target =
					this.getNextBlock(grabbed_block, grabbed_block.slide_dir);
				// 슬라이드할 곳 블록이 비어 있다면.
				if (swap_target == null)
				{
					break; // 루프 탈출. 
				}
				// 슬라이드할 곳 블록을 잡을 수 있는 상태가 아니라면.
				if (!swap_target.isGrabbable() || swap_target.isYarn() || swap_target.isDarkCloud()) //털실과는 swap을 못하게 함
				{
					break; // 루프 탈출. 
				}
				//  현재 위치에서 슬라이드할 곳까지의 거리를 구한다.
				float offset = this.grabbed_block.calcDirOffset(
					mouse_position_xy, this.grabbed_block.slide_dir);
				// 이동 거리가 블록 크기의 절반보다 작다면 .
				if (offset < Block.COLLISION_SIZE / 2.0f)
				{
					break; // 루프 탈출. 
				}

				if (colorBtn.usingColorItemState && !grabbed_block.isKeyBlock() && !swap_target.isKeyBlock())
                {
					this.swapBlockChangeColor(grabbed_block, grabbed_block.slide_dir, swap_target);
					this.grabbed_block = null;
					colorBtn.UseColorItem();
					audioSource.clip = colorAudio;
					audioSource.Play();
					break;
				}

				// 블록을 교체한다.
				this.swapBlock(
					grabbed_block, grabbed_block.slide_dir, swap_target);

				// 2스테이지인 경우
				if (!FreeSwapMode)
				{
					// 지연 후 블럭이 발화중인지 아닌지 체크해주기 위해 코루틴 사용
					// 블럭을 sawp했을 때 match되지 않으면 블럭의 위치를 되돌리기 위한 함수 호출
					// 코루틴 함수 안에서 키 블럭을 삭제할 수 있을지 확인하는 함수를 호출함
					StartCoroutine(swapBackBlock(grabbed_block, swap_target));
				}
				else
                {
					move_counter.minusLeftMoves();
				}

				this.grabbed_block = null; // 지금은 블록을 잡고 있지 않다.
			} while (false);



			if (!Input.GetMouseButton(0))
			{ // 마우스 버튼이 눌려져 있지 않으면.
				this.grabbed_block.endGrab(); // 블록을 놓았을 때의 처리를 실행.
				this.grabbed_block = null; //  grabbed_block을 비게 설정.
			}
		}

		// 낙하 중 또는 슬라이드 중이면.
		if (this.is_has_falling_block() || this.is_has_sliding_block())
		{
			// 아무것도 하지 않는다.
			// 낙하 중도 슬라이드 중도 아니면.
		}
		else
		{
			int ignite_count = 0; // 발화 수.
								  // 그리드 안의 모든 블록에 대해서 처리.
			foreach (BlockControl block in this.blocks)
			{
				if (!block.isIdle())
				{ // 대기 중이면 루프의 처음으로 점프하고,.
					continue; // 다음 블록을 처리한다.
				}
				// 세로 또는 가로에 같은 색 블록이 세 개 이상 나열했다면.
				if(this.checkfourmatch(block)) //2*2 match
                {
					ignite_count += 2;
				}
				else if (this.checkConnection(block))
				{
					ignite_count++; // 발화 수를 증가.
				}
			}

			if (ignite_count > 0)
			{ // 발화 수가 0보다 크면.

				if (!this.is_vanishing_prev)
				{
					// 직전에 연쇄가 아니라면 발화 횟수 리셋.
					this.score_counter.clearIgniteCount();
				}
				// 발화 횟수를 늘린다.
				this.score_counter.addIgniteCount(ignite_count);
				// 합계 스코어 갱신.
				this.score_counter.updateTotalScore();



				// ＝한 군데라도 맞춰진 곳이 있으면.
				int block_count = 0; // 발화 중인 블록 수(다음 장에서 사용한다).
									 // 그리드 내의 모든 블록에 대해서 처리.
				foreach (BlockControl block in this.blocks)
				{
					if (block.isVanishing())
					{ // 발화중（점점 사라진다）이면.
						block.rewindVanishTimer(); // 재발화！.
						block_count++; // 발화 중인 블록의 개수를 증가.


						this.checkYarn(block);
					}
				}
			}
		}

		// 하나라도 연소 중인 블록이 있는가?.
		bool is_vanishing = this.is_has_vanishing_block();
		// 조건을 만족하면 블록을 떨어뜨리고 싶다.
		do
		{
			if (is_vanishing)
			{ // 연소 중인 블록이 있다면.
				break; // 낙하 처리를 실행하지 않는다.
			}
			if (this.is_has_sliding_block())
			{ // 교체 중인 블록이 있다면.
				break; // 낙하 처리를 실행하지 않는다.
			}
			for (int x = 0; x < Block.BLOCK_NUM_X; x++)
			{
				// 열에 교체 중인 블록이 있다면, 그 열은 처리하지 않고 다음 열로 진행한다.
				if (this.is_has_sliding_block_in_column(x))
				{
					continue;
				}
				// 그 열에 있는 블록을 위에서부터 검사.
				for (int y = 0; y < Block.BLOCK_NUM_Y - 1; y++)
				{
					// 지정 중인 블록이 비표시라면, 다음 블록으로.
					if (!this.blocks[x, y].isVacant())
					{
						continue;
					}
					// 지정 중인 블록 아래에 있는 블록을 검사.
					for (int y1 = y + 1; y1 < Block.BLOCK_NUM_Y; y1++)
					{
						// 아래에 있는 블록이 비표시라면, 다음 블록으로.
						if (this.blocks[x, y1].isVacant())
						{
							continue;
						}
						//  블록을 교체한다.
						this.fallBlock(this.blocks[x, y], Block.DIR4.UP,
									   this.blocks[x, y1]);
						break;
					}
				}
			}
			// 보충처리.
			for (int x = 0; x < Block.BLOCK_NUM_X; x++)
			{
				int fall_start_y = Block.BLOCK_NUM_Y + 1; //이부분 수정해서 리스폰 위치 맨 윗줄 +1 되게 함
				//for (int y = Block.BLOCK_NUM_Y-1; y >= 0; y--) //이렇게 바꿈으로써 아래부터 채워지게 됨
				for (int y = 0; y < Block.BLOCK_NUM_Y; y++)
				{
					/*if (blocks[x, y].isDarkCloud()) //먹구름 아래 있는 블럭은 채워주지 않음
						break;*/
					// 비표시 블록이 아니라면 다음 블록으로.
					if (!this.blocks[x, y].isVacant())
					{
						continue;
					}
					this.blocks[x, y].GetComponent<MeshFilter>().sharedMesh = BlockPrefab.GetComponent<MeshFilter>().sharedMesh;
					this.blocks[x, y].setKeyBlock(false);
					this.blocks[x, y].setYarn(false);
					this.blocks[x, y].setDarkCloud(false);
					this.blocks[x, y].beginRespawn(fall_start_y); // 블록 부활.
					fall_start_y++;
				}
			}
		} while (false);
		this.is_vanishing_prev = is_vanishing;
	}


	IEnumerator swapBackBlock(BlockControl grabbed_block, BlockControl swap_target)
	{
		// 이미 발화되고 있던 애들이랑은 swap을 못하게 막기 위해
		// 이미 발화되고 있는 애들은 기다리지 않고 바로 swap back 해줌
		if (grabbed_block.isVanishing() || swap_target.isVanishing())
        {
			this.swapBlock(
			swap_target, grabbed_block.slide_dir, grabbed_block);
			yield break; //코루틴 빠져나감
		}

		// 대기 시간
		yield return new WaitForSeconds(0.3f);

		// 교체된 블럭이 모두 발화중이 아니라면 되돌려놓음
		// 즉, 블럭을 sawp했을 때 sawp된 블럭 둘 중 하나라도 발화중이라면 되돌려놓지 않음
		if (!grabbed_block.isVanishing() && !swap_target.isVanishing())
		{
			this.swapBlock(
			swap_target, grabbed_block.slide_dir, grabbed_block);
		}
        else
        {
			move_counter.minusLeftMoves(); //다시 되돌리지 않는다면, 그러니까 발화가 제대로 되었다면
		}
			
		if(KeyMode)
			checkKeyBlock(grabbed_block, swap_target);
	}

	public void checkKeyBlock(BlockControl block0, BlockControl block1)
	{
		/*if (mCurrentRowGap == 0) //horizontal split 상태가 아닐때
		{
			if (block0.GetComponent<MeshFilter>().sharedMesh == KeyBlockPrefab.GetComponent<MeshFilter>().sharedMesh &&
				block0.i_pos.arrY == 0)
			{
				block0.toVanishing();
				score_counter.minusGoalKeyCount();
			}

			if (block1.GetComponent<MeshFilter>().sharedMesh == KeyBlockPrefab.GetComponent<MeshFilter>().sharedMesh &&
				block1.i_pos.arrY == 0)
			{
				block1.toVanishing();
				score_counter.minusGoalKeyCount();
			}
		}
		else //horizontal split 상태일때
		{
			if (block0.GetComponent<MeshFilter>().sharedMesh == KeyBlockPrefab.GetComponent<MeshFilter>().sharedMesh &&
				(block0.i_pos.arrY == 0 ||
				block0.i_pos.arrY == mRow + 1))
			{
				block0.toVanishing();
				score_counter.minusGoalKeyCount();
			}

			if (block1.GetComponent<MeshFilter>().sharedMesh == KeyBlockPrefab.GetComponent<MeshFilter>().sharedMesh &&
				(block1.i_pos.arrY == 0 ||
				block1.i_pos.arrY == mRow + 1))
			{
				block1.toVanishing();
				score_counter.minusGoalKeyCount();
			}
		}*/

		//split 상태이든 아니든 y좌표가 0이여야만 발화
		/*if (block0.GetComponent<MeshFilter>().sharedMesh == KeyBlockPrefab.GetComponent<MeshFilter>().sharedMesh &&
				block0.i_pos.arrY == 0)*/
		if (block0.isKeyBlock() && block0.i_pos.arrY == 0)
		{
			block0.toVanishing();
			audioSource.clip = targetAudio;      //맞춰서 없어질 때 나는 소리.
			audioSource.Play();
		}

		/*if (block1.GetComponent<MeshFilter>().sharedMesh == KeyBlockPrefab.GetComponent<MeshFilter>().sharedMesh &&
			block1.i_pos.arrY == 0)*/
		if (block1.isKeyBlock() && block1.i_pos.arrY == 0)
		{
			block1.toVanishing();
			audioSource.clip = targetAudio;
			audioSource.Play();
		}
	}

	// 블록을 만들어 내고, 가로 아홉 칸 세로 아홉 칸으로 배치.
	public void initialSetUp(int startRowGap, int startColumnGap, int row, int column)
	{
		mCurrentRowGap = startRowGap;
		mCurrentColumnGap = startColumnGap;
		mRow = row;
		mColumn = column;
		// 크기는 9×9로 한다.
		this.blocks =
			new BlockControl[Block.BLOCK_NUM_X, Block.BLOCK_NUM_Y];
		// 블록의 색 번호.
		int color_index = 0;

		Block.COLOR color = Block.COLOR.FIRST;

		for (int y = 0; y < Block.BLOCK_NUM_Y; y++)
		{ // 처음행부터 시작행부터 마지막행까지.
			for (int x = 0; x < Block.BLOCK_NUM_X; x++)
			{// 왼쪽 끝에서부터 오른쪽 끝까지.
			 // BlockPrefab의 인스턴스를 씬 위에 만든다.
				color = this.selectBlockColor();
				GameObject game_object;
				BlockControl block;

				// 2스테이지 일때 색에 기반해서 key 블럭(둥근 모양)으로 설정함
				if (KeyMode &&
					((x == 0 && y == Block.BLOCK_NUM_Y - 1) //2사분면의 좌상단 
					|| (x == 0 && y == row) //3사분면의 좌상단
					|| (x == Block.BLOCK_NUM_X - 1 && y == Block.BLOCK_NUM_Y - 1) //1사분면의 우상단
					|| (x == Block.BLOCK_NUM_X - 1 && y == row))) //4사분면의 우상단
				{
					game_object =
					Instantiate(this.KeyBlockPrefab) as GameObject;
					// 위에서 만든 블록의 BlockControl 클래스를 가져온다.
					block = game_object.GetComponent<BlockControl>();
					// 블록을 칸에 넣는다.
					this.blocks[x, y] = block;

					color = Block.COLOR.YELLOW; //안쓰는 색인 노란색으로 색을 정함
					block.setKeyBlock(true);
				}
				else if (YarnMode &&
					((x == Block.BLOCK_NUM_X - 1 && y == row + 1) //1사분면의 좌 하단
					|| (x == 0 && y == row + 1) //2사분면의 좌 하단
					|| (x == 0 && y == 0) //3사분면의 좌하단
					|| (x == Block.BLOCK_NUM_X - 1 && y == 0))) //4사분면의 우하단
				{
					game_object =
					Instantiate(this.YarnPrefab) as GameObject;
					// 위에서 만든 블록의 BlockControl 클래스를 가져온다.
					block = game_object.GetComponent<BlockControl>();
					// 블록을 칸에 넣는다.
					this.blocks[x, y] = block;

					color = Block.COLOR.BLACK; //안쓰는 색인 검은색으로 색을 정함
					block.setYarn(true);
				}
				else if (CloudMode && y == row)
				{
					game_object = Instantiate(this.DarkCloudPrefab) as GameObject;
					// 위에서 만든 블록의 BlockControl 클래스를 가져온다.
					block = game_object.GetComponent<BlockControl>();
					// 블록을 칸에 넣는다.
					this.blocks[x, y] = block;
					block.setDarkCloud(true);
				}
				else
                {
					game_object = Instantiate(this.BlockPrefab) as GameObject;

					// 위에서 만든 블록의 BlockControl 클래스를 가져온다.
					block = game_object.GetComponent<BlockControl>();
					// 블록을 칸에 넣는다.
					this.blocks[x, y] = block;
				}
				
				// 블록의 배열을 위한 좌표 (인덱스) 를 설정.
				block.i_pos.arrX = x;
				block.i_pos.arrY = y;

				// 블록의 위치 정보(그리드 좌표)를 설정.
				if (x > mColumn)
					block.i_pos.currentX = x + mCurrentColumnGap;
				else
					block.i_pos.currentX = x;

				if (y> mRow)
					block.i_pos.currentY = y + mCurrentRowGap;
				else
					block.i_pos.currentY = y;

				//block.i_pos.y = y;
				// 각 BlockControl이 연계하는 GameRoot는 자신이라고 설정.
				block.block_root = this;
				// 그리드 좌표를 실제 위치(씬 좌표)로 변환.
				Vector3 position = BlockRoot.calcBlockPosition(block.i_pos);
				// 씬 상의 블록 위치를 이동.
				block.transform.position = position;

				// 블록의 색을 변경. 
				// block.setColor((Block.COLOR)color_index);
				// 지금의 출현 확률을 바탕으로 색을 결정한다.
				/*color = this.selectBlockColor();
				block.setColor(color);*/
				block.setColor(color);
				// 블록의 이름을 설정(후술).
				block.name = "block(" + block.i_pos.arrX.ToString() +
					"," + block.i_pos.arrY.ToString() + ")";
				// 모든 종류의 색 중에서 임의로 한 색을 선택.
				color_index =
					Random.Range(0, (int)Block.COLOR.NORMAL_COLOR_NUM);
			}
		}
	}

	public void verticalSplitSetUp(int gap)
	{
		Debug.Log("verticalSplitSetUp 실행");
		for (int y = 0; y < Block.BLOCK_NUM_Y; y++)
		{ // 처음행부터 시작행부터 마지막행까지.
			for (int x = 0; x < Block.BLOCK_NUM_X; x++)
			{// 왼쪽 끝에서부터 오른쪽 끝까지.
			 
				// BlockPrefab의 인스턴스로 씬 위에 올라가 있는 애들을 가져온다.
				GameObject game_object = GameObject.Find("block(" + x.ToString() + "," + y.ToString() + ")");
				// null 체크
				if (game_object == null) 
					break;
				// 위에서 가져온 블록의 BlockControl 클래스를 가져온다.
				BlockControl block = game_object.GetComponent<BlockControl>();

				// 블록의 위치 정보(그리드 좌표)를 재설정.
				if (x > mColumn)
					block.i_pos.currentX = block.i_pos.arrX + gap;

				// 각 BlockControl이 연계하는 GameRoot는 자신이라고 설정.
				block.block_root = this;
				// 그리드 좌표를 실제 위치(씬 좌표)로 변환.
				Vector3 position = BlockRoot.calcBlockPosition(block.i_pos);
				// 씬 상의 블록 위치를 이동.
				block.transform.position = position;
			}
		}
		mCurrentColumnGap = gap;
	}

	public void horizontalSplitSetUp(int gap)
	{
		Debug.Log("horizontalSplitSetUp 실행");
		for (int y = 0; y < Block.BLOCK_NUM_Y; y++)
		{ // 처음행부터 시작행부터 마지막행까지.
			for (int x = 0; x < Block.BLOCK_NUM_X; x++)
			{// 왼쪽 끝에서부터 오른쪽 끝까지.

				// BlockPrefab의 인스턴스로 씬 위에 올라가 있는 애들을 가져온다.
				GameObject game_object = GameObject.Find("block(" + x.ToString() + "," + y.ToString() + ")");
				// null 체크
				if (game_object == null)
					break;
				// 위에서 가져온 블록의 BlockControl 클래스를 가져온다.
				BlockControl block = game_object.GetComponent<BlockControl>();

				// 블록의 위치 정보(그리드 좌표)를 재설정.
				if (y > mRow)
					block.i_pos.currentY = block.i_pos.arrY + gap;

				// 각 BlockControl이 연계하는 GameRoot는 자신이라고 설정.
				block.block_root = this;
				// 그리드 좌표를 실제 위치(씬 좌표)로 변환.
				Vector3 position = BlockRoot.calcBlockPosition(block.i_pos);
				// 씬 상의 블록 위치를 이동.
				block.transform.position = position;
			}
		}
		mCurrentRowGap = gap;
	}

	// 지정된 그리드 좌표에서 씬 상의 좌표를 구한다. 
	public static Vector3 calcBlockPosition(Block.iPosition i_pos)
	{
		// 배치할 좌측 상단 모퉁이 위치를 초깃값으로 설정.
		Vector3 position = new Vector3(-(Block.BLOCK_NUM_X / 2.0f - 0.5f),
									   -(Block.BLOCK_NUM_Y / 2.0f - 0.5f), 0.0f);
		// 초깃값＋그리드 좌표 × 블록 크기.
		position.x += (float)i_pos.currentX * Block.COLLISION_SIZE;
		position.y += (float)i_pos.currentY * Block.COLLISION_SIZE;
		return (position); // 씬의 좌표를 반환한다.
	}


	public bool unprojectMousePosition(out Vector3 world_position, Vector3 mouse_position)
	{
		bool ret;
		// 판을 생성. 이 판은 카메라에서 보이는 면이 앞.
		// 블록의 절반 크기만큼 앞으로 놓인다.
		Plane plane = new Plane(Vector3.back, new Vector3(
			0.0f, 0.0f, -Block.COLLISION_SIZE / 2.0f));
		// 카메라와 마우스를 통과하는 광선을 생성.
		Ray ray = this.main_camera.GetComponent<Camera>().ScreenPointToRay(
			mouse_position);
		float depth;
		// 광선 ray가 판 plane에 닿았다면.
		if (plane.Raycast(ray, out depth))
		{
			// 인수 world_position을 마우스 위치로 덮어쓴다.
			world_position = ray.origin + ray.direction * depth;
			ret = true;
			// 닿지 않았다면.
		}
		else
		{
			// 인수 world_position을 제로인 벡터로 덮어쓴다.
			world_position = Vector3.zero;
			ret = false;
		}
		return (ret);
	}




	public BlockControl getNextBlock(
		BlockControl block, Block.DIR4 dir)
	{
		// 슬라이드할 곳의 블록을 여기에 저장.
		BlockControl next_block = null;
		switch (dir)
		{
			case Block.DIR4.RIGHT:
				if (block.i_pos.arrX < Block.BLOCK_NUM_X - 1
					&& (mCurrentColumnGap == 0 || block.i_pos.arrX != mColumn)) //gap이 0이냐 아니냐에 따른 이동 제한
				{// 그리드 안이라면.
					next_block = this.blocks[block.i_pos.arrX + 1, block.i_pos.arrY];
				}
				break;

			case Block.DIR4.LEFT:
				if (block.i_pos.arrX > 0
					&& (mCurrentColumnGap == 0 || block.i_pos.arrX != mColumn + 1))
				{ // 그리드 안이라면.
					next_block = this.blocks[block.i_pos.arrX - 1, block.i_pos.arrY];
				}
				break;
			case Block.DIR4.UP:
				if (block.i_pos.arrY < Block.BLOCK_NUM_Y - 1 
					&& (mCurrentRowGap == 0 || block.i_pos.arrY != mRow))
				{ // 그리드 안이라면.
					next_block = this.blocks[block.i_pos.arrX, block.i_pos.arrY + 1];
				}
				break;
			case Block.DIR4.DOWN:
				if (block.i_pos.arrY > 0
					&& (mCurrentRowGap == 0 || block.i_pos.arrY != mRow + 1))
				{ // 그리드 안이라면.
					next_block = this.blocks[block.i_pos.arrX, block.i_pos.arrY - 1];
				}
				break;
		}
		return (next_block);
	}

	public static Vector3 getDirVector(Block.DIR4 dir)
	{
		Vector3 v = Vector3.zero;
		switch (dir)
		{
			case Block.DIR4.RIGHT: v = Vector3.right; break; // 오른쪽으로 1단위 이동한다.
			case Block.DIR4.LEFT: v = Vector3.left; break; // 왼쪽으로 1단위 이동한다.
			case Block.DIR4.UP: v = Vector3.up; break; // 위로 1단위 이동한다.
			case Block.DIR4.DOWN: v = Vector3.down; break; // 아래로 1단위 이동한다.
		}
		v *= Block.COLLISION_SIZE; // 블록 크기를 곱한다.
		return (v);
	}

	public static Block.DIR4 getOppositDir(Block.DIR4 dir)
	{
		Block.DIR4 opposit = dir;
		switch (dir)
		{
			case Block.DIR4.RIGHT: opposit = Block.DIR4.LEFT; break;
			case Block.DIR4.LEFT: opposit = Block.DIR4.RIGHT; break;
			case Block.DIR4.UP: opposit = Block.DIR4.DOWN; break;
			case Block.DIR4.DOWN: opposit = Block.DIR4.UP; break;
		}
		return (opposit);
	}

	public void swapBlockChangeColor(BlockControl block0, Block.DIR4 dir, BlockControl block1)
    {
		// 각 블록의 색을 기억해 둔다.
		Block.COLOR color0 = block0.color;
		Block.COLOR color1 = block1.color;

		// 각 블록의 메쉬를 기억해 둔다
		Mesh block0Mesh = block0.gameObject.GetComponent<MeshFilter>().sharedMesh;
		Mesh block1Mesh = block1.gameObject.GetComponent<MeshFilter>().sharedMesh;

		bool isKey0 = block0.isKeyBlock();
		bool isKey1 = block1.isKeyBlock();
		bool isYarn0 = block0.isYarn();
		bool isYarn1 = block1.isYarn();

		// 각 블록의.
		// 확대율을 기억해 둔다.
		Vector3 scale0 =
			block0.transform.localScale;
		Vector3 scale1 =
			block1.transform.localScale;
		//  각 블록의 '사라지는 시간'을 기억해 둔다.
		float vanish_timer0 = block0.vanish_timer;
		float vanish_timer1 = block1.vanish_timer;
		// 각 블록이 이동할 곳을 구한다.
		Vector3 offset0 = BlockRoot.getDirVector(dir);
		Vector3 offset1 = BlockRoot.getDirVector(BlockRoot.getOppositDir(dir));
		block0.setColor(color1); //  색을 교체한다.
	//	block1.setColor(color0);

		//메쉬 필터를 통한 외형 변경
		block0.GetComponent<MeshFilter>().sharedMesh = block1Mesh;
		block1.GetComponent<MeshFilter>().sharedMesh = block0Mesh;

		block0.setKeyBlock(isKey1);
		block1.setKeyBlock(isKey0);
		block0.setYarn(isYarn1);
		block1.setYarn(isYarn0);

		block0.transform.localScale = scale1; // 확대율을 교체한다.
		block1.transform.localScale = scale0;
		block0.vanish_timer = vanish_timer1; // 사라지는 시간을 교체한다.
		block1.vanish_timer = vanish_timer0;
		block0.beginSlide(offset0); // 원래 블록의 이동을 시작.
		block1.beginSlide(offset1); // 이동할 곳의 블록 이동을 시작.

		swapBlock(block1, dir, block0);
	}

	public void swapBlock(BlockControl block0, Block.DIR4 dir, BlockControl block1)
	{
		// 각 블록의 색을 기억해 둔다.
		Block.COLOR color0 = block0.color;
		Block.COLOR color1 = block1.color;

		// 각 블록의 메쉬를 기억해 둔다
		Mesh block0Mesh = block0.gameObject.GetComponent<MeshFilter>().sharedMesh;
		Mesh block1Mesh = block1.gameObject.GetComponent<MeshFilter>().sharedMesh;

		bool isKey0 = block0.isKeyBlock();
		bool isKey1 = block1.isKeyBlock();
		bool isYarn0 = block0.isYarn();
		bool isYarn1 = block1.isYarn();

		// 각 블록의.
		// 확대율을 기억해 둔다.
		Vector3 scale0 =
			block0.transform.localScale;
		Vector3 scale1 =
			block1.transform.localScale;
		//  각 블록의 '사라지는 시간'을 기억해 둔다.
		float vanish_timer0 = block0.vanish_timer;
		float vanish_timer1 = block1.vanish_timer;
		// 각 블록이 이동할 곳을 구한다.
		Vector3 offset0 = BlockRoot.getDirVector(dir);
		Vector3 offset1 = BlockRoot.getDirVector(BlockRoot.getOppositDir(dir));
		block0.setColor(color1); //  색을 교체한다.
		block1.setColor(color0);

		//메쉬 필터를 통한 외형 변경
		block0.GetComponent<MeshFilter>().sharedMesh = block1Mesh;
		block1.GetComponent<MeshFilter>().sharedMesh = block0Mesh;

		block0.setKeyBlock(isKey1);
		block1.setKeyBlock(isKey0);
		block0.setYarn(isYarn1);
		block1.setYarn(isYarn0);

		block0.transform.localScale = scale1; // 확대율을 교체한다.
		block1.transform.localScale = scale0;
		block0.vanish_timer = vanish_timer1; // 사라지는 시간을 교체한다.
		block1.vanish_timer = vanish_timer0;
		block0.beginSlide(offset0); // 원래 블록의 이동을 시작.
		block1.beginSlide(offset1); // 이동할 곳의 블록 이동을 시작.
	}

	public bool checkfourmatch(BlockControl start)
	{
		bool ret = false;
		bool squ = false;
		//bool midche = false;

		int normal_block_num = 0;
		// 인수인 블록이 발화 후가 아니면.
		if (!start.isVanishing())
		{
			normal_block_num = 1;
		}
		else
		{
			return false;
		}
		// 그리드 좌표를 기억해 둔다.
		int rx;
		int lx;
		rx = start.i_pos.arrX;
		lx = start.i_pos.arrX;
		// 블록의 왼쪽을 검사.
		for (int x = lx - 1; x > 0; x--)
		{
			BlockControl next_block = this.blocks[x, start.i_pos.arrY];
			if (next_block.color != start.color || (mCurrentColumnGap != 0 && next_block.i_pos.arrX == mColumn))
			{ // 색이 다르면.
				break; // 루프 탈출.
			}
			if (next_block.step == Block.STEP.FALL || // 낙하 중이면.
			   next_block.next_step == Block.STEP.FALL)
			{
				break; // 루프 탈출.
			}
			if (next_block.step == Block.STEP.SLIDE || // 슬라이드 중이면.
			   next_block.next_step == Block.STEP.SLIDE)
			{
				break; // 루프 탈출.
			}
			if (!next_block.isVanishing())
			{ // 발화 중이 아니면.
				normal_block_num++; // 검사용 카운터를 증가.
			}
			lx = x;
		}
		// 블록의 오른쪽을 검사.
		for (int x = rx + 1; x < Block.BLOCK_NUM_X; x++)
		{
			BlockControl next_block = this.blocks[x, start.i_pos.arrY];
			if (next_block.color != start.color || (mCurrentColumnGap != 0 && next_block.i_pos.arrX == mColumn + 1))
			{
				break;
			}
			if (next_block.step == Block.STEP.FALL ||
			   next_block.next_step == Block.STEP.FALL)
			{
				break;
			}
			if (next_block.step == Block.STEP.SLIDE ||
			   next_block.next_step == Block.STEP.SLIDE)
			{
				break;
			}
			if (!next_block.isVanishing())
			{
				normal_block_num++;
			}
			rx = x;
		}

		normal_block_num = 0;
		if (!start.isVanishing())
		{
			normal_block_num = 1;
		}
		int uy = start.i_pos.arrY;
		int dy = start.i_pos.arrY;
		// 블록의 위쪽을 검사. 라고 쓰여있는데 아래쪽 검사하는 코드인 것 같음!
		for (int y = dy - 1; y > 0; y--)
		{
			BlockControl next_block = this.blocks[start.i_pos.arrX, y];
			if (next_block.color != start.color || (mCurrentRowGap != 0 && next_block.i_pos.arrY == mRow)) { break; }
			if (next_block.step == Block.STEP.FALL || next_block.next_step == Block.STEP.FALL) { break; }
			if (next_block.step == Block.STEP.SLIDE || next_block.next_step == Block.STEP.SLIDE) { break; }
			if (!next_block.isVanishing()) { normal_block_num++; }
			dy = y;
		}
		// 블록의 아래쪽을 검사. 라고 쓰여있는데 위쪽 검사하는 코드인 것 같음!
		for (int y = uy + 1; y < Block.BLOCK_NUM_Y; y++)
		{
			BlockControl next_block = this.blocks[start.i_pos.arrX, y];
			if (next_block.color != start.color || (mCurrentRowGap != 0 && next_block.i_pos.arrY == mRow + 1)) { break; }
			if (next_block.step == Block.STEP.FALL || next_block.next_step == Block.STEP.FALL) { break; }
			if (next_block.step == Block.STEP.SLIDE || next_block.next_step == Block.STEP.SLIDE) { break; }
			if (!next_block.isVanishing()) { normal_block_num++; }
			uy = y;
		}
		do
		{
			if ((rx - lx + 1 == 2 && uy - dy + 1 == 2) && this.blocks[lx, dy].color == this.blocks[lx, dy + 1].color
			   && this.blocks[lx, dy].color == this.blocks[lx + 1, dy].color && this.blocks[lx, dy].color == this.blocks[lx + 1, dy + 1].color)
			{
				this.blocks[lx, dy].toVanishing();
				this.blocks[lx, dy + 1].toVanishing();
				this.blocks[lx + 1, dy].toVanishing();
				this.blocks[lx + 1, dy + 1].toVanishing();
				audioSource.clip = puzzleMatch;
				audioSource.Play();
				ret = true;
				squ = true;
			}
			else if (normal_block_num == 0)
			{
				break;
			}
			else
			{
				break;
			}
			/*else
			{
			   // move on to the next block
			   lx++;
			   rx++;
			   if (rx >= Block.BLOCK_NUM_X) break;
			}*/
		} while (false);
		return (ret);
	}
	public bool checkConnection(BlockControl start)
	{
		bool ret = false;
		int normal_block_num = 0;
		// 인수인 블록이 발화 후가 아니면.
		if (!start.isVanishing())
		{
			normal_block_num = 1;
		}
		// 그리드 좌표를 기억해 둔다.
		int rx;
		int lx;
		rx = start.i_pos.arrX;
		lx = start.i_pos.arrX;
		// 블록의 왼쪽을 검사.
		for (int x = lx - 1; x > 0; x--)
		{
			BlockControl next_block = this.blocks[x, start.i_pos.arrY];
			if (next_block.isKeyBlock() || next_block.isYarn()) //현재 블럭이 key block이거나 털실이면
            {
				break; //루프 탈출
            }
			if (next_block.color != start.color || (mCurrentColumnGap != 0 && next_block.i_pos.arrX == mColumn))
			{ // 색이 다르면.
				break; // 루프 탈출.
			}
			if (next_block.step == Block.STEP.FALL || // 낙하 중이면.
			   next_block.next_step == Block.STEP.FALL)
			{
				break; // 루프 탈출.
			}
			if (next_block.step == Block.STEP.SLIDE || // 슬라이드 중이면.
			   next_block.next_step == Block.STEP.SLIDE)
			{
				break; // 루프 탈출.
			}
			if (!next_block.isVanishing())
			{ // 발화 중이 아니면.
				normal_block_num++; // 검사용 카운터를 증가.
			}
			lx = x;
		}
		// 블록의 오른쪽을 검사.
		for (int x = rx + 1; x < Block.BLOCK_NUM_X; x++)
		{
			BlockControl next_block = this.blocks[x, start.i_pos.arrY];
			if (next_block.isKeyBlock() || next_block.isYarn()) //현재 블럭이 key block이거나 털실이면
			{
				break; //루프 탈출
			}
			if (next_block.color != start.color || (mCurrentColumnGap != 0 && next_block.i_pos.arrX == mColumn + 1))
			{
				break;
			}
			if (next_block.step == Block.STEP.FALL ||
			   next_block.next_step == Block.STEP.FALL)
			{
				break;
			}
			if (next_block.step == Block.STEP.SLIDE ||
			   next_block.next_step == Block.STEP.SLIDE)
			{
				break;
			}
			if (!next_block.isVanishing())
			{
				normal_block_num++;
			}
			rx = x;
		}
		do
		{
			// 오른쪽 블록의 그리드 번호 - 왼쪽 블록의 그리드 번호 +.
			// 중앙 블록(1)을 더한 수가 3미만 이면.
			if (rx - lx + 1 < 3)
			{
				break; // 루프 탈출.
			}
			if (normal_block_num == 0)
			{ // 발화 중이 아닌 블록이 하나도 없으면.
				break; // 루프 탈출.
			}
			for (int x = lx; x < rx + 1; x++)
			{
				// 나열된 같은 색 블록을 발화 상태로.
				this.blocks[x, start.i_pos.arrY].toVanishing();
				audioSource.clip = puzzleMatch;
				audioSource.Play();
				ret = true;
			}
		} while (false);
		normal_block_num = 0;
		if (!start.isVanishing())
		{
			normal_block_num = 1;
		}
		int uy = start.i_pos.arrY;
		int dy = start.i_pos.arrY;
		// 블록의 위쪽을 검사. 라고 쓰여있는데 아래쪽 검사하는 코드인 것 같음!
		for (int y = dy - 1; y > 0; y--)
		{
			BlockControl next_block = this.blocks[start.i_pos.arrX, y];
			if (next_block.isKeyBlock() || next_block.isYarn()) //현재 블럭이 key block이거나 털실이면
			{
				break; //루프 탈출
			}
			if (next_block.color != start.color || (mCurrentRowGap != 0 && next_block.i_pos.arrY == mRow)) { break; }
			if (next_block.step == Block.STEP.FALL || next_block.next_step == Block.STEP.FALL) { break; }
			if (next_block.step == Block.STEP.SLIDE || next_block.next_step == Block.STEP.SLIDE) { break; }
			if (!next_block.isVanishing()) { normal_block_num++; }
			dy = y;
		}
		// 블록의 아래쪽을 검사. 라고 쓰여있는데 위쪽 검사하는 코드인 것 같음!
		for (int y = uy + 1; y < Block.BLOCK_NUM_Y; y++)
		{
			BlockControl next_block = this.blocks[start.i_pos.arrX, y];
			if(next_block.isKeyBlock() || next_block.isYarn()) //현재 블럭이 key block이거나 털실이면
			{
				break; //루프 탈출
            }
			if (next_block.color != start.color || (mCurrentRowGap != 0 && next_block.i_pos.arrY == mRow + 1)) { break; }
			if (next_block.step == Block.STEP.FALL || next_block.next_step == Block.STEP.FALL) { break; }
			if (next_block.step == Block.STEP.SLIDE || next_block.next_step == Block.STEP.SLIDE) { break; }
			if (!next_block.isVanishing()) { normal_block_num++; }
			uy = y;
		}
		do
		{
			if (uy - dy + 1 < 3) { break; }
			if (normal_block_num == 0) { break; }
			for (int y = dy; y < uy + 1; y++)
			{
				this.blocks[start.i_pos.arrX, y].toVanishing();
				audioSource.clip = puzzleMatch;
				audioSource.Play();
				ret = true;
			}
		} while (false);
		return (ret);
	}

	public int checkYarn(BlockControl start) //주변에 털실이 있는지 확인하는 코드
	{
		int ret = 0;
		// 그리드 좌표를 기억해 둔다.
		int rx;
		int lx;
		rx = start.i_pos.arrX;
		lx = start.i_pos.arrX;
		// 블록의 왼쪽을 검사.
		if ((mCurrentColumnGap != 0 && lx - 1 == mColumn) || lx - 1 < 0) 
        {
			
		}
		else if (this.blocks[lx - 1, start.i_pos.arrY].isYarn())
		{
			
			this.blocks[lx - 1, start.i_pos.arrY].toVanishing();
			this.blocks[lx - 1, start.i_pos.arrY].setYarn(false);
			audioSource.clip = targetAudio;
			audioSource.Play();
			ret++;
		}
		// 블록의 오른쪽을 검사.
		if ((mCurrentColumnGap != 0 && rx + 1 == mColumn + 1) || rx + 1 >= Block.BLOCK_NUM_X)
		{
			
		}
		else if(this.blocks[rx + 1, start.i_pos.arrY].isYarn()) 
		{ 
			this.blocks[rx + 1, start.i_pos.arrY].toVanishing();
			this.blocks[rx + 1, start.i_pos.arrY].setYarn(false);
			audioSource.clip = targetAudio;
			audioSource.Play();
			ret++;
		}
		
		int uy = start.i_pos.arrY;
		int dy = start.i_pos.arrY;
		//블록의 아래쪽을 검사
		if ((mCurrentRowGap != 0 && dy - 1 == mRow) || dy - 1 < 0) 
		{
			
		}
		else if (this.blocks[start.i_pos.arrX, dy - 1].isYarn())
        {
			this.blocks[start.i_pos.arrX, dy - 1].toVanishing();
			this.blocks[start.i_pos.arrX, dy - 1].setYarn(false);
			audioSource.clip = targetAudio;
			audioSource.Play();
			ret++;
		}
		//블록의 위쪽을 검사
		if ((mCurrentRowGap != 0 && uy + 1 == mRow + 1) || uy + 1 >= Block.BLOCK_NUM_Y)
		{
			
		}
		else if (this.blocks[start.i_pos.arrX, uy + 1].isYarn())
        {
			this.blocks[start.i_pos.arrX, uy + 1].toVanishing();
			this.blocks[start.i_pos.arrX, uy + 1].setYarn(false);
			audioSource.clip = targetAudio;
			audioSource.Play();
			ret++;
		}
		return (ret);
	}

	private bool is_has_vanishing_block()
	{
		bool ret = false;
		foreach (BlockControl block in this.blocks)
		{
			if (block.vanish_timer > 0.0f)
			{
				ret = true;
				break;
			}
		}
		return (ret);
	}

	private bool is_has_sliding_block()
	{
		bool ret = false;
		foreach (BlockControl block in this.blocks)
		{
			if (block.step == Block.STEP.SLIDE)
			{
				ret = true;
				break;
			}
		}
		return (ret);
	}

	private bool is_has_falling_block()
	{
		bool ret = false;
		foreach (BlockControl block in this.blocks)
		{
			if (block.step == Block.STEP.FALL)
			{
				ret = true;
				break;
			}
		}
		return (ret);
	}

	public void fallBlock(
		BlockControl block0, Block.DIR4 dir, BlockControl block1)
	{
		/*if (block0.isDarkCloud() || block1.isDarkCloud())
			return;*/
		// block0과 block1의 색, 크기, 사라질 때까지 걸리는 시간, 표시, 비표시, 상태를 기록.
		Block.COLOR color0 = block0.color;
		Block.COLOR color1 = block1.color;

		// 각 블록의 메쉬를 기억해 둔다
		Mesh block0Mesh = block0.gameObject.GetComponent<MeshFilter>().sharedMesh;
		Mesh block1Mesh = block1.gameObject.GetComponent<MeshFilter>().sharedMesh;

		bool isKey0 = block0.isKeyBlock();
		bool isKey1 = block1.isKeyBlock();
		bool isYarn0 = block0.isYarn();
		bool isYarn1 = block1.isYarn();
		bool isCloud0 = block0.isDarkCloud();
		bool isCloud1 = block1.isDarkCloud();

		Vector3 scale0 = block0.transform.localScale;
		Vector3 scale1 = block1.transform.localScale;
		float vanish_timer0 = block0.vanish_timer;
		float vanish_timer1 = block1.vanish_timer;
		bool visible0 = block0.isVisible();
		bool visible1 = block1.isVisible();
		Block.STEP step0 = block0.step;
		Block.STEP step1 = block1.step;

		// block0과 block1의 각종 속성을 교체한다.
		block0.setColor(color1);
		block1.setColor(color0);

		//메쉬 필터를 통한 외형 변경
		block0.GetComponent<MeshFilter>().sharedMesh = block1Mesh;
		block1.GetComponent<MeshFilter>().sharedMesh = block0Mesh;

		block0.setKeyBlock(isKey1);
		block1.setKeyBlock(isKey0);
		block0.setYarn(isYarn1);
		block1.setYarn(isYarn0);
		block0.setDarkCloud(isCloud1);
		block1.setDarkCloud(isCloud0);

		block0.transform.localScale = scale1;
		block1.transform.localScale = scale0;
		block0.vanish_timer = vanish_timer1;
		block1.vanish_timer = vanish_timer0;
		block0.setVisible(visible1);
		block1.setVisible(visible0);
		block0.step = step1;
		block1.step = step0;
		block0.beginFall(block1);

		if (KeyMode)
			checkKeyBlock(block0, block1); //떨어지는 애들 키 블럭 체크
	}


	private bool is_has_sliding_block_in_column(int x)
	{
		bool ret = false;
		for (int y = 0; y < Block.BLOCK_NUM_Y; y++)
		{
			if (this.blocks[x, y].isSliding())
			{ // 슬라이드 중인 블록이 있으면.
				ret = true; // true를 반환한다. 
				break;
			}
		}
		return (ret);
	}



	public void create()
	{
		this.level_control = new LevelControl();
		this.level_control.initialize(); // 레벨 데이터 초기화.
		this.level_control.loadLevelData(this.levelData); // 데이터 읽기.
		this.level_control.selectLevel(); // 레벨 선택.
	}
	public Block.COLOR selectBlockColor()
	{
		Block.COLOR color = Block.COLOR.FIRST;
		// 이번 레벨의 레벨 데이터를 가져온다.
		LevelData level_data =
			this.level_control.getCurrentLevelData();
		float rand = Random.Range(0.0f, 1.0f); // 0.0~1.0 사이의 난수.
		float sum = 0.0f; // 출현 확률의 합계.
		int i = 0;
		// 블록의 종류 전체를 처리하는 루프.
		for (i = 0; i < level_data.probability.Length - 1; i++)
		{
			if (level_data.probability[i] == 0.0f)
			{
				continue; // 출현 확률이 0이면 루프의 처음으로 점프.
			}
			sum += level_data.probability[i]; // 출현 확률을 더한다.
			if (rand < sum)
			{ // 합계가 난숫값을 웃돌면.
				break; // 루프를 빠져나온다.
			}
		}
		color = (Block.COLOR)i; // i번째 색을 반환한다.
		return (color);
	}



}