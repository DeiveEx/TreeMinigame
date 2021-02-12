using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    [SerializeField] private ObjectPoolSO treePool;
    [SerializeField] private float newTreeDistance;
    [SerializeField] private float cameraMovementDuration;
    [SerializeField] private AnimationCurve cameraMovementCurve;

    private Camera mainCamera;
    private Vector3 cameraOffset;
    private Tree currentTree;

    private void Awake()
    {
        //Since Unity only optimized the "Camera.main" thing only on Unity 2020.2 and we're using 2019.4 (LTS), it's still a good idea to cache the camera
        mainCamera = Camera.main;
        cameraOffset = mainCamera.transform.position;
    }

    private void Start()
    {
        GenerateNewTree();
    }

    public void GenerateNewTree()
    {
        Tree tree = treePool.GetPooledObject<Tree>();
        Vector3 previousTreePosition = currentTree == null ? Vector3.zero : currentTree.transform.position;

        //Position the new tree behind the current tree and to a random side
        tree.transform.position = previousTreePosition + (new Vector3((Random.Range(0, 2) * 2) - 1, 0, 1) * newTreeDistance);
        currentTree = tree;
        currentTree.GenerateTree();

        StopAllCoroutines();
        StartCoroutine(MoveCameraToTree(tree));
    }

    private IEnumerator MoveCameraToTree(Tree tree)
    {
        yield return new WaitForSeconds(tree.GetGrowAnimationDuration());

        float timePassed = 0;
        Vector3 startPosition = mainCamera.transform.position;

        while (timePassed < cameraMovementDuration)
        {
            timePassed += Time.deltaTime;

            mainCamera.transform.position = Vector3.LerpUnclamped(startPosition, tree.transform.position + cameraOffset, cameraMovementCurve.Evaluate(timePassed / cameraMovementDuration));

            yield return null;
        }

        mainCamera.transform.position = tree.transform.position + cameraOffset;
    }

    public void RemovePiece()
    {
        currentTree.RemoveBottomPiece();
    }
}
