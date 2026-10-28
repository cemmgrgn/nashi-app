using System.Runtime.InteropServices;
using Microsoft.Win32;
namespace Nashi.Windows;
/// Optional classic Explorer verbs for the current user, so no administrator rights are needed.
/// Windows 11 lists them under "Show more options". Key names keep Convert, Tools, File actions in order.
internal static class ExplorerMenu
{
    private const string Root=@"Software\Classes\AllFilesystemObjects\shell\Nashi";
    private static readonly (string Key,string Mode,string Title)[] Commands=[("1convert","convert","Convert"),("2tools","tools","Tools"),("3actions","actions","File actions")];
    [DllImport("user32.dll")]internal static extern bool AllowSetForegroundWindow(int processId);
    internal static bool Enabled{get{using var key=Registry.CurrentUser.OpenSubKey(Root);return key is not null;}}
    internal static void Enable(string executable)
    {
        Registry.CurrentUser.DeleteSubKeyTree(Root,false);
        using var root=Registry.CurrentUser.CreateSubKey(Root);root.SetValue("MUIVerb","Nashi");root.SetValue("Icon","\""+executable+"\",0");root.SetValue("SubCommands","");root.SetValue("MultiSelectModel","Player");
        foreach(var (key,mode,title) in Commands)
        {
            using var verb=root.CreateSubKey(@"shell\"+key);verb.SetValue("MUIVerb",L10n.Text(title));verb.SetValue("MultiSelectModel","Player");
            using var command=verb.CreateSubKey("command");command.SetValue("","\""+executable+"\" --explorer "+mode+" \"%1\"");
        }
    }
    internal static void Disable()=>Registry.CurrentUser.DeleteSubKeyTree(Root,false);
    /// Rewrites an enabled menu after the portable folder moves or the language changes.
    internal static void Refresh()
    {
        try{if(Enabled&&Environment.ProcessPath is {} executable)Enable(executable);}
        catch(Exception error)when(error is IOException or UnauthorizedAccessException or System.Security.SecurityException){/* The menu keeps its previous command. */}
    }
}
