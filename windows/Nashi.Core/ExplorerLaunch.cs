namespace Nashi.Core;

/// Explorer right-click commands start Nashi once per selected item. Forwarded launches
/// carry their wheel in a marker that cannot be a Windows path, so the running instance
/// can gather the items into one wheel while older instances simply ignore the marker.
public static class ExplorerLaunch
{
    public static IReadOnlyList<string> Modes{get;}=["convert","tools","actions"];
    private const string Marker="nashi-explorer:";
    public static (string Mode,string[] Paths)? Parse(string[] args)
    {
        var index=Array.IndexOf(args,"--explorer");
        return index>=0&&index+1<args.Length&&Modes.Contains(args[index+1])?(args[index+1],args[(index+2)..]):null;
    }
    public static string[] Encode(string mode,string[] paths)=>Modes.Contains(mode)?[Marker+mode,..paths]:throw new ArgumentException("Unknown Explorer command.");
    public static (string Mode,string[] Paths)? Decode(string[] message)=>message.Length>1&&message[0].StartsWith(Marker,StringComparison.Ordinal)&&Modes.Contains(message[0][Marker.Length..])?(message[0][Marker.Length..],message[1..]):null;
}
