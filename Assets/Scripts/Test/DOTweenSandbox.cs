using UnityEngine;
using DG.Tweening;

public class DOTweenSandbox : MonoBehaviour
{
    [SerializeField] private Transform[] _verticalSpheres;
    [SerializeField] private Transform[] _horizontalSpheres;

    void Start()
    {
        transform.DOMove(new Vector3(0, 5, 0), 2f)
            .SetEase(Ease.InOutQuad)
            .SetLoops(-1, LoopType.Yoyo);


        _verticalSpheres[0].DOLocalPath(
            new Vector3[] { new Vector3(0, 0, -1), new Vector3(0, -1, 0), new Vector3(0, 0, 1), new Vector3(0, 1, 0) },
            2f,
            PathType.Linear)
            .SetEase(Ease.Linear)
            .SetLoops(-1, LoopType.Restart);
        _verticalSpheres[1].DOLocalPath(
            new Vector3[] { new Vector3(0, 0, 1), new Vector3(0, 1, 0), new Vector3(0, 0, -1), new Vector3(0, -1, 0) },
            2f,
            PathType.Linear)
            .SetEase(Ease.Linear)
            .SetLoops(-1, LoopType.Restart);

        _horizontalSpheres[0].DOLocalPath(
            new Vector3[] { new Vector3(0, 0, -1), new Vector3(-1, 0, 0), new Vector3(0, 0, 1), new Vector3(1, 0, 0) },
            2f,
            PathType.Linear)
            .SetEase(Ease.Linear)
            .SetLoops(-1, LoopType.Restart);
        _horizontalSpheres[1].DOLocalPath(
            new Vector3[] { new Vector3(0, 0, 1), new Vector3(1, 0, 0), new Vector3(0, 0, -1), new Vector3(-1, 0, 0) },
            2f,
            PathType.Linear)
            .SetEase(Ease.Linear)
            .SetLoops(-1, LoopType.Restart);
        
    }
}
