using System;
using System.Collections.Generic;
using UnityEngine;

public class Tree : PoolableObject
{
    [SerializeField] private Transform trunkParent;
    [SerializeField] private ObjectPoolSO treePiecesPool;
    [SerializeField] private Vector2Int minMaxPiecesAmount;
    [SerializeField] private float growAnimationDuration;
    [SerializeField] private AnimationCurve popOutCurve;
    [SerializeField] private AnimationCurve GrowAndBounceCurve;
    [SerializeField] private float collapseAnimationDuration;
    [SerializeField] private AnimationCurve collapseCurve;

    public event EventHandler treeDestroyed;

    private Queue<TreePiece> piecesCollection = new Queue<TreePiece>();
    private int treeSize;

    private enum GrowAnimationType
    {
        PopOut,
        GrowOut,
        Linear
    }

    public void GenerateTree()
    {
        //Generate a new trunk with a random size
        GenerateTrunk(UnityEngine.Random.Range(minMaxPiecesAmount.x, minMaxPiecesAmount.y + 1)); //Random.Range with ints is exclusive for the max value, so we add 1
        AnimateTreeSpawn();
    }

    private void OnDisable()
    {
        StopAllCoroutines();
    }

    private void GenerateTrunk(int size)
    {
        //Clear the list of pieces if any still exists
        while (piecesCollection.Count > 0)
        {
            TreePiece piece = piecesCollection.Dequeue();
            piece.ReturnToPool();
        }

        treeSize = size;
        trunkParent.localPosition = Vector3.zero;

        //Create new pieces based on the size
        for (int i = 0; i < size; i++)
        {
            TreePiece piece = treePiecesPool.GetPooledObject<TreePiece>();
            piece.transform.SetParent(trunkParent);
            piece.transform.SetAsFirstSibling();
            piece.transform.localPosition = Vector3.up * i;
            piece.transform.localScale = Vector3.one;
            piece.transform.localRotation = Quaternion.AngleAxis(UnityEngine.Random.Range(0, 4) * 90, Vector3.up);
            piecesCollection.Enqueue(piece);
        }
    }

    public void AnimateTreeSpawn()
    {
        //Choose a random animation based on the enum
        GrowAnimationType animationType = (GrowAnimationType)UnityEngine.Random.Range(0, Enum.GetNames(typeof(GrowAnimationType)).Length);

        //Execute the animation based on the chosen value
        switch (animationType)
        {
            case GrowAnimationType.PopOut:
                StartCoroutine(Helper.AnimationRoutine(growAnimationDuration, t =>
                {
                    trunkParent.localScale = Vector3.one * popOutCurve.Evaluate(t);
                }));
                break;
            case GrowAnimationType.GrowOut:
                StartCoroutine(Helper.AnimationRoutine(growAnimationDuration, t =>
                {
                    trunkParent.transform.localPosition = Vector3.LerpUnclamped(-Vector3.up * piecesCollection.Count, Vector3.zero, GrowAndBounceCurve.Evaluate(t));
                }));
                break;
            case GrowAnimationType.Linear:
                StartCoroutine(Helper.AnimationRoutine(growAnimationDuration, t =>
                {
                    trunkParent.transform.localPosition = Vector3.LerpUnclamped(-Vector3.up * piecesCollection.Count, Vector3.zero, t);
                }));
                break;
            default:
                break;
        }
    }

    public void RemoveBottomPiece()
    {
        if (piecesCollection.Count == 0)
            return;

        TreePiece pieceToRemove = piecesCollection.Dequeue();
        pieceToRemove.DestroyPiece();

        //Move the trunk down
        StopAllCoroutines();
        Vector3 startPosition = trunkParent.localPosition;
        Vector3 endPosition = -Vector3.up * (treeSize - piecesCollection.Count);

        StartCoroutine(Helper.AnimationRoutine(collapseAnimationDuration, t =>
        {
            trunkParent.localPosition = Vector3.LerpUnclamped(startPosition, endPosition, collapseCurve.Evaluate(t));
        }));

        //If this was the last piece, we fire the event saying the tree was totally destroyed
        if (piecesCollection.Count == 0)
        {
            treeDestroyed?.Invoke(this, EventArgs.Empty);
        }
    }

    public override void ReturnToPool()
    {
        //Return all remaining pieces to the pool, in case there's still any
        while (piecesCollection.Count > 0)
        {
            TreePiece piece = piecesCollection.Dequeue();
            piece.transform.SetParent(null);
            piece.ReturnToPool();
        }

        base.ReturnToPool();
    }

    public float GetGrowAnimationDuration()
    {
        return growAnimationDuration;
    }

    public float GetcCllapseAnimationDuration()
    {
        return collapseAnimationDuration;
    }
}
