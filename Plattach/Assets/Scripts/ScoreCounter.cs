using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class ScoreCounter : MonoBehaviour
{
	[SerializeField]
	private int bonusNorm; //피버 기준, 만약에 이게 1000이라고 치면 n천점일때마다 피버가 실행되는거임
							// 여기서 피버는, 일정 시간 동안 지속되는게 아니라, 그냥 이동 횟수 1회 늘려주는 찬스라고 생각하면됨
	public struct Count
	{ // 점수 관리용 구조체.
		public int ignite; // 발화 수.
		public int score; // 점수.
		public int total_score; // 합계 점수.
		public int bonus_gage;
		//public int moveleft_score;
	};
	public Count last; // 마지막(이번) 점수.
	public Count best; // 최고 점수.
	public static int QUOTA_SCORE = 1000; // 클리어에 필요한 점수.
	public GUIStyle guistyle; // 폰트 스타일.;

	private BlockRoot block_root = null;
	private MoveCounter move_counter = null;
	private MoveCounter left_counter = null;
	public GameObject scoreManagerObject;
	private ScoreManager scoreManager;
	public int bonusCount = 0;
	//public int level;
	//public int moveleft_score;
	Rect rScrollRect;  // 화면상의 스크롤 뷰의 위치
	Rect rScrollArea; // 총 스크롤 되는 공간
	Vector2 vScrollPos; // 스크롤 바의 위치
	float hSbarValue;

	public Image energy;
	public Text energypercent;

	public int plusMove;

	AudioSource audioSource;
	public AudioSource bonusAudioSource;
	public AudioClip bonusAudio;

	public GameObject leftEnergyIcon;
	Animator animator;
	void Start()
	{
		this.left_counter = this.gameObject.GetComponent<MoveCounter>();
		this.block_root = this.gameObject.GetComponent<BlockRoot>();
		this.move_counter = this.gameObject.GetComponent<MoveCounter>();
		scoreManagerObject = GameObject.Find("ScoreManager");
		this.scoreManager = scoreManagerObject.GetComponent<ScoreManager>();
		this.last.ignite = 0;
		this.last.score = 0;
		this.last.total_score = this.scoreManager.GetCurrentScore();
		this.guistyle.fontSize = 25;
		this.last.bonus_gage = 0;
		audioSource = GetComponent<AudioSource>();

		animator = leftEnergyIcon.GetComponent<Animator>();
	}

	void OnGUI()
	{
		this.guistyle.fontSize = 40;
		int x = 1350;
		int y = 100;
		GUI.color = Color.black;
		/*this.print_value(x + 20, y, "발화 카운트", this.last.ignite);
		y += 50;
		this.print_value(x + 20, y, "가산 스코어", this.last.score);
		y += 50;*/
		this.print_value(x + 20, y, "점수", this.last.total_score);
		/*x -= 1000;
		y -= 50;
		this.print_value(x + 20, y, "보너스 이동 게이지", (float)this.last.bonus_gage / bonusNorm * 100);*/

		/*rScrollRect = new Rect(100, 100, 400, 400); // 화면상의 100, 100, 400, 400 의 위치에 스크롤 공간을 잡는다.
		rScrollArea = new Rect(0, 0, 500, 700);      // 100, 100 을 기준으로, 0, 0, 500, 700 만큼의 스크롤 되는 content의 공간을 잡는다.
		vScrollPos = GUI.BeginScrollView(rScrollRect, vScrollPos, rScrollArea);
		GUI.EndScrollView();*/

	//	hSbarValue = GUI.HorizontalScrollbar(new Rect(400, 850, 1000, 950), (float)this.last.bonus_gage / bonusNorm, 300.0f, 0.0f, 3000.0f);
		energy.fillAmount = (float)this.last.bonus_gage / bonusNorm;
		energypercent.text = ((float)this.last.bonus_gage / bonusNorm * 100).ToString() + "%";
	}
	public void print_value(int x, int y, string label, float value)
	{
		// label을 표시.
		GUI.Label(new Rect(x, y, 100, 20), label, guistyle);
		y += 100;
		// 다음 행에 value를 표시.
		GUI.Label(new Rect(x , y, 100, 20), value.ToString(), guistyle);
		y += 22;
	}
	public void addIgniteCount(int count)
	{
		this.last.ignite += count; // 발화 수에 count를 가산.
		this.update_score(); // 점수를 계산.
	}
	public void clearIgniteCount()
	{
		this.last.ignite = 0; // 발화 횟수를 리셋.
	}


    private void update_score()
	{
		this.last.score = this.last.ignite * 10; // 스코어를 갱신.
		
	}
	public void updateTotalScore()
	{
		this.last.total_score += this.last.score; // 합계 스코어를 갱신.
		this.last.bonus_gage = (this.last.total_score - this.scoreManager.GetCurrentScore()) - (bonusNorm * bonusCount);
		Fever_time();
	}

    public int GetTotalScore()
    {
		return this.last.total_score;
    }
	/*public int GetLevelTwoScore(int currentScore)
    {
		
		return 
    }*/
	public bool isGameClear()
	{
		bool is_clear = false;
		// 현재 합계 스코어가 클리어 기준보다 크다면.
		/*if (this.last.total_socre > QUOTA_SCORE)
		{
			is_clear = true;
		}*/
		return (is_clear);
	}

    public void Fever_time()
	{
		if (bonusNorm * (bonusCount + 1) <= this.last.total_score - this.scoreManager.GetCurrentScore())
		{
			animator.SetTrigger("bonus");

			this.last.bonus_gage = 0;
			move_counter.plusLeftMoves(plusMove);
			bonusCount++;
			bonusAudioSource.clip = bonusAudio;      //게이지 다 차면 나는 소리
			//bonusAudioSource.pitch += 0.7f;
			bonusAudioSource.Play();
			//audioSource.pitch -= 0.7f;
		}
	}
}