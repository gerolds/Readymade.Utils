using System;
using Cysharp.Threading.Tasks;
using Readymade.Utils.Pooling;
using UnityEngine;

/// <summary>
/// Destroys the target object with some configurable parameters. Intended to be called from a <see cref="UnityEvent"/>.
/// </summary>
/// <remarks>This is a prototyping component.</remarks>
[RequireComponent(typeof(PoolableObject))]
public class TimedReleaseToPool : CommandComponent
{
    [Tooltip("Delay the release of.")]
    [Min(0)]
    [SerializeField]
    private float delay = 0;

    private bool _isExcecuting;

    private void OnEnable()
    {
        ReleaseAsync().Forget();
    }

    /// <summary>
    /// Releases the object to the pool after a delay.
    /// </summary>
    public async UniTaskVoid ReleaseAsync()
    {
        if (_isExcecuting)
            return;

        _isExcecuting = true;

        if (delay > 0)
        {
            await UniTask.Delay(TimeSpan.FromSeconds(delay), delayTiming: PlayerLoopTiming.Update);
        }

        PooledInstance releaseHandle = GetComponent<PooledInstance>();
        Debug.Assert(releaseHandle, "Not a pooled instance.", this);
        releaseHandle.Release();
    }

    /// <inheritdoc cref="CommandComponent"/>
    protected override void OnExecute()
    {
        ReleaseAsync().Forget();
    }
}