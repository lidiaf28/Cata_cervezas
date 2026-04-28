using UnityEngine;
using TMPro;

public class DebugVR : MonoBehaviour
{
    public TextMeshProUGUI texto;

    public void Log(string msg)
    {
        texto.text += "\n" + msg;
    }

    public void Clear()
    {
        texto.text = "";
    }
}