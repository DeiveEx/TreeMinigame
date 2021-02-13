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

    public event EventHandler roundFinished;

    private Camera mainCamera;
    private int treeCountInRound;
    private Vector3 cameraOffset;
    private Tree currentTree;
    private GameState currentState;
    private Coroutine cameraMovement;

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
        {
            currentTree.treeDestroyed -= TreeDestroyedEventHandler;
            currentTree.ReturnToPool();
        }

        currentTree = null;
        treeCountInRound = UnityEngine.Random.Range(minMaxTreesPerRound.x, minMaxTreesPerRound.y + 1); //Random.Range with ints is exclusive for the max value, so we add 1
        GenerateNewTree();
    }

    public void GenerateNewTree()
    {
        //Stop the camera movement if it's still moving
        if (cameraMovement != null)
            StopCoroutine(cameraMovement);

        //Check if we finished the current round
        if (treeCountInRound <= 0)
        {
            roundFinished?.Invoke(this, EventArgs.Empty);
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
            StartCoroutine(ReturnTreeToPoolRoutine(currentTree)); //Just so when we click the "new tree" button the current doesn't suddenly disappear, we return it to the pool after the camera already moved
        }

        //Position the new tree behind the current tree and to a random side
        Vector3 treeOffset = currentTree == null ? Vector3.zero : new Vector3((UnityEngine.Random.Range(0, 2) * 2) - 1, 0, 1) * newTreeDistance;
        tree.transform.position = previousTreePosition + treeOffset;
        tree.treeDestroyed += TreeDestroyedEventHandler;
        currentTree = tree;
        currentTree.GenerateTree();

        //Move the camera
        cameraMovement = StartCoroutine(MoveCameraToTree(tree)); //We keep a reference of this coroutine so we stop it in case this method is called again before the camera movement ends

        treeCountInRound--;
    }

    private void TreeDestroyedEventHandler(object sender, EventArgs e)
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
        yield return new WaitForSeconds(tree.GetGrowAnimationDuration());

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

        //Check if this is the last piece of the tree.
        bool isLastPiece = currentTree.GetRemainingPiecesCount() == 1;

        //Remove the piece and wait for the collapse animation to end
        currentTree.RemoveBottomPiece();
        yield return new WaitForSeconds(currentTree.GetCollapseAnimationDuration());

        //If we removed the last piece of the tree, we don't change the state back to Play because we know the camera will move to the next tree
        if (!isLastPiece)
            currentState = GameState.Play;
    }
}
