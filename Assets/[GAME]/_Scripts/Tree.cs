using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Tree : PoolableObject
{
    [SerializeField] private Transform trunkParent;
    [SerializeField] private ObjectPoolSO treePiecesPool;
    [SerializeField] private Vector2Int minMaxPiecesAmount;
    [SerializeField] private float growAnimationDuration;
    [SerializeField] private AnimationCurve popOutCurve;
    [SerializeField] private AnimationCurve GrowAndBounceCurve;

    private Queue<TreePiece> piecesCollection = new Queue<TreePiece>();

    private enum GrowAnimationType
    {
        PopOut,
        GrowOut,
        Linear
    }

    public void GenerateTree()
    {
        //Generate a new trunk with a random size
        GenerateTrunk(Random.Range(minMaxPiecesAmount.x, minMaxPiecesAmount.y));
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
            piece.DestroyPiece();
        }

        //Create new pieces based on the size
        for (int i = 0; i < size; i++)
        {
            TreePiece piece = treePiecesPool.GetPooledObject<TreePiece>();
            piece.transform.SetParent(trunkParent);
            piece.transform.SetAsFirstSibling();
            piece.transform.localPosition = Vector3.up * i;
            piece.transform.localScale = Vector3.one;
            piece.transform.localRotation = Quaternion.AngleAxis(Random.Range(0, 4) * 90, Vector3.up);
            piecesCollection.Enqueue(piece);
        }
    }

    public void AnimateTreeSpawn()
    {
        //Choose a random animation based on the enum
        GrowAnimationType animationType = (GrowAnimationType)Random.Range(0, System.Enum.GetNames(typeof(GrowAnimationType)).Length);

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
        TreePiece pieceToRemove = piecesCollection.Dequeue();
        pieceToRemove.DestroyPiece();


    }

    public float GetGrowAnimationDuration()
    {
        return growAnimationDuration;
    }
}
