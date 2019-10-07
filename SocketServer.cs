using System.Collections;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Net;
using System.Threading;
using System;
using UnityEngine;
using UnityEngine.UI;

public class SocketServer : MonoBehaviour {

    public string serverIP;
    public int serverPort;
    public Text textOutput;

    private string TAG = "SocketServer: ";

    private bool isRunningThread = false;
    private Socket server;
    private List<Socket> socketList = new List<Socket>();
    private byte[] msg = new byte[10000000];


    public enum messageID
    {
        msgString = 0,
        msgInt = 1,
        msgFloat = 2,
        msgEmpatica = 3,
        msgHeadPose = 4,
        msgCameraPose = 5,
        msgHeadCamera = 6,
        EndThread = 99
    }

    // Use this for initialization
    void Start () {

        InitSocketServer();

    }
	
	// Update is called once per frame
	void Update () {
        //textOutput.text = "haaaaaaaaaaaaaaaaaa";

    }

    void OnApplicationQuit()
    {
        server.Close();
        Debug.Log(TAG + "Server closed");
    }

    //init socket server
    void InitSocketServer()
    {
        server = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

        IPAddress ip = IPAddress.Parse(serverIP);
        IPEndPoint ip_end_point = new IPEndPoint(ip, serverPort);

        server.Bind(ip_end_point);
        server.Listen(10);
        Debug.Log(TAG + "Start server socket: " + server.LocalEndPoint.ToString());
        textOutput.text = TAG + "Start server socket: " + server.LocalEndPoint.ToString();
        server.BeginAccept(new AsyncCallback(AcceptClient), server);
    }

    void AcceptClient(IAsyncResult ar)
    {
        Socket myserver = ar.AsyncState as Socket;
        Socket client = myserver.EndAccept(ar);
        Debug.Log(TAG + "New Client added, Client ip: " + client.RemoteEndPoint);
        textOutput.text = TAG + "New Client added, Client ip: " + client.RemoteEndPoint;
        socketList.Add(client);

        isRunningThread = true;
        Thread t = new Thread(ReceiveMsg);
        t.Start(client);

        myserver.BeginAccept(new AsyncCallback(AcceptClient), myserver);
    }

