using UnityEngine;

public class Test : MonoBehaviour
{
    [SerializeField] private Effect effect;

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Return))
        {
            effect.Play();
        }
    }
}
