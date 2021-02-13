using System;
using System.Collections;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    [SerializeField] private ObjectPoolSO treePool;
    [SerializeField] private Vector2Int minMaxTreesPerRound;
    [SerializeField] private float newTreeDistance;
    [SerializeField] private float cameraMovementDuration;
    [SerializeField] private AnimationCurve cameraMovementCurve;

    private Camera mainCamera;
    private int treeCountInRound;
    private Vector3 cameraOffset;
    private Tree currentTree;
    private GameState currentState;

    private enum GameState
    {
        Wait,
        Play
    }

    private void Awake()
    {
        //Since Unity only optimized the "Camera.main" thing only on Unity 2020.2 and we're using 2019.4 (LTS), it's still a good idea to cache the camera
        mainCamera = Camera.main;

        //Get the initial camera position as the offset from where we'll see the trees (as if the tree were at (0, 0, 0) in the world)
        cameraOffset = mainCamera.transform.position;
    }

    private void Start()
    {
        currentState = GameState.Play;
        StartNewRound();
    }

    public void StartNewRound()
    {
        if (currentTree != null)
            currentTree.ReturnToPool();

        currentTree = null;
        treeCountInRound = UnityEngine.Random.Range(minMaxTreesPerRound.x, minMaxTreesPerRound.y + 1); //Random.Range with ints is exclusive for the max value, so we add 1
        GenerateNewTree();
    }

    public void GenerateNewTree()
    {
        StopAllCoroutines();

        //Check if we finished the current round
        if (treeCountInRound <= 0)
        {
            Debug.Log("Round finished");
            StartNewRound();
            return;
        }

        //Get a new tree from the pool
        Tree tree = treePool.GetPooledObject<Tree>();

        //Get the position of the previous tree and do any necessary cleanup to it
        Vector3 previousTreePosition = Vector3.zero;

        if (currentTree != null)
        {
            previousTreePosition = currentTree.transform.position;
            currentTree.treeDestroyed -= TreeDestroyedEventHandler;
            StartCoroutine(ReturnTreeToPoolRoutine(currentTree)); //Just so when click the "next tree" button the current doesn't suddenly disappear, we return it to the pool after the camera already moved
        }

        //Position the new tree behind the current tree and to a random side
        Vector3 treeOffset = currentTree == null ? Vector3.zero : new Vector3((UnityEngine.Random.Range(0, 2) * 2) - 1, 0, 1) * newTreeDistance;
        tree.transform.position = previousTreePosition + treeOffset;
        tree.treeDestroyed += TreeDestroyedEventHandler;
        currentTree = tree;
        currentTree.GenerateTree();

        //Move the camera
        StartCoroutine(MoveCameraToTree(tree));

        treeCountInRound--;
    }

    private void TreeDestroyedEventHandler(object sender, System.EventArgs e)
    {
        GenerateNewTree();
    }

    private IEnumerator ReturnTreeToPoolRoutine(Tree tree)
    {
        yield return new WaitForSeconds(cameraMovementDuration);
        tree.ReturnToPool();
    }

    private IEnumerator MoveCameraToTree(Tree tree)
    {
        currentState = GameState.Wait;

        yield return new WaitForSeconds(tree.GetGrowAnimationDuration());

        Vector3 startPosition = mainCamera.transform.position;

        yield return StartCoroutine(Helper.AnimationRoutine(cameraMovementDuration, t =>
        {
            mainCamera.transform.position = Vector3.LerpUnclamped(startPosition, tree.transform.position + cameraOffset, cameraMovementCurve.Evaluate(t));
        }));

        currentState = GameState.Play;
    }

    public void RemovePiece()
    {
        if (currentState == GameState.Wait)
            return;

        StartCoroutine(RemovePieceRoutine());
    }

    private IEnumerator RemovePieceRoutine()
    {
        currentState = GameState.Wait;
        currentTree.RemoveBottomPiece();
        yield return new WaitForSeconds(currentTree.GetcCllapseAnimationDuration());
        currentState = GameState.Play;
    }
}
