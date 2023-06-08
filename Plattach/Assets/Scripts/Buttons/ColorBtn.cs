using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ColorBtn : MonoBehaviour
{
    public GameObject scoreManagerObject;
    private ScoreManager scoreManager;
    public Text numberOfColor;
    public Text infoOfColor;
    public bool usingColorItemState;
    public BombBtn bombBtn;

    // Start is called before the first frame update
    void Start()
    {
        scoreManagerObject = GameObject.Find("ScoreManager");
        this.scoreManager = scoreManagerObject.GetComponent<ScoreManager>();
        numberOfColor.text = this.scoreManager.CurrentColorItem.ToString();
        usingColorItemState = false;
        infoOfColor.text = "���ϴ� ���� ���� \n�ٲٴ� ������";

        if (this.scoreManager.CurrentColorItem == 0)
        {
            this.GetComponent<Button>().interactable = false;
        }
    }

    public void OnClick()
    {
        if(!bombBtn.usingBombItemState)
        {
            if (usingColorItemState)
            {
                infoOfColor.text = "���ϴ� ���� ���� \n�ٲٴ� ������";
                usingColorItemState = false;
                Debug.Log("�÷� ��ư ���");
                this.scoreManager.CurrentColorItem++;
                numberOfColor.text = this.scoreManager.CurrentColorItem.ToString();

                Color color = this.GetComponent<Button>().GetComponent<Image>().color;
                color.a = 1.0f;
                this.GetComponent<Button>().GetComponent<Image>().color = color;
            }
            else
            {
                infoOfColor.text = "���ϴ� ���� �ֺ� ���� swap�ϸ� ���� ���ؿ�\n��ư �ٽ� ������ ��� ����";
                usingColorItemState = true;
                Debug.Log("�÷� ��ư ����: " + numberOfColor.text);
                this.scoreManager.CurrentColorItem--;
                numberOfColor.text = this.scoreManager.CurrentColorItem.ToString();

                Color color = this.GetComponent<Button>().GetComponent<Image>().color;
                color.a = 0.5f;
                this.GetComponent<Button>().GetComponent<Image>().color = color;

                /*if (this.scoreManager.CurrentBombItem == 0)
                {
                    this.GetComponent<Button>().interactable = false;
                }*/
            }
        }
    }

    public void UseColorItem()
    {
        usingColorItemState = false;
        infoOfColor.text = "���ϴ� ���� ���� \n�ٲٴ� ������";
        Color color = this.GetComponent<Button>().GetComponent<Image>().color;
        color.a = 1.0f;
        this.GetComponent<Button>().GetComponent<Image>().color = color;
        if (this.scoreManager.CurrentColorItem == 0)
        {
            this.GetComponent<Button>().interactable = false;
        }
    }
}
