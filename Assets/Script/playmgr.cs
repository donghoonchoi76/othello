using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;

public class playmgr : MonoBehaviour {
    const int MAX_COL = 8;
    const int MAX_STONE = 64;
   
    enum BoardState : byte { None = 0, White, Black };

    BoardState[] _board = new BoardState[MAX_STONE];
    Dictionary<int, GameObject> _stone = new Dictionary<int, GameObject>();
    List<int> _possiblePlace = new List<int>();
    List<GameObject> _objAvailableSpot = new List<GameObject>();

    GameObject _whiteStone;
    GameObject _blackStone;
    GameObject _tag;
	GameObject _uiMgr;

	bool _userTurn;
    bool _cpuHasNoPlace;
	bool _gameOver;

	//
    void Start()
    {
		_gameOver = false;
        _whiteStone = (GameObject)Resources.Load("Prefabs/white");
        _blackStone = (GameObject)Resources.Load("Prefabs/black");
        _tag = (GameObject)Resources.Load("Prefabs/tag");
		_uiMgr = GameObject.Find("UIMgr");

        InitBoard();
        CheckAvailableSpot(BoardState.Black, ref _possiblePlace, ref _board);
        ShowAvailableSpot();
        _cpuHasNoPlace = false;

		_userTurn = true;		
		_uiMgr.SendMessage("SetStateMessage", "");
		ShowScore ();
    }

	void GoTitle(bool b)
	{
		if(b) Application.LoadLevel("Title");
	}
    
    void Update () {
        if (Application.platform == RuntimePlatform.Android)
        {
            if(Input.GetKey(KeyCode.Escape)) Application.Quit();
        }

		if (_gameOver) 
		{
			if (Input.GetMouseButtonDown (0))
			{
				iTween.CameraFadeTo(iTween.Hash("amount", 1.0f, "time", 0.5f, "oncomplete", "GoTitle", "oncompleteparams", true, "oncompletetarget", gameObject));
			}

			return;
		}

		if (_userTurn) 
		{
			if(_possiblePlace.Count > 0)
			{
				if (Input.GetMouseButtonDown (0))
				{
					if(ProcessInput ()) _userTurn = false;
				}
			}
			else _userTurn = false;
		}
		else
		{
			_uiMgr.SendMessage ("SetStateMessage", "");
			TurnCPU();
			_userTurn = true;
		}

		if (_cpuHasNoPlace && _possiblePlace.Count <= 0)
        {
			_uiMgr.SendMessage("SetStateMessage", "Touch Screen");

			int scoreWhite = 0;
			int scoreBlack = 0;
			int score = GetScore(ref _board, ref scoreWhite, ref scoreBlack);
			_uiMgr.SendMessage("ShowResult", score);
			_gameOver = true;
        }
    }

	void ShowScore()
	{
		int scoreWhite = 0;
		int scoreBlack = 0;
		GetScore(ref _board, ref scoreWhite, ref scoreBlack);
		_uiMgr.SendMessage("SetWhiteScore", scoreWhite);
		_uiMgr.SendMessage("SetBlackScore", scoreBlack);
	}

    int GetScore(ref BoardState[] board, ref int white, ref int black)
    {
        white = 0;
        black = 0;
        foreach (BoardState state in board)
        {
            if (state == BoardState.Black) black++;
            else if (state == BoardState.White) white++;
        }

        int score = white - black;

        return score;
    }

    void TurnCPU()
    {
		long sTime = System.DateTime.Now.Ticks;
		int idx = ProcCPU(ref _board, true, -100, 100, permanentvariable.MAX_DEPTH[permanentvariable.MAX_DEPTH_IDX]);
		long cTime = (System.DateTime.Now.Ticks - sTime) / System.TimeSpan.TicksPerMillisecond;
		while (cTime < 1000) 
		{
			cTime = (System.DateTime.Now.Ticks - sTime) / System.TimeSpan.TicksPerMillisecond;
		}

		if (idx >= 0)
        {
			_cpuHasNoPlace = false;
            int ix = GetX(idx);
            int iy = GetY(idx);
            SetBoard(ix, iy, BoardState.White, ref _board);

            GenerateScore(ix, iy, ref _board);
			ShowScore();
        }
		else _cpuHasNoPlace = true;

        CheckAvailableSpot(BoardState.Black, ref _possiblePlace, ref _board);
		if (_possiblePlace.Count > 0) {
			_uiMgr.SendMessage("SetStateMessage", "");
			ShowAvailableSpot();
		}
	}

