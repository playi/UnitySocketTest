using UnityEngine;
using UnityEngine.UI;

using System;
using System.Net;
using System.Net.Sockets;

public class mainController : MonoBehaviour {

  public InputField inputHost;
  public InputField inputPort;
  public Button     buttonConnection;
  public Text       textStatus;
  public InputField inputSend;
  public Button     buttonSend;
  public Button     buttonClear;
  public Text       textReceived;

  private TcpClient theSocket = null;


  void Start() {

    inputHost.text = "52.1.32.175";
    inputPort.text = "5500";

    buttonConnection.onClick.AddListener(onClickConnection);
    buttonSend      .onClick.AddListener(onClickSend);
    buttonClear     .onClick.AddListener(onClickClear);

    updateStatus();
  }

  void Update() {
    tryRead();
  }

  bool Connected {
    get {
      if (theSocket == null) {
        return false;
      }
      else {
        return theSocket.Connected;
      }
    }
  }

  void updateStatus() {
    string s = Connected ? "connected" : "disconnected";
    setStatus(s);

    buttonConnection.GetComponentInChildren<Text>().text = Connected ? "disconnect" : "connect";
  }

  void setStatus(string s) {
    textStatus.text = s;
  }

  void onClickConnection() {
    if (Connected) {
      theSocket.Close();
      theSocket = null;
      updateStatus();
    }
    else {
      theSocket = new TcpClient();
      bool valid = true;
      IPAddress ipa  = null;
      uint      port = 0;
      valid = valid && tryGetAddress(out ipa );
      valid = valid && tryGetPort   (out port);
      if (valid) {
        setStatus("connecting..");
        IAsyncResult result = theSocket.BeginConnect(ipa, (int)port, null, null);
        if (result.AsyncWaitHandle.WaitOne(1000, true)) {
          if (Connected) {
            updateStatus();
          }
          else {
            setStatus("error connecting");
          }
        }
        else {
          setStatus("timeout connecting");
        }
      }
      else {
        setStatus("bad host/IP");
      }
    }
  }

  bool tryGetAddress(out IPAddress ipa) {
    ipa = null;
    byte[] addr;
    if (!tryGetHost(out addr)) {
      return false;
    }
    ipa = new IPAddress(addr);
    return true;
  }

  bool tryGetHost(out byte[] ret) {
    ret = new byte[] {0, 0, 0, 0};
    string host = inputHost.text;
    string[] parts = host.Split('.');
    if (parts.Length != 4) {
      Debug.LogError("invalid ip: " + host);
      return false;
    }
    else {
      ret = new byte[4];
      for (int n = 0; n < parts.Length; ++n) {
        byte b;
        if (!byte.TryParse(parts[n], out b)) {
          Debug.LogError("invalid ip: " + host);
          return false;
        }
        ret[n] = b;
      }
      return true;
    }
  }

  bool tryGetPort(out uint ret) {
    string port = inputPort.text;
    if (!uint.TryParse(port, out ret)) {
      Debug.LogError("invalid port: " + port);
      return false;
    }
    else {
      return true;
    }
  }

  void tryRead() {
    if (!Connected) {
      return;
    }

    if (theSocket.Available > 0) {
      NetworkStream ns = theSocket.GetStream();
      byte[] buf = new byte[theSocket.Available];
      ns.Read(buf, 0, theSocket.Available);
      textReceived.text += System.Text.Encoding.ASCII.GetString(buf, 0, buf.Length);
    }
  }

  void onClickSend() {
  }

  void onClickClear() {
    textReceived.text = "";
  }


}
