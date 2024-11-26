using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Pool;

// 排行榜資料結構
public class LeaderboardData
{
    public int Rank;
    public Sprite Avatar;
    public string Name;
    public int Score;
}

public class LeaderboardManager : MonoBehaviour
{
    [Header("Scroll View Setup")]
    public VerticalLayoutGroup verticalLayoutGroup; // Content 的 VerticalLayoutGroup
    public ScrollRect scrollView; // 滑動視圖
    public RectTransform content; // ScrollView 的 Content
    public GameObject leaderboardCellPrefab; // LeaderboardCell 預製件
    public GameObject leaderboardContentPrefab; // LeaderboardContent 預製件

    [Header("Data Setup")]
    public int totalDataCount = 100; // 總資料數量
    public float cellHeight = 200f; // 每個 Cell 的高度

    private ObjectPool<GameObject> contentPool; // 管理 LeaderboardContent 的物件池
    private List<GameObject> leaderboardCells = new List<GameObject>(); // 所有的 Cell
    private Dictionary<int, GameObject> activeContents = new Dictionary<int, GameObject>(); // 當前可見的 Contents

    private float viewportHeight; // ScrollView 的可視範圍高度
    private List<LeaderboardData> leaderboardData = new List<LeaderboardData>(); // 排行榜數據

    private void Start()
    {
        // 初始化排行榜數據
        InitializeData();

        // 初始化物件池
        contentPool = new ObjectPool<GameObject>(
            createFunc: () =>
            {
                var content = Instantiate(leaderboardContentPrefab);
                content.SetActive(false);
                return content;
            },
            actionOnGet: content => content.SetActive(true),
            actionOnRelease: content => content.SetActive(false),
            actionOnDestroy: content => Destroy(content)
        );

        // 設定 Content 的高度
        // viewportHeight = scrollView.viewport.rect.height;
        viewportHeight = scrollView.GetComponent<RectTransform>().rect.height;
        // 總高度=(Cell 高度 + 間距)×總資料數量−最後一行的間距-可視範圍高度
        float sizeDeltaX = content.sizeDelta.x;
        float sizeDeltaY = (cellHeight + verticalLayoutGroup.spacing) * totalDataCount - viewportHeight;// - verticalLayoutGroup.spacing;
        content.sizeDelta = new Vector2(sizeDeltaX, sizeDeltaY);
        //content.sizeDelta = new Vector2(content.sizeDelta.x, totalDataCount * cellHeight);
        Debug.Log($"content:{content.rect.height}");

        // 創建所有的 LeaderboardCell
        for (int i = 0; i < totalDataCount; i++)
        {
            var cell = Instantiate(leaderboardCellPrefab, content);
            var rect = cell.GetComponent<RectTransform>();
            rect.anchoredPosition = new Vector2(0, -i * cellHeight); // 設置位置
            leaderboardCells.Add(cell);
        }

        // 監聽滑動事件
        scrollView.onValueChanged.AddListener(_ => UpdateVisibleContents());
        UpdateVisibleContents(); // 初始化顯示內容
    }

    private void InitializeData()
    {
        // 模擬排行榜數據
        for (int i = 0; i < totalDataCount; i++)
        {
            leaderboardData.Add(new LeaderboardData
            {
                Rank = i + 1,
                Avatar = null, // 可以填入頭像資源
                Name = $"Player {i + 1}",
                Score = Random.Range(1000, 10000)
            });
        }
    }

    private void UpdateVisibleContents()
    {
        // 獲取有效的 Cell 高度（包括 spacing）
        float effectiveCellHeight = cellHeight + verticalLayoutGroup.spacing;

        // 計算當前滾動位置的索引
        float scrollPos = content.anchoredPosition.y;
        int firstVisibleIndex = Mathf.FloorToInt(scrollPos / effectiveCellHeight);
        int lastVisibleIndex = Mathf.CeilToInt((scrollPos + viewportHeight) / effectiveCellHeight);

        // 確保索引在合理範圍內
        firstVisibleIndex = Mathf.Clamp(firstVisibleIndex, 0, totalDataCount - 1);
        lastVisibleIndex = Mathf.Clamp(lastVisibleIndex, 0, totalDataCount - 1);

        // 回收不可見範圍內的 Content
        var keysToRemove = new List<int>();
        foreach (var pair in activeContents)
        {
            if (pair.Key < firstVisibleIndex || pair.Key > lastVisibleIndex)
            {
                contentPool.Release(pair.Value); // 回收 Content
                keysToRemove.Add(pair.Key); // 記錄需要移除的索引
            }
        }
        foreach (var key in keysToRemove)
        {
            activeContents.Remove(key);
        }

        // 為可見範圍內的 Cell 創建或更新 Content
        for (int i = firstVisibleIndex; i <= lastVisibleIndex; i++)
        {
            if (!activeContents.ContainsKey(i))
            {
                var contentObj = contentPool.Get(); // 從池中取出 Content
                contentObj.transform.SetParent(content.GetChild(i), false);

                // 設置數據
                var contentScript = contentObj.GetComponent<LeaderboardContent>();
                var data = leaderboardData[i];
                contentScript.SetData(i, data.Rank, data.Avatar, data.Name, data.Score);

                activeContents[i] = contentObj; // 記錄該 Content
            }
        }
    }

}