    int ProcCPU(ref BoardState[] board, bool whiteStone, int a, int b, int depth)
    {
        if (depth <= 0)
        {
            int white = 0;
            int black = 0;
            return GetScore(ref board, ref white, ref black);
        }

        BoardState stone = whiteStone ? BoardState.White : BoardState.Black;

        List<int> possiblePlace = new List<int>();
        CheckAvailableSpot(stone, ref possiblePlace, ref board);
		if (possiblePlace.Count <= 0) 
		{
			if(depth == permanentvariable.MAX_DEPTH[permanentvariable.MAX_DEPTH_IDX]) return -1;

			int white = 0;
			int black = 0;
			return GetScore(ref board, ref white, ref black);
		}

        int ret = -1;
        foreach (int tmpPos in possiblePlace)
        {
            BoardState[] child = board.Clone() as BoardState[];
            child[tmpPos] = stone;
            GenerateScore(GetX(tmpPos), GetY(tmpPos), ref child);
			int v2 = ProcCPU(ref child, !whiteStone, a, b, depth - 1);

			if(whiteStone)
			{
				if(v2 > a)
				{
					a = v2;
					if(depth == permanentvariable.MAX_DEPTH[permanentvariable.MAX_DEPTH_IDX]) ret = tmpPos;
					else ret = a;
				}

				if(b <= a) break;
			}
			else
			{
				if(v2 < b)
				{
					b = v2;
					if(depth == permanentvariable.MAX_DEPTH[permanentvariable.MAX_DEPTH_IDX]) ret = tmpPos;
					else ret = b;
				}

				if(b <= a) break;
			}
        }

        return ret;
    }

    bool ProcessInput()
    {
        Vector3 mousePosition = Input.mousePosition;
        mousePosition.z = 10.0f;
         
        Vector2 v = Camera.main.ScreenToWorldPoint(mousePosition);
        
        int layerMask = 1 << LayerMask.NameToLayer("UI");
        Collider2D[] col = Physics2D.OverlapPointAll(v, layerMask);
        if (col.Length > 0)
        {
			foreach(Collider2D c in col)
			{
				if(c.gameObject.transform.renderer.isVisible)
				{
					HideAvailableSpot();
					int i = System.Convert.ToInt32(c.gameObject.name);
					int ix = GetX(i);
					int iy = GetY(i);
					SetBoard(ix, iy, BoardState.Black, ref _board);
					GenerateScore(ix, iy, ref _board);
					ShowScore();
					return true;
				}
			}
        }

        return false;
    }

