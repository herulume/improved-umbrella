using System.Collections.Generic;

namespace OpenerCreator.Windows;

internal class LoadedActions
{
    private readonly HashSet<int> wrongActionsIndex = [];
    private List<uint> actions = [];

    internal bool IsWrongActionAt(int i)
    {
        return wrongActionsIndex.Contains(i);
    }

    internal void AddWrongActionAt(int i)
    {
        wrongActionsIndex.Add(i);
    }

    internal void ClearWrongActions()
    {
        wrongActionsIndex.Clear();
    }

    internal int ActionsCount()
    {
        return actions.Count;
    }

    internal uint GetActionAt(int i)
    {
        return actions[i];
    }

    internal void AddAction(uint action)
    {
        actions.Add(action);
    }

    internal void RemoveActionAt(int i)
    {
        actions.RemoveAt(i);
    }

    internal void InsertActionAt(int i, uint action)
    {
        actions.Insert(i, action);
    }

    internal List<uint> GetActionsByRef()
    {
        return actions;
    }

    internal void ClearActions()
    {
        actions.Clear();
    }

    internal void AddActionsByRef(List<uint> l)
    {
        actions = l;
    }
}