    void ReceiveMsg(object socket)
    {
        Socket mSocket = socket as Socket;
        while(isRunningThread)
        {
            try
            {
                int packageLength = mSocket.Receive(msg);
                Debug.Log(TAG + "Client id: " + mSocket.RemoteEndPoint.ToString() + " Package Length: " + packageLength);
            }
            catch(Exception e)
            {
                Debug.LogError(TAG + e.Message);
                socketList.Remove(mSocket);
                //mSocket.Shutdown(SocketShutdown.Both);
                //mSocket.Close();
                break;
            }

            ByteBuffer buff = new ByteBuffer(msg);

            int id = buff.ReadInt();
            if (id == (int)messageID.msgString)
            {
                string mssage = buff.ReadString();
                Debug.Log(TAG + "Client id: " + mSocket.RemoteEndPoint.ToString() + " Test Message: " + mssage);
            }
            else if (id == (int)messageID.msgInt)
            {
                int mssage = buff.ReadInt();
                Debug.Log(TAG + "Client id: " + mSocket.RemoteEndPoint.ToString() + " Test Message: " + mssage);
            }
            else if (id == (int)messageID.msgFloat)
            {
                float mssage = buff.ReadFloat();
                Debug.Log(TAG + "Client id: " + mSocket.RemoteEndPoint.ToString() + " Test Message: " + mssage);
            }
            else if (id == (int)messageID.EndThread)
            {
                isRunningThread = false;
                string mssage = buff.ReadString();
                Debug.Log(TAG + "Client id: " + mSocket.RemoteEndPoint.ToString() + " Test Message: " + mssage);
            }
            else if (id == (int)messageID.msgHeadPose)
            {
                float transformX = buff.ReadFloat();
                float transformY = buff.ReadFloat();
                float transformZ = buff.ReadFloat();

                float rotationX = buff.ReadFloat();
                float rotationY = buff.ReadFloat();
                float rotationZ = buff.ReadFloat();

                Debug.Log(TAG + "Client id: " + mSocket.RemoteEndPoint.ToString() + " Transform: " + transformX + transformY + transformZ + " Rotation: " + rotationX + rotationY + rotationZ);
                textOutput.text = TAG + "Client id: " + mSocket.RemoteEndPoint.ToString() + " Transform: " + transformX + transformY + transformZ + " Rotation: " + rotationX + rotationY + rotationZ;
            }
            else if (id == (int)messageID.msgCameraPose)
            {
                float transformX = buff.ReadFloat();
                float transformY = buff.ReadFloat();
                float transformZ = buff.ReadFloat();

                float rotationX = buff.ReadFloat();
                float rotationY = buff.ReadFloat();
                float rotationZ = buff.ReadFloat();

                Debug.Log(TAG + "Client id: [Camera]" + mSocket.RemoteEndPoint.ToString() + " Transform: " + transformX + transformY + transformZ + " Rotation: " + rotationX + rotationY + rotationZ);
                textOutput.text = TAG + "Client id: [Camera]" + mSocket.RemoteEndPoint.ToString() + " Transform: " + transformX + transformY + transformZ + " Rotation: " + rotationX + rotationY + rotationZ;
            }
            else if (id == (int)messageID.msgHeadCamera)
            {
                float transformX = buff.ReadFloat();
                float transformY = buff.ReadFloat();
                float transformZ = buff.ReadFloat();

                float rotationX = buff.ReadFloat();
                float rotationY = buff.ReadFloat();
                float rotationZ = buff.ReadFloat();
                float rotationW = buff.ReadFloat();

                Debug.Log(TAG + "Client id: " + mSocket.RemoteEndPoint.ToString() + " Transform: " + transformX + transformY + transformZ + " Rotation: " + rotationX + rotationY + rotationZ);
                textOutput.text = TAG + "Client id: " + mSocket.RemoteEndPoint.ToString() + " Transform: " + transformX + transformY + transformZ + " Rotation: " + rotationX + rotationY + rotationZ;

                float CtransformX = buff.ReadFloat();
                float CtransformY = buff.ReadFloat();
                float CtransformZ = buff.ReadFloat();

                float CrotationX = buff.ReadFloat();
                float CrotationY = buff.ReadFloat();
                float CrotationZ = buff.ReadFloat();
                float CrotationW = buff.ReadFloat();

                Debug.Log(TAG + "Client id: [Camera]" + mSocket.RemoteEndPoint.ToString() + " Transform: " + CtransformX + CtransformY + CtransformZ + " Rotation: " + CrotationX + CrotationY + CrotationZ);
                textOutput.text = TAG + "Client id: [Camera]" + mSocket.RemoteEndPoint.ToString() + " Transform: " + CtransformX + CtransformY + CtransformZ + " Rotation: " + CrotationX + CrotationY + CrotationZ;
            }
            else
                continue;    
        }
    }

    void sendMsgString(int id, string msg)
    {
        ByteBuffer buffer = new ByteBuffer();
        buffer.WriteInt(id);
        buffer.WriteString(msg);

        for (int i = 0; i < socketList.Count; i++)
        {           
            int msgLength = socketList[i].Send(buffer.ToBytes());
            Debug.Log(TAG + "Send msg length: " + msgLength);
        }
    }

    void sendMsgInt(int id, int msg)
    {
        ByteBuffer buffer = new ByteBuffer();
        buffer.WriteInt(id);
        buffer.WriteInt(msg);

        for (int i = 0; i < socketList.Count; i++)
        {
            int msgLength = socketList[i].Send(buffer.ToBytes());
            Debug.Log(TAG + "Send msg length: " + msgLength);
        }
    }

    void sendMsgFloat(int id, float msg)
    {
        ByteBuffer buffer = new ByteBuffer();
        buffer.WriteInt(id);
        buffer.WriteFloat(msg);

        for (int i = 0; i < socketList.Count; i++)
        {
            int msgLength = socketList[i].Send(buffer.ToBytes());
            Debug.Log(TAG + "Send msg length: " + msgLength);
        }
    }

    //Test on UI
    void OnGUI()
    {
         if (GUI.Button(new Rect(100, 50, 200, 100), "Send String to client"))
         {
            if(isRunningThread)
                sendMsgString((int)messageID.msgString, "Message from server...");
         }

        if (GUI.Button(new Rect(100, 200, 200, 100), "Send Int to client"))
        {
            if (isRunningThread)
                sendMsgInt((int)messageID.msgInt, 12);
        }

        if (GUI.Button(new Rect(100, 350, 200, 100), "Send Float to client"))
        {
            if (isRunningThread)
                sendMsgFloat((int)messageID.msgFloat, 16.66f);
        }

    }
}