    void GenerateScore(int x, int y, ref BoardState[] board)
    {
        int currIndex = x + (MAX_COL * y);
        BoardState currState = board[currIndex];
        
        for (int i = 1; i < MAX_COL; i++)
        {
            int ix = x - i;
            if (ix < 0) break;
            if (board[currIndex - i] == BoardState.None) break;
            if (board[currIndex - i] == currState)
            {
                for (int j = 1; j < i; j++)
                {
                    SetBoard(x - j, y, currState, ref board);
                }
                break;
            }
        }

        for (int i = 1; i < MAX_COL; i++)
        {
            int ix = x + i;
            if (ix >= MAX_COL) break;
            if (board[currIndex + i] == BoardState.None) break;
            if (board[currIndex + i] == currState)
            {
                for (int j = 1; j < i; j++)
                {
                    SetBoard(x + j, y, currState, ref board);
                }
                break;
            }
        }

        for (int i = 1; i < MAX_COL; i++)
        {
            int iy = y - i;
            if (iy < 0) break;

            if (board[currIndex - (MAX_COL * i)] == BoardState.None) break;
            if (board[currIndex - (MAX_COL * i)] == currState)
            {
                for (int j = 1; j < i; j++)
                {
                    SetBoard(x, y - j, currState, ref board);
                }
                break;
            }
        }

        for (int i = 1; i < MAX_COL; i++)
        {
            int iy = y + i;
            if (iy >= MAX_COL) break;

            if (board[currIndex + (MAX_COL * i)] == BoardState.None) break;
            if (board[currIndex + (MAX_COL * i)] == currState)
            {
                for (int j = 1; j < i; j++)
                {
                    SetBoard(x, y + j, currState, ref board);
                }
                break;
            }
        }

        for (int i = 1; i < MAX_COL; i++)
        {
            int ix = x - i;
            int iy = y - i;
            if (ix < 0 || iy < 0) break;

            if (board[currIndex - (MAX_COL * i) - i] == BoardState.None) break;
            if (board[currIndex - (MAX_COL * i) - i] == currState)
            {
                for (int j = 1; j < i; j++)
                {
                    SetBoard(x - j, y - j, currState, ref board);
                }
                break;
            }
        }

        for (int i = 1; i < MAX_COL; i++)
        {
            int ix = x + i;
            int iy = y - i;
            if (ix >= MAX_COL || iy < 0) break;

            if (board[currIndex - (MAX_COL * i) + i] == BoardState.None) break;
            if (board[currIndex - (MAX_COL * i) + i] == currState)
            {
                for (int j = 1; j < i; j++)
                {
                    SetBoard(x + j, y - j, currState, ref board);
                }
                break;
            }
        }

        for (int i = 1; i < MAX_COL; i++)
        {
            int ix = x + i;
            int iy = y + i;
            if (ix >= MAX_COL || iy >= MAX_COL) break;

            if (board[currIndex + (MAX_COL * i) + i] == BoardState.None) break;
            if (board[currIndex + (MAX_COL * i) + i] == currState)
            {
                for (int j = 1; j < i; j++)
                {
                    SetBoard(x + j, y + j, currState, ref board);
                }
                break;
            }
        }

        for (int i = 1; i < MAX_COL; i++)
        {
            int ix = x - i;
            int iy = y + i;
            if (ix < 0 || iy >= MAX_COL) break;

            if (board[currIndex + (MAX_COL * i) - i] == BoardState.None) break;
            if (board[currIndex + (MAX_COL * i) - i] == currState)
            {
                for (int j = 1; j < i; j++)
                {
                    SetBoard(x - j, y + j, currState, ref board);
                }
                break;
            }
        }
	}

    void InitBoard()
    {
        for (int i = 0; i < MAX_STONE; i++)
        {
            _board[i] = BoardState.None;
        }

        SetBoard(3, 3, BoardState.White, ref _board);
        SetBoard(4, 4, BoardState.White, ref _board);
        SetBoard(3, 4, BoardState.Black, ref _board);
        SetBoard(4, 3, BoardState.Black, ref _board);
    }

