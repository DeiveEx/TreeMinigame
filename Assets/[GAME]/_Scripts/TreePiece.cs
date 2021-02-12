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
        renderer.sharedMaterial = defaultMaterial;
    }

    public void DestroyPiece()
    {
        transform.parent = null;
        Color white = Color.white;
        Color clear = white;
        clear.a = 0;

        renderer.sharedMaterial = fadeMaterial;

        //The body of the destroy animation
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
