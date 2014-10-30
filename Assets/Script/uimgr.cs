using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;

public class uimgr : MonoBehaviour {
	Transform _whiteScore;
	Transform _blackScore;
	Transform _State;

	Transform _youwin;
	Transform _cpuwin;
	Transform _draw;


   
	// Use this for initialization
	void Start () {
        SetResolution();

        Screen.sleepTimeout = SleepTimeout.NeverSleep;
        iTween.CameraFadeAdd();
        iTween.CameraFadeFrom(1.0f, 0.5f);

		_whiteScore = transform.FindChild ("whiteScore");
		_blackScore = transform.FindChild ("blackScore");
		_State = transform.FindChild ("uiState");

		_youwin = transform.FindChild ("youwin");
		permanentvariable.ToggleVisibility (_youwin, false);
		_cpuwin = transform.FindChild ("cpuwin");
		permanentvariable.ToggleVisibility (_cpuwin, false);
		_draw = transform.FindChild ("draw");
		permanentvariable.ToggleVisibility (_draw, false);
	}

	void ShowResult(int score)
	{
		if(score > 0) permanentvariable.ToggleVisibility (_cpuwin, true);
		else if(score < 0) permanentvariable.ToggleVisibility (_youwin, true);
		else permanentvariable.ToggleVisibility (_draw, true);
	}

	void SetWhiteScore(int score)
	{
		_whiteScore.guiText.text = "CPU : " + score;
	}

	void SetBlackScore(int score)
	{
		_blackScore.guiText.text = "PLAYER : " + score;
	}

	void SetStateMessage(string txt)
	{
		_State.guiText.text = txt;
	}
	    
    void Update () {
        if (Application.platform == RuntimePlatform.Android)
        {
            if(Input.GetKey(KeyCode.Escape)) Application.Quit();
        }
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