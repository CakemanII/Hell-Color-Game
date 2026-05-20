using System.Collections;
using UnityEngine;

public abstract class Item : MonoBehaviour
{
    private bool equipped = false;
    protected bool transitioning { get; private set; } = false;
    public bool IsEquipped => equipped;

    protected bool entityWantsToUse { private set; get; }


    public void UseItemInput(bool input)
    {
        entityWantsToUse = input;
        OnUseItemInput();
    }

    protected virtual void OnUseItemInput() { }

    public void Equip()
    {
        if (equipped) return;
        WaitForEquipped();
    }

    public void Unequip()
    {
        if (!equipped) return;
        WaitForUnequipped();
    }

    private IEnumerator WaitForEquipped()
    {
        equipped = true;
        yield return null;
    }

    private IEnumerator WaitForUnequipped()
    {
        equipped = false;
        yield return null;
    }

    private IEnumerator WaitForAnimationStateToFinish(string animationStateName)
    {
        transitioning = true;
        transitioning = false;
        yield return null;
    }
}
