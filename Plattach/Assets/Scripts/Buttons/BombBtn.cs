using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BombBtn : MonoBehaviour
{
    public GameObject scoreManagerObject;
    private ScoreManager scoreManager;
    public Text numberOfBomb;
    public Text infoOfBomb;
    public bool usingBombItemState;
    public ColorBtn colorBtn;
    private void Start()
    {
        scoreManagerObject = GameObject.Find("ScoreManager");
        this.scoreManager = scoreManagerObject.GetComponent<ScoreManager>();
        numberOfBomb.text = this.scoreManager.CurrentBombItem.ToString();
        usingBombItemState = false;
        infoOfBomb.text = "���ϴ� ���� \n�����ϴ� ������";

        if (this.scoreManager.CurrentBombItem == 0)
        {
            this.GetComponent<Button>().interactable = false;
        }
    }
    public void OnClick()
    {
        if(!colorBtn.usingColorItemState)
        {
            if (usingBombItemState)
            {
                infoOfBomb.text = "���ϴ� ���� \n�����ϴ� ������";
                usingBombItemState = false;
                Debug.Log("��ź ��ư ���");
                this.scoreManager.CurrentBombItem++;
                numberOfBomb.text = this.scoreManager.CurrentBombItem.ToString();

                Color color = this.GetComponent<Button>().GetComponent<Image>().color;
                color.a = 1.0f;
                this.GetComponent<Button>().GetComponent<Image>().color = color;
            }
            else
            {
                infoOfBomb.text = "������ ���� �����ϼ���\n��ư �ٽ� ������ ��� ����";
                usingBombItemState = true;
                Debug.Log("��ź ��ư ����: " + numberOfBomb.text);
                this.scoreManager.CurrentBombItem--;
                numberOfBomb.text = this.scoreManager.CurrentBombItem.ToString();

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
    public void UseBombItem()
    {
        usingBombItemState = false;
        infoOfBomb.text = "���ϴ� ���� \n�����ϴ� ������";
        Color color = this.GetComponent<Button>().GetComponent<Image>().color;
        color.a = 1.0f;
        this.GetComponent<Button>().GetComponent<Image>().color = color;
        if (this.scoreManager.CurrentBombItem == 0)
        {
            this.GetComponent<Button>().interactable = false;
        }
    }
}
