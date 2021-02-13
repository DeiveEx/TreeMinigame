using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TreePiece : PoolableObject
{
    [SerializeField] private new Renderer renderer;
    [SerializeField] private float destroyAnimDuration;
    [SerializeField] private Vector3 destroyAnimTargetScale;
    [SerializeField] private AnimationCurve destroyAnimCurve;
    [SerializeField] private Material defaultMaterial;
    [SerializeField] private Material fadeMaterial;

    private void OnEnable()
    {
        //Reset the material
        renderer.sharedMaterial = defaultMaterial;
    }

    private void OnDisable()
    {
        StopAllCoroutines();
    }

    public void DestroyPiece()
    {
        //Remove the parent so when the tree is returned to its own pool, it doesn't bring this piece with it
        transform.parent = null;

        //Set the colors and material for the fade effect
        Color white = Color.white;
        Color clear = white;
        clear.a = 0; //Unity's "Clear" color is (0, 0, 0, 0), but we want (1, 1, 1, 0)

        renderer.sharedMaterial = fadeMaterial;

        //The actual body of the destroy animation
        Action<float> body = t =>
        {
            transform.localScale = Vector3.LerpUnclamped(Vector3.one, destroyAnimTargetScale, destroyAnimCurve.Evaluate(t));
            renderer.material.color = Color.Lerp(white, clear, t);
        };

        //What to do after the animation ends
        Action callback = () =>
        {
            ReturnToPool();
        };

        //Start the animation
        StartCoroutine(Helper.AnimationRoutine(destroyAnimDuration, body, callback));
    }
}
