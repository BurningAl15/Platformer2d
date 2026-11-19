using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Open menus in the order they appeared. The Cancel action (gamepad B, Esc)
/// closes the top one. Esc is also bound to Pause, so pause/settings toggles
/// skip the frame in which Cancel already closed something.
/// </summary>
public static class MenuBackStack
{
    private static readonly List<(UnityEngine.Object owner, Action close)> stack = new List<(UnityEngine.Object, Action)>();
    private static int lastCloseFrame = -1;

    public static bool ClosedThisFrame => lastCloseFrame == Time.frameCount;

    public static bool HasOpen { get { Prune(); return stack.Count > 0; } }

    public static bool IsTop(UnityEngine.Object owner) { Prune(); return stack.Count > 0 && stack[stack.Count - 1].owner == owner; }

    public static void Push(UnityEngine.Object owner, Action close)
    {
        Remove(owner);
        stack.Add((owner, close));
    }

    public static void Remove(UnityEngine.Object owner) => stack.RemoveAll(entry => entry.owner == owner);

    /// <summary>Closes the most recent menu; returns false when none is open.</summary>
    public static bool CloseTop()
    {
        Prune();
        if (stack.Count == 0) return false;
        var top = stack[stack.Count - 1];
        stack.RemoveAt(stack.Count - 1);
        lastCloseFrame = Time.frameCount;
        top.close?.Invoke();
        return true;
    }

    // Menus destroyed with their scene leave stale entries behind.
    private static void Prune() => stack.RemoveAll(entry => entry.owner == null);
}
