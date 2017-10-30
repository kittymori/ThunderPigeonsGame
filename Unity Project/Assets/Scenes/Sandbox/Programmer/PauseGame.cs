﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using XboxCtrlrInput;

public class PauseGame : MonoBehaviour
{

    public Transform canvas;
   // public Transform canvas2;
    public Transform optionsPanel;
    public Transform pauseMenu;
    public XboxController controller;

    void Start ()
    {
		
	}
	
	void Update ()
    {
        if (XCI.GetButtonDown(XboxButton.Start, controller)||(Input.GetKeyDown(KeyCode.Escape)))
        {
            Pause();
        }
	}

    public void Pause()
    {
        if (canvas.gameObject.activeInHierarchy == false)
        {
            canvas.gameObject.SetActive(true);
            //canvas2.gameObject.SetActive(false);
            optionsPanel.gameObject.SetActive(false);
            pauseMenu.gameObject.SetActive(true);
            Time.timeScale = 0;
        }
        else
        {
            canvas.gameObject.SetActive(false);
           // canvas2.gameObject.SetActive(true);
            optionsPanel.gameObject.SetActive(false);
            pauseMenu.gameObject.SetActive(true);
            Time.timeScale = 1;
        }
    }
}
