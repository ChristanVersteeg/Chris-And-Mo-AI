using UnityEngine;

public class Passable : MonoBehaviour
{
    public void Pass() => gameObject.SetActive(false);
}
