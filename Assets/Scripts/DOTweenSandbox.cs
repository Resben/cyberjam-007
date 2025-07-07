using UnityEngine;
using DG.Tweening;

public class DOTweenSandbox : MonoBehaviour
{
    void Start()
    {
        transform.DOMove(new Vector3(0, 5, 0), 2f)
            .SetEase(Ease.InOutQuad)
            .OnComplete(() => Debug.Log("Movement complete!"));
    }
}
