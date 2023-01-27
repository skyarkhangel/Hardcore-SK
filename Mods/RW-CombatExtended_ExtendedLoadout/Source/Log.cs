using System.Diagnostics;

namespace CombatExtended.ExtendedLoadout;

public class DbgLog
{
    [Conditional("DEBUG")]
    public static void Msg(string message) => Verse.Log.Message($"[ExtendedLoadout] {message}");

    [Conditional("DEBUG")]
    public static void Err(string message) => Verse.Log.Error($"[ExtendedLoadout] {message}");

    [Conditional("DEBUG")]
    public static void Wrn(string message) => Verse.Log.Warning($"[ExtendedLoadout] {message}");
}