using UnityEngine;
using UnityEngine.UI;

public class Enable : MonoBehaviour
{
    void Start() => GetComponent<RawImage>().enabled = true;
}
