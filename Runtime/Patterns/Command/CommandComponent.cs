using UnityEngine;

public abstract class CommandComponent : MonoBehaviour, ICommand
{
    [SerializeField] private bool debug;

    public void Execute()
    {
        if (debug)
        {
            Debug.Log($"[{this.GetType().Name}] Executing...", this);
        }

        OnExecute();
    }

    protected abstract void OnExecute();
}