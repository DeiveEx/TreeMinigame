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

        //Get the initial camera position as the offset from where we'll see the trees
        cameraOffset = mainCamera.transform.position;
    }

    private void Start()
    {
        GenerateNewTree();
    }

    public void GenerateNewTree()
    {
        //Get a new tree from the pool
        Tree tree = treePool.GetPooledObject<Tree>();

        //Get the position of the previous tree and do any necessary cleanup to it
        Vector3 previousTreePosition = Vector3.zero;

        if (currentTree != null)
        {
            previousTreePosition = currentTree.transform.position;
            currentTree.treeDestroyed -= TreeDestroyedEventHandler;
            currentTree.ReturnToPool();
        }

        //Position the new tree behind the current tree and to a random side
        tree.transform.position = previousTreePosition + (new Vector3((Random.Range(0, 2) * 2) - 1, 0, 1) * newTreeDistance);
        currentTree = tree;
        tree.treeDestroyed += TreeDestroyedEventHandler;
        currentTree.GenerateTree();

        //Move the camera
        StopAllCoroutines();
        StartCoroutine(MoveCameraToTree(tree));
    }

    private void TreeDestroyedEventHandler(object sender, System.EventArgs e)
    {
        GenerateNewTree();
    }

    private IEnumerator MoveCameraToTree(Tree tree)
    {
        yield return new WaitForSeconds(tree.GetGrowAnimationDuration());

        Vector3 startPosition = mainCamera.transform.position;

        yield return StartCoroutine(Helper.AnimationRoutine(cameraMovementDuration, t =>
        {
            mainCamera.transform.position = Vector3.LerpUnclamped(startPosition, tree.transform.position + cameraOffset, cameraMovementCurve.Evaluate(t));
        }));
    }

    public void RemovePiece()
    {
        currentTree.RemoveBottomPiece();
    }
}