    void CheckAvailableSpot(BoardState state, ref List<int> list, ref BoardState[] board)
    {
        list.Clear();

        int count = -1;
        foreach(BoardState tmpState in board)
        {
            count++;
            if(tmpState != state) continue;

            int x = GetX(count);
            int y = GetY(count);

            for (int i = 1; i < MAX_COL; i++)
            {
                int ix = x - i;
                if (ix < 0) break;
                if (board[count - i] == state) break;
                if (board[count - i] == BoardState.None)
                {
                    if (i != 1) list.Add(count - i);
                    break;
                }
            }

            for (int i = 1; i < MAX_COL; i++)
            {
                int ix = x + i;
                if (ix >= MAX_COL) break;
                if (board[count + i] == state) break;
                if (board[count + i] == BoardState.None)
                {
                    if (i != 1) list.Add(count + i);
                    break;
                }
            }

            for (int i = 1; i < MAX_COL; i++)
            {
                int iy = y - i;
                if (iy < 0) break;

                if (board[count - (MAX_COL * i)] == state) break;
                if (board[count - (MAX_COL * i)] == BoardState.None)
                {
                    if (i != 1) list.Add(count - (MAX_COL * i));
                    break;
                }
            }

            for (int i = 1; i < MAX_COL; i++)
            {
                int iy = y + i;
                if (iy >= MAX_COL) break;

                if (board[count + (MAX_COL * i)] == state) break;
                if (board[count + (MAX_COL * i)] == BoardState.None)
                {
                    if (i != 1) list.Add(count + (MAX_COL * i));
                    break;
                }
            }

            for (int i = 1; i < MAX_COL; i++)
            {
                int ix = x - i;
                int iy = y - i;
                if (ix < 0 || iy < 0) break;

                if (board[count - (MAX_COL * i) - i] == state) break;
                if (board[count - (MAX_COL * i) - i] == BoardState.None)
                {
                    if (i != 1) list.Add(count - (MAX_COL * i) - i);
                    break;
                }
            }

            for (int i = 1; i < MAX_COL; i++)
            {
                int ix = x + i;
                int iy = y - i;
                if (ix >= MAX_COL || iy < 0) break;

                if (board[count - (MAX_COL * i) + i] == state) break;
                if (board[count - (MAX_COL * i) + i] == BoardState.None)
                {
                    if (i != 1) list.Add(count - (MAX_COL * i) + i);
                    break;
                }
            }

            for (int i = 1; i < MAX_COL; i++)
            {
                int ix = x + i;
                int iy = y + i;
                if (ix >= MAX_COL || iy >= MAX_COL) break;

                if (board[count + (MAX_COL * i) + i] == state) break;
                if (board[count + (MAX_COL * i) + i] == BoardState.None)
                {
                    if (i != 1) list.Add(count + (MAX_COL * i) + i);
                    break;
                }
            }

            for (int i = 1; i < MAX_COL; i++)
            {
                int ix = x - i;
                int iy = y + i;
                if (ix < 0 || iy >= MAX_COL) break;

                if (board[count + (MAX_COL * i) - i] == state) break;
                if (board[count + (MAX_COL * i) - i] == BoardState.None)
                {
                    if (i != 1) list.Add(count + (MAX_COL * i) - i);
                    break;
                }
            }
        }
    }

    void SetBoard(int x, int y, BoardState state, ref BoardState[] board)
    {
        int index = GetIndex(x, y);
        if (board[index] == state) return;

        board[index] = state;

        if (board == _board)
        {
            if (_stone.ContainsKey(index))
            {
                Destroy(_stone[index]);
                _stone.Remove(index);
            }

            switch (state)
            {
                case BoardState.Black:
                    _blackStone.transform.position = GetDisplayPos(x, y);
                    _stone.Add(index, Instantiate(_blackStone) as GameObject);
                    break;

                case BoardState.White:
                    _whiteStone.transform.position = GetDisplayPos(x, y);
                    _stone.Add(index, Instantiate(_whiteStone) as GameObject);
                    break;
            }
        }
    }

    int GetIndex(int x, int y)
    {
        return (y * MAX_COL + x);
    }

    int GetX(int index)
    {
        return (index % MAX_COL);
    }

    int GetY(int index)
    {
        return (index / MAX_COL);
    }

    Vector3 GetDisplayPos(int x, int y)
    {
        Vector3 pos = new Vector3((x * permanentvariable._stoneSize) - permanentvariable._displayWidth, permanentvariable._displayHeight - (y * permanentvariable._stoneSize), 0.0f);
        return pos;
    }

    void ShowAvailableSpot()
    {
        for(int i=0; i< _objAvailableSpot.Count; i++)
        {
            if (i >= _possiblePlace.Count)
            {
                permanentvariable.ToggleVisibility(_objAvailableSpot[i].transform, false);
            }
            else
            {
                int x = GetX(_possiblePlace[i]);
                int y = GetY(_possiblePlace[i]);
                _objAvailableSpot[i].transform.position = GetDisplayPos(x, y);

                _objAvailableSpot[i].name = _possiblePlace[i].ToString();
                permanentvariable.ToggleVisibility(_objAvailableSpot[i].transform, true);            
            }
        }

        for (int i = _objAvailableSpot.Count; i < _possiblePlace.Count; i++)
        {
            int x = GetX(_possiblePlace[i]);
            int y = GetY(_possiblePlace[i]);
            _tag.transform.position = GetDisplayPos(x, y);
            GameObject obj = Instantiate(_tag) as GameObject;
            obj.name = _possiblePlace[i].ToString();
            _objAvailableSpot.Add(obj);
        }
    }

    void HideAvailableSpot()
    {
        for (int i = 0; i < _objAvailableSpot.Count; i++)
        {
            permanentvariable.ToggleVisibility(_objAvailableSpot[i].transform, false);
        }
    }
}