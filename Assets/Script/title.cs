using UnityEngine;
using System.Collections;

public class title : MonoBehaviour {
	
	const int MAX_DEPTH_COUNT = 3;

	GameObject[] imgDifficulty = new GameObject[MAX_DEPTH_COUNT];

	// Use this for initialization
	void Start () {
        SetResolution();
		imgDifficulty[0] = GameObject.Find ("easy");
		imgDifficulty[1] = GameObject.Find ("medium");
		imgDifficulty[2] = GameObject.Find ("hard");

		SetDifficulty(permanentvariable.MAX_DEPTH_IDX);

        iTween.CameraFadeAdd();
        iTween.CameraFadeFrom(1.0f, 0.5f);
	}
	
	// Update is called once per frame
	void Update () {
        if (Application.platform == RuntimePlatform.Android)
        {
            if (Input.GetKey(KeyCode.Escape)) Application.Quit();
        }

        if (Input.GetMouseButtonDown(0))
        {
            Vector3 mousePosition = Input.mousePosition;
            mousePosition.z = 10.0f;
            Vector2 v = Camera.main.ScreenToWorldPoint(mousePosition);
            
            int layerMask = 1 << LayerMask.NameToLayer("UI");
            Collider2D[] col = Physics2D.OverlapPointAll(v, layerMask);
            if (col.Length > 0)
            {
                foreach (Collider2D c in col)
                {
					if (c.collider2D.gameObject.name.Equals("btnEasy"))
					{
						SetDifficulty(0);
					}
					if (c.collider2D.gameObject.name.Equals("btnMedium"))
					{
						SetDifficulty(1);
					}
					if (c.collider2D.gameObject.name.Equals("btnHard"))
					{
						SetDifficulty(2);
					}

                    if (c.collider2D.gameObject.name.Equals("btnStart"))
                    {
                        iTween.CameraFadeTo(iTween.Hash("amount", 1.0f, "time", 0.5f, "oncomplete", "OnButton", "oncompleteparams", true, "oncompletetarget", gameObject));
                    }

                    if (c.collider2D.gameObject.name.Equals("btnExit"))
                    {
						iTween.CameraFadeTo(iTween.Hash("amount", 1.0f, "time", 0.5f, "oncomplete", "OnButton", "oncompleteparams", false, "oncompletetarget", gameObject));
                    }
                }
            }
        }
	}
	void SetDifficulty(int idx)
	{
		permanentvariable.MAX_DEPTH_IDX = idx;
		for (int i=0; i<MAX_DEPTH_COUNT; i++) 
		{
			bool show = ( i == permanentvariable.MAX_DEPTH_IDX) ? true : false;
			permanentvariable.ToggleVisibility(imgDifficulty[i].transform, show);
		}
	}

    void OnButton(bool bGameStart)
    {
        if (bGameStart) Application.LoadLevel("Play");
        else Application.Quit();
    }

	void SetResolution()
	{
		Camera[] objCameras = Camera.allCameras;
		
		float ratioWidth = permanentvariable._displayWidth;
		float ratioHeight = permanentvariable._displayHeight;
		float desireRatio = ratioHeight / ratioWidth;
		float ratio = (float)Screen.height / (float)Screen.width;
		if (ratio < desireRatio) // horizontally long..
		{
			float width = Screen.height * ratioWidth / ratioHeight / Screen.width;
			float left = (1.0f - width) * 0.5f;
			
			foreach (Camera obj in objCameras)
				obj.rect = new Rect(left, 0.0f, width, 1.0f);
			
			GameObject letterboxborder = (GameObject)Resources.Load("Prefabs/letterboxborder");
			GameObject leftborder = (GameObject)Instantiate(letterboxborder);
			leftborder.camera.rect = new Rect(0, 0, left, 1.0f);
			
			GameObject rightborder = (GameObject)Instantiate(letterboxborder);
			rightborder.camera.rect = new Rect(left + width, 0, left, 1.0f);
		}
		else if (ratio > desireRatio) // vertically long..
		{
			float height = Screen.width * ratioHeight / ratioWidth / Screen.height;
			float top = (1.0f - height) * 0.5f;
			
			foreach (Camera obj in objCameras)
				obj.rect = new Rect(0.0f, top, 1.0f, height);
			
			GameObject letterboxborder = (GameObject)Resources.Load("Prefabs/letterboxborder");
			GameObject topborder = (GameObject)Instantiate(letterboxborder);
			topborder.camera.rect = new Rect(0, 0, 1.0f, top);
			
			GameObject bottomborder = (GameObject)Instantiate(letterboxborder);
			bottomborder.camera.rect = new Rect(0.0f, top + height, 1.0f, top);
		}
	}
}
