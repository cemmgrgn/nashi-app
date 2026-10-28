using System.Runtime.InteropServices;
using System.Windows.Threading;
namespace Nashi.Windows;
public sealed partial class MainWindow
{
    // Explorer starts one process per selected item, so launches that arrive close together share one wheel.
    private readonly List<string> explorerBatch=[];private string explorerMode="";private DispatcherTimer? explorerTimer;
    [StructLayout(LayoutKind.Sequential)]private struct CursorPoint{public int X,Y;}
    [DllImport("user32.dll")]private static extern bool GetCursorPos(out CursorPoint point);
    internal void AcceptExplorer(string mode,string[] paths)
    {
        // A different command replaces one still waiting for a busy operation, so modes never mix in one batch.
        if(mode!=explorerMode&&explorerBatch.Count>0){if(busy)explorerBatch.Clear();else OpenExplorerWheel();}
        explorerMode=mode;foreach(var path in paths)if((File.Exists(path)||Directory.Exists(path))&&!explorerBatch.Contains(path,StringComparer.OrdinalIgnoreCase))explorerBatch.Add(path);
        explorerTimer??=new(){Interval=TimeSpan.FromMilliseconds(350)};explorerTimer.Tick-=ExplorerReady;explorerTimer.Tick+=ExplorerReady;explorerTimer.Stop();explorerTimer.Start();
    }
    private void ExplorerReady(object? sender,EventArgs e){explorerTimer?.Stop();OpenExplorerWheel();}
    private void OpenExplorerWheel()
    {
        if(explorerBatch.Count==0)return;
        // Keep the Explorer command until the current operation finishes instead of falling back to a plain launch.
        if(busy){explorerTimer?.Start();return;}
        var paths=explorerBatch.ToArray();explorerBatch.Clear();
        var choices=GlobalDropWheel.FileChoices(explorerMode,paths);
        // Without a matching tool, open the files in Nashi instead of showing an empty wheel.
        if(choices.Count==0){AcceptLaunch(paths);return;}
        var wheel=new WheelWindow(null,choices,async choice=>await RunFileChoice(choice.Id,paths)){Topmost=true};wheel.Show();
        if(GetCursorPos(out var point))DesktopGeometry.PlaceWheel(wheel,point.X,point.Y,WheelPresentation.Diameter*preferences.Value.WheelScale);wheel.Activate();
    }
}
