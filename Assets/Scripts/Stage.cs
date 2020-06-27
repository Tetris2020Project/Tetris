using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Stage : MonoBehaviour
{
    [Header("Editor Objects")] //유니티 에디터를 위한 특수 기능
    public GameObject tilePrefab;
    public Transform backgroundNode;
    public Transform boardNode;
    public Transform tetrominoNode;
    public GameObject gameoverPanel;

    [Header("Game Settings")]
    [Range(4, 40)]
    public int boardWidth = 10;
    [Range(5, 20)]
    public int boardHeight = 20;
    public float fallTime = 1.0f;
    //블록 offset 변수
    public Vector3Int[,] JLSTZ_OFFSET_DATA { get; private set; }
    public Vector3Int[,] I_OFFSET_DATA { get; private set; }
    public Vector3Int[,] O_OFFSET_DATA { get; private set; }
    public bool isClockwise = false;
    public int oldrotindex = 0, newrotindex = 0;
    public bool[] tetrominoCycle = { true, true, true, true, true, true, true };
    public int tetrominoCycleCount = 0;

    private int halfWidth;
    private int halfHeight;
    private float nextFallTime;
    float time, starTime;

    public int block_num; //다음 블록의 숫자
    public int index;   //MakeBlock()의 index를 다른 함수에서도 쓰도록 밖으로 빼옴

    private void Start()
    {
        Score.currentScore = 0;// 현재 점수
        Timer.timerTxt = ""; //타이머 선언

        gameoverPanel.SetActive(false);
        halfWidth = Mathf.RoundToInt(boardWidth * 0.5f);
        halfHeight = Mathf.RoundToInt(boardHeight * 0.5f);

        nextFallTime = Time.time + fallTime;
        Background();

        //블록 offset 설정
        JLSTZ_OFFSET_DATA = new Vector3Int[5, 4];
        JLSTZ_OFFSET_DATA[0, 0] = new Vector3Int(0, 0, 0);
        JLSTZ_OFFSET_DATA[0, 1] = new Vector3Int(0, 0, 0);
        JLSTZ_OFFSET_DATA[0, 2] = new Vector3Int(0, 0, 0);
        JLSTZ_OFFSET_DATA[0, 3] = new Vector3Int(0, 0, 0);

        JLSTZ_OFFSET_DATA[1, 0] = new Vector3Int(0, 0, 0);
        JLSTZ_OFFSET_DATA[1, 1] = new Vector3Int(1, 0, 0);
        JLSTZ_OFFSET_DATA[1, 2] = new Vector3Int(0, 0, 0);
        JLSTZ_OFFSET_DATA[1, 3] = new Vector3Int(-1, 0, 0);

        JLSTZ_OFFSET_DATA[2, 0] = new Vector3Int(0, 0, 0);
        JLSTZ_OFFSET_DATA[2, 1] = new Vector3Int(1, -1, 0);
        JLSTZ_OFFSET_DATA[2, 2] = new Vector3Int(0, 0, 0);
        JLSTZ_OFFSET_DATA[2, 3] = new Vector3Int(-1, -1, 0);

        JLSTZ_OFFSET_DATA[3, 0] = new Vector3Int(0, 0, 0);
        JLSTZ_OFFSET_DATA[3, 1] = new Vector3Int(0, 2, 0);
        JLSTZ_OFFSET_DATA[3, 2] = new Vector3Int(0, 0, 0);
        JLSTZ_OFFSET_DATA[3, 3] = new Vector3Int(0, 2, 0);

        JLSTZ_OFFSET_DATA[4, 0] = new Vector3Int(0, 0, 0);
        JLSTZ_OFFSET_DATA[4, 1] = new Vector3Int(1, 2, 0);
        JLSTZ_OFFSET_DATA[4, 2] = new Vector3Int(0, 0, 0);
        JLSTZ_OFFSET_DATA[4, 3] = new Vector3Int(-1, 2, 0);

        I_OFFSET_DATA = new Vector3Int[5, 4];
        I_OFFSET_DATA[0, 0] = new Vector3Int(0, 0, 0);
        I_OFFSET_DATA[0, 1] = new Vector3Int(-1, 0, 0);
        I_OFFSET_DATA[0, 2] = new Vector3Int(-1, 1, 0);
        I_OFFSET_DATA[0, 3] = new Vector3Int(0, 1, 0);

        I_OFFSET_DATA[1, 0] = new Vector3Int(-1, 0, 0);
        I_OFFSET_DATA[1, 1] = new Vector3Int(0, 0, 0);
        I_OFFSET_DATA[1, 2] = new Vector3Int(1, 1, 0);
        I_OFFSET_DATA[1, 3] = new Vector3Int(0, 1, 0);

        I_OFFSET_DATA[2, 0] = new Vector3Int(2, 0, 0);
        I_OFFSET_DATA[2, 1] = new Vector3Int(0, 0, 0);
        I_OFFSET_DATA[2, 2] = new Vector3Int(-2, 1, 0);
        I_OFFSET_DATA[2, 3] = new Vector3Int(0, 1, 0);

        I_OFFSET_DATA[3, 0] = new Vector3Int(-1, 0, 0);
        I_OFFSET_DATA[3, 1] = new Vector3Int(0, 1, 0);
        I_OFFSET_DATA[3, 2] = new Vector3Int(1, 0, 0);
        I_OFFSET_DATA[3, 3] = new Vector3Int(0, -1, 0);

        I_OFFSET_DATA[4, 0] = new Vector3Int(2, 0, 0);
        I_OFFSET_DATA[4, 1] = new Vector3Int(0, -2, 0);
        I_OFFSET_DATA[4, 2] = new Vector3Int(-2, 0, 0);
        I_OFFSET_DATA[4, 3] = new Vector3Int(0, 2, 0);

        O_OFFSET_DATA = new Vector3Int[1, 4];
        O_OFFSET_DATA[0, 0] = new Vector3Int(0, 0, 0);
        O_OFFSET_DATA[0, 1] = new Vector3Int(0, -1, 0);
        O_OFFSET_DATA[0, 2] = new Vector3Int(-1, -1, 0);
        O_OFFSET_DATA[0, 3] = new Vector3Int(-1, 0, 0);

        // 바닥에 닿을 경우 처리
        for (int i = 0; i < boardHeight; ++i)
        {
            var col = new GameObject((boardHeight - i - 1).ToString());
            col.transform.position = new Vector3(0, halfHeight - i, 0);
            col.transform.parent = boardNode;// 보드노드의 자식으로 만들어 줌
        }
        block_num = Random.Range(0, 7); //0부터 6까지 중 하나의 숫자가 임의로 출현
        while (tetrominoCycle[block_num] == false) block_num = Random.Range(0, 7);
        MakeBlock(block_num);
        block_num = Random.Range(0, 7); //0부터 6까지 중 하나의 숫자가 임의로 출현
        while (tetrominoCycle[block_num] == false) block_num = Random.Range(0, 7);
    }

    void Update() //실시간 이동
    {
        if (gameoverPanel.activeSelf)
        {
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                UnityEngine.SceneManagement.SceneManager.LoadScene(0);
            }
        }
        else
        {
            Vector3 b = Vector3.zero; //테트로미노의 이동여부 (0,0,0)
            bool isRotate = false; //회전을 모두 마쳤는가? 1. 아래키를 누를 때 2.위치가 고정되었을 때

            if (Input.GetKeyDown(KeyCode.LeftArrow)) //왼쪽 화살표를 누르면
            {
                b.x = -1;
            }
            else if (Input.GetKeyDown(KeyCode.RightArrow)) //오른쪽 화살표를 누르면
            {
                b.x = 1;
            }
            else if (Input.GetKeyDown(KeyCode.UpArrow)) //위 화살표를 누르면
            {
                isClockwise = true; //시계방향
                isRotate = true; //회전
            }
            else if (Input.GetKeyDown(KeyCode.X)) //X를 누르면
            {
                isClockwise = false; //반시계방향
                isRotate = true; //회전
            }
            else if (Input.GetKeyDown(KeyCode.DownArrow)) //아래 화살표를 누르면
            {
                b.y = -1; //아래 방향으로 이동
                nextFallTime = Time.time + fallTime;
            }

            //// Add New Function
            if (Input.GetKeyDown(KeyCode.Space)) //추가기능 space
            {
                while (MoveBlock(Vector3.down, false))
                {
                }
            }

            // 자동으로 아래로 떨어지기
            if (Time.time > nextFallTime)
            {
                nextFallTime = Time.time + fallTime;
                b = Vector3.down;
                isRotate = false;
            }

            if (b != Vector3.zero || isRotate)
            {
                MoveBlock(b, isRotate); //테트로미노를 이동
            }
        }

    }

    bool Border(Transform root) //경계선에 위치하면 true, 그렇지 않으면 false
    {
        for (int i = 0; i < root.childCount; ++i)
        {
            var node = root.GetChild(i);
            int x = Mathf.RoundToInt(node.transform.position.x + halfWidth);
            int y = Mathf.RoundToInt(node.transform.position.y + halfHeight - 1);

            if (x < 0 || x > boardWidth - 1) //0 ~ boardWidth-1 까지가 안전한 위치
                return false;

            if (y < 0)
                return false;

            var column = boardNode.Find(y.ToString()); // y위치에 해당되는 노드를 가져옴

            if (column != null && column.Find(x.ToString()) != null)
                return false;
        }
        return true;
    }
    bool OffsetTest()
    {
        var node = tetrominoNode;
        Vector3Int offset1, offset2;
        Vector3Int[,] offset;

        if (index == 0) offset = I_OFFSET_DATA; //I블록을 위한 별도의 offset
        else if (index == 2) offset = O_OFFSET_DATA;    //o블록을 위한 별도의 offset
        else offset = JLSTZ_OFFSET_DATA;    //나머지 블록을 위한 공통의 offset

        if (isClockwise) node.transform.rotation *= Quaternion.Euler(0, 0, -90);
        else node.transform.rotation *= Quaternion.Euler(0, 0, 90);

        for (int i = 0; i < (index == 2 ? 1 : 5); i++)
        {
            offset1 = offset[i, oldrotindex];
            offset2 = offset[i, newrotindex];

            node.transform.position += offset1 - offset2;
            if (!Border(node))
            {
                node.transform.position += offset2 - offset1;
            }
            else
            {
                tetrominoNode = node;
                return true;
            }
        }
        return false;
    }

    bool MoveBlock(Vector3 moveblock, bool isRotate) // 블록의 움직임이 가능하면 true, 그렇지 않으면 false
    {
        Vector3 pre_position = tetrominoNode.transform.position;
        Quaternion pre_rotation = tetrominoNode.transform.rotation;

        tetrominoNode.transform.position += moveblock;
        if (isRotate)
        {
            if (isClockwise)
            {
                if (newrotindex == 3) newrotindex = 0;
                else newrotindex++;

                if (!OffsetTest())
                {
                    tetrominoNode.transform.rotation *= Quaternion.Euler(0, 0, 90);
                    newrotindex = oldrotindex;
                }
                else oldrotindex = newrotindex;
            }
            else
            {
                if (newrotindex == 0) newrotindex = 3;
                else newrotindex--;

                if (!OffsetTest())
                {
                    tetrominoNode.transform.rotation *= Quaternion.Euler(0, 0, -90);
                    newrotindex = oldrotindex;
                }
                else oldrotindex = newrotindex;
            }
        }
        if (!Border(tetrominoNode))
        {
            tetrominoNode.transform.position = pre_position;
            tetrominoNode.transform.rotation = pre_rotation;

            if ((int)moveblock.y == -1 && (int)moveblock.x == 0 && isRotate == false)
            {
                AddToBoard(tetrominoNode);
                ClearBlock();
                oldrotindex = 0;
                newrotindex = 0;

                MakeBlock(block_num);
                block_num = Random.Range(0, 7); //0부터 6까지 중 하나의 숫자가 임의로 출현
                while (tetrominoCycle[block_num] == false) block_num = Random.Range(0, 7);
            }

            if (!Border(tetrominoNode))
            {
                gameoverPanel.SetActive(true);
            }

            return false; // 들어올 수 없는 위치에 들어오면 false
        }
        return true;
    }

    void AddToBoard(Transform root) // 테트로미노를 보드에 위치를 확정
    {
        while (root.childCount > 0) //테트로미노의 모든 자식 타일을 가져오는 코드
        {
            var node = root.GetChild(0);
            // 유니티 좌표를 테트리스 좌표로 변경
            int x = Mathf.RoundToInt(node.transform.position.x + halfWidth);
            int y = Mathf.RoundToInt(node.transform.position.y + halfHeight - 1);

            node.parent = boardNode.Find(y.ToString());
            node.name = x.ToString();
        }
    }

    void ClearBlock() // 모든 행에 블록이 있으면 행을 삭제하고 점수에 추가
    {
        bool cleared = false; //행이 지워졌는가

        // 완성된 행 == 행의 자식 갯수가 가로 크기
        foreach (Transform column in boardNode)
        {
            if (column.childCount == boardWidth)
            {
                foreach (Transform tile in column) //해당행의 블록을 제거
                {
                    Destroy(tile.gameObject);
                }
                Score.currentScore += 100; // 점수 추가
                column.DetachChildren();
                cleared = true;
            }
        }
        if (cleared)
        {
            for (int i = 1; i < boardNode.childCount; ++i)
            {
                var column = boardNode.Find(i.ToString());

                // 이미 비어 있는 행은 무시
                if (column.childCount == 0)
                    continue;

                int emptyCol = 0;
                int idx = i - 1;
                while (idx >= 0) //비어있는 행의 개수
                {
                    if (boardNode.Find(idx.ToString()).childCount == 0)
                    {
                        emptyCol++;
                    }
                    idx--;
                }

                if (emptyCol > 0) // 비어있는 행만큼 아래로 당겨
                {
                    var targetColumn = boardNode.Find((i - emptyCol).ToString());

                    while (column.childCount > 0)
                    {
                        Transform tile = column.GetChild(0);
                        tile.parent = targetColumn;
                        tile.transform.position += new Vector3(0, -emptyCol, 0);
                    }
                    column.DetachChildren();
                }
            }
        }
    }

    // ===== 기본셋팅 ===== //
    // 1. 타일 생성하기
    Tile MakeTile(Transform parent, Vector2 position, Color color, int order = 1)
    {
        // Instntiate를 사용해 프리팹으로부터 복제한 후 부모를 지정하고 색상 및 정렬 순서를 지정
        var go = Instantiate(tilePrefab);

        go.transform.parent = parent;
        go.transform.localPosition = position;

        var tile = go.GetComponent<Tile>();
        tile.color = color;
        tile.sortingOrder = order;

        return tile;
    }

    //2. 배경 생성하기
    void Background()
    {
        Color color = Color.gray;

        // 타일 보드
        // 가로 : -halfWidth ~ halfWidth
        // 세로 : halfHeight ~ -halfHeight

        color.a = 0.5f;
        for (int x = -halfWidth; x < halfWidth; ++x)
        {
            for (int y = halfHeight; y > -halfHeight; --y)
            {
                MakeTile(backgroundNode, new Vector2(x, y), color, 0);
            }
        }

        Color32 e_color;
        e_color = new Color32(83, 97, 115, 255);

        // 좌우 테두리
        for (int y = halfHeight; y > -halfHeight; --y)
        {
            MakeTile(backgroundNode, new Vector2(-halfWidth - 1, y), e_color, 0);
            MakeTile(backgroundNode, new Vector2(halfWidth, y), e_color, 0);
        }

        // 아래 테두리
        for (int x = -halfWidth - 1; x <= halfWidth; ++x)
        {
            MakeTile(backgroundNode, new Vector2(x, -halfHeight), e_color, 0);
        }
    }

    // 3. 블록 생성하기
    void MakeBlock(int blkno)
    {
        index = block_num;
        tetrominoCycle[index] = false; tetrominoCycleCount++;
        if (tetrominoCycleCount == 7)
        {
            tetrominoCycleCount = 0;
            for (int k = 0; k < 7; k++)
                tetrominoCycle[k] = true;
        }
        int bomb_pb = Random.Range(0, 100);
        Color32 color = Color.white;
        Color32 bomb_color = Color.black;

        //기본 위치 지정
        tetrominoNode.rotation = Quaternion.identity;
        tetrominoNode.position = new Vector2(0, halfHeight);

        switch (index) //랜덤으로 생성된 숫자로 블록의 색과 모양을 정함
        {
            // I형 : 빨강색
            case 0:
                color = new Color32(235, 51, 35, 255);
                if (bomb_pb <= 1)
                {
                    MakeTile(tetrominoNode, new Vector2(-1f, 0.0f), bomb_color);
                    MakeTile(tetrominoNode, new Vector2(0f, 0.0f), bomb_color);
                    MakeTile(tetrominoNode, new Vector2(1f, 0.0f), bomb_color);
                    MakeTile(tetrominoNode, new Vector2(2f, 0.0f), bomb_color);
                }
                else
                {
                    MakeTile(tetrominoNode, new Vector2(-1f, 0.0f), color);
                    MakeTile(tetrominoNode, new Vector2(0f, 0.0f), color);
                    MakeTile(tetrominoNode, new Vector2(1f, 0.0f), color);
                    MakeTile(tetrominoNode, new Vector2(2f, 0.0f), color);
                }
                break;

            // T형 : 주황색
            case 1:
                color = new Color32(243, 168, 59, 255);
                if (bomb_pb <= 1)
                {
                    MakeTile(tetrominoNode, new Vector2(-1f, 0f), bomb_color);
                    MakeTile(tetrominoNode, new Vector2(0f, 0f), bomb_color);
                    MakeTile(tetrominoNode, new Vector2(1f, 0f), bomb_color);
                    MakeTile(tetrominoNode, new Vector2(0f, 1f), bomb_color);
                }
                else
                {
                    MakeTile(tetrominoNode, new Vector2(-1f, 0f), color);
                    MakeTile(tetrominoNode, new Vector2(0f, 0f), color);
                    MakeTile(tetrominoNode, new Vector2(1f, 0f), color);
                    MakeTile(tetrominoNode, new Vector2(0f, 1f), color);
                }
                break;

            // ㅁ형 : 노란색
            case 2:
                color = new Color32(255, 253, 84, 255);
                if (bomb_pb <= 1)
                {
                    MakeTile(tetrominoNode, new Vector2(0f, 0f), bomb_color);
                    MakeTile(tetrominoNode, new Vector2(1f, 0f), bomb_color);
                    MakeTile(tetrominoNode, new Vector2(0f, 1f), bomb_color);
                    MakeTile(tetrominoNode, new Vector2(1f, 1f), bomb_color);
                }
                else
                {
                    MakeTile(tetrominoNode, new Vector2(0f, 0f), color);
                    MakeTile(tetrominoNode, new Vector2(1f, 0f), color);
                    MakeTile(tetrominoNode, new Vector2(0f, 1f), color);
                    MakeTile(tetrominoNode, new Vector2(1f, 1f), color);
                }
                break;

            // ㄴ형 : 초록색
            case 3:
                color = new Color32(117, 250, 76, 255);
                if (bomb_pb <= 1)
                {
                    MakeTile(tetrominoNode, new Vector2(-1f, 0.0f), bomb_color);
                    MakeTile(tetrominoNode, new Vector2(0f, 0.0f), bomb_color);
                    MakeTile(tetrominoNode, new Vector2(1f, 0.0f), bomb_color);
                    MakeTile(tetrominoNode, new Vector2(1f, 1.0f), bomb_color);
                }
                else
                {
                    MakeTile(tetrominoNode, new Vector2(-1f, 0.0f), color);
                    MakeTile(tetrominoNode, new Vector2(0f, 0.0f), color);
                    MakeTile(tetrominoNode, new Vector2(1f, 0.0f), color);
                    MakeTile(tetrominoNode, new Vector2(1f, 1.0f), color);
                }
                break;

            // ㄱ형 : 파란색
            case 4:
                color = new Color32(0, 33, 245, 255);
                if (bomb_pb <= 1)
                {
                    MakeTile(tetrominoNode, new Vector2(-1f, 0.0f), bomb_color);
                    MakeTile(tetrominoNode, new Vector2(0f, 0.0f), bomb_color);
                    MakeTile(tetrominoNode, new Vector2(1f, 0.0f), bomb_color);
                    MakeTile(tetrominoNode, new Vector2(-1f, 1.0f), bomb_color);
                }
                else
                {
                    MakeTile(tetrominoNode, new Vector2(-1f, 0.0f), color);
                    MakeTile(tetrominoNode, new Vector2(0f, 0.0f), color);
                    MakeTile(tetrominoNode, new Vector2(1f, 0.0f), color);
                    MakeTile(tetrominoNode, new Vector2(-1f, 1.0f), color);
                }
                break;

            // S형 : 하늘색
            case 5:
                color = new Color32(115, 251, 253, 255);
                if (bomb_pb <= 1)
                {
                    MakeTile(tetrominoNode, new Vector2(-1f, -1f), bomb_color);
                    MakeTile(tetrominoNode, new Vector2(0f, -1f), bomb_color);
                    MakeTile(tetrominoNode, new Vector2(0f, 0f), bomb_color);
                    MakeTile(tetrominoNode, new Vector2(1f, 0f), bomb_color);
                }
                else
                {
                    MakeTile(tetrominoNode, new Vector2(-1f, -1f), color);
                    MakeTile(tetrominoNode, new Vector2(0f, -1f), color);
                    MakeTile(tetrominoNode, new Vector2(0f, 0f), color);
                    MakeTile(tetrominoNode, new Vector2(1f, 0f), color);
                }
                break;

            // 2형 : 자주색
            case 6:
                color = new Color32(155, 47, 246, 255);
                if (bomb_pb <= 1)
                {
                    MakeTile(tetrominoNode, new Vector2(-1f, 1f), bomb_color);
                    MakeTile(tetrominoNode, new Vector2(0f, 1f), bomb_color);
                    MakeTile(tetrominoNode, new Vector2(0f, 0f), bomb_color);
                    MakeTile(tetrominoNode, new Vector2(1f, 0f), bomb_color);
                }
                else
                {
                    MakeTile(tetrominoNode, new Vector2(-1f, 1f), color);
                    MakeTile(tetrominoNode, new Vector2(0f, 1f), color);
                    MakeTile(tetrominoNode, new Vector2(0f, 0f), color);
                    MakeTile(tetrominoNode, new Vector2(1f, 0f), color);
                }
                break;
        }
    }
}

