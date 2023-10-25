﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Socket
using System;
using System.Text;
using System.Threading;
using System.Net;
using System.Net.Sockets;

// Model
using Live2D;
using Live2D.Cubism.Core;
using Live2D.Cubism.Framework;

// Windows setting
using System.Runtime.InteropServices;

/* 
 * Author: Kennard Wang 
 * GitHiub: https://github.com/KennardWang
*/

public class Momose : MonoBehaviour
{
    // Window setting, reference: https://blog.csdn.net/qq_39097425/article/details/81664448
    //[DllImport("user32.dll")]
    //static extern IntPtr SetWindowLong(IntPtr hwnd, int _nIndex, int dwNewLong);
    //[DllImport("user32.dll")]
    //static extern bool SetWindowPos(IntPtr hWnd, int hWndInsertAfter, int X, int Y, int cx, int cy, uint uFlags);
    //[DllImport("user32.dll")]
    //static extern IntPtr GetForegroundWindow();


    // Player setting
    //const uint SHOWWINDOW = 0x0040;
    //const int STYLE = -16;
    //const int BORDER = 1;
    //const int TOP = -1;

    //int GUIwidth = 480;
    //int GUIheight = 360;
    //int GUIposX = 1450;
    //int GUIposY = 650;


    // Socket connect, reference: https://blog.csdn.net/u012234115/article/details/46481845
    Socket clientSocket;
    Socket serverSocket;

    // Receive Data
    byte[] recData = new byte[1024];

    // Model parameter, reference: https://docs.live2d.com/cubism-sdk-tutorials/about-parameterupdating-of-model/?locale=ja
    private CubismModel Model;
    private CubismParameter parameter;  // do not use array, otherwise it won't update
    private float t1;  // time controller for breath
    private float t2;  // time controller for hands
    private float angleX;  // head angle
    private float angleY;  // head angle
    private float angleZ;  // head angle
    private float eyeOpenLeft;
    private float eyeOpenRight;
    private float eyeBallX;
    private float eyeBallY;
    private float eyebrowLeft;
    private float eyebrowRight;
    private float mouthOpen;
    private float mouthWidth;

    void init()
    {
        // Initialize IP and port
        const string IP = "127.0.0.1";
        int PORT = PlayerPrefs.GetInt("port");
        Debug.Log(PORT);

        // Socket initialization
        serverSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        IPEndPoint ipEndPoint = new IPEndPoint(IPAddress.Parse(IP), PORT);
        serverSocket.Bind(ipEndPoint);
        serverSocket.Listen(100);

        // start a new thread to update parameters
        Thread connect = new Thread(new ThreadStart(paraUpdate));
        connect.Start();

        // Model initialization
        Model = this.FindCubismModel();
        t1 = t2 = 0.0f;
        eyeOpenLeft = eyeOpenRight = 1.0f;
        eyeBallX = eyeBallY = 0.0f;
        mouthWidth = 1.0f;
        mouthOpen = 0.0f;
        eyebrowLeft = eyebrowRight = -1.0f;
    }

    // Start is called before the first frame update
    void Start()
    {
        //Screen.SetResolution(GUIwidth, GUIheight, false);  // set resolution
        //StartCoroutine("displaySetting");

        init();
    }

    // Update is called once per frame, please use LateUpdate() instead of Update(), otherwise it will stuck
    void LateUpdate()
    {
        // breath
        t1 += (Time.deltaTime * 3f);
        parameter = Model.Parameters[23];
        parameter.Value = Mathf.Sin(t1) * 0.5f + 0.5f;

        // hands
        t2 += (Time.deltaTime * 2f);

        // left hand
        parameter = Model.Parameters[32];
        parameter.Value = Mathf.Sin(t2) * 1.0f;

        // right hand
        parameter = Model.Parameters[33];
        parameter.Value = Mathf.Sin(t2) * 1.0f;

        // update yaw
        parameter = Model.Parameters[0];
        parameter.Value = angleX;

        // update pitch
        parameter = Model.Parameters[1];
        parameter.Value = angleY;

        // update roll
        parameter = Model.Parameters[2];
        parameter.Value = angleZ;

        // update eyes
        parameter = Model.Parameters[4];  // left
        parameter.Value = eyeOpenLeft;

        parameter = Model.Parameters[6];  // right
        parameter.Value = eyeOpenRight;

        // update eyeballs
        parameter = Model.Parameters[8];  // X axis
        parameter.Value = eyeBallX;

        parameter = Model.Parameters[9];  // Y axis
        parameter.Value = eyeBallY;

        // update eyebrows
        parameter = Model.Parameters[10];  // left
        parameter.Value = eyebrowLeft;

        parameter = Model.Parameters[11];  // right
        parameter.Value = eyebrowRight;

        // update mouth
        parameter = Model.Parameters[18];
        parameter.Value = mouthWidth;

        parameter = Model.Parameters[19];
        parameter.Value = mouthOpen;
    }

    void SocketConnect()
    {
        if (clientSocket != null) { clientSocket.Close(); }

        clientSocket = serverSocket.Accept();
    }

    void paraUpdate()
    {
        string buff = "";
        string[] para;  // parameters

        SocketConnect();

        while (true)
        {
            recData = new byte[1024];
            int len = clientSocket.Receive(recData);

            // client sends data by group, check if no data comes, then enter next group(loop)
            if (len == 0)
            {
                SocketConnect();
                continue;
            }

            buff = Encoding.ASCII.GetString(recData, 0, len);  // store data in buffer as string type
            para = buff.Split(' '); // get 11 parameters

            angleZ = Convert.ToSingle(para[0]);  // roll
            angleY = Convert.ToSingle(para[1]);  // pitch
            angleX = Convert.ToSingle(para[2]);  // yaw

            // eye openness
            eyeOpenLeft = Convert.ToSingle(para[3]);
            eyeOpenRight = Convert.ToSingle(para[4]);

            // eyeballs
            eyeBallX = Convert.ToSingle(para[5]);
            eyeBallY = Convert.ToSingle(para[6]);

            // eyebrows
            eyebrowLeft = Convert.ToSingle(para[7]);
            eyebrowRight = Convert.ToSingle(para[8]);

            // mouth
            mouthWidth = Convert.ToSingle(para[9]);
            mouthOpen = Convert.ToSingle(para[10]);
        }
    }

    /*
    IEnumerator displaySetting()
    {
        yield return new WaitForSeconds(0.1f); // wait for 0.1 second
        SetWindowLong(GetForegroundWindow(), STYLE, BORDER); // pop up window
        bool result = SetWindowPos(GetForegroundWindow(), TOP, GUIposX, GUIposY, GUIwidth, GUIheight, SHOWWINDOW); // set position and display at top
    }
    */
}
