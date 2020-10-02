#if UNITY_IOS && !UNITY_EDITOR
using System.Runtime.InteropServices;
#endif
using UnityEngine;

public class CubeScript : MonoBehaviour
{
    public GameObject cube;
    public Vector3 rotate;
    public GUIStyle msgStyle;

    private Rect _rect;
    private string _msg;

#if UNITY_IOS && !UNITY_EDITOR
    [DllImport("__Internal")]
    private static extern void UnityOnStart();
#endif

    private void Start()
    {
        _rect = new Rect(0, 0, Screen.width, Screen.height);
        _msg = "";

#if UNITY_IOS && !UNITY_EDITOR
        UnityOnStart();
#endif
    }

    private void OnMessageReceived(string msg)
    {
        _msg = msg;
    }

    private void OnGUI()
    {
        GUI.Label(_rect, string.IsNullOrEmpty(_msg) ? "Waiting for message..." : _msg, msgStyle);
    }

    private void Update()
    {
        cube.transform.Rotate(rotate * Time.deltaTime);
    }
}
