using UnityEngine;
using UnityEngine.UI;

public class LeaderboardContent : MonoBehaviour
{
    public Text rankText;
    public Image avatarImage;
    public Text nameText;
    public Text scoreText;

    // �Ω�O���� Cell �������ƾگ���
    public int Index { get; set; }

    public void SetData(int cellIndex, int rank, Sprite avatar, string name, int score)
    {
        Index = cellIndex; // �O������
        rankText.text = rank.ToString("D4");
        avatarImage.sprite = avatar;
        nameText.text = name;
        scoreText.text = score.ToString();
    }
}

