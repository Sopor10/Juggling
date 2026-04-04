using MudBlazor;

namespace Siteswaps.Generator.Components;

public class DialogTracker
{
    public IDialogReference? CurrentDialog { get; set; }

    public void Close()
    {
        CurrentDialog?.Close();
        CurrentDialog = null;
    }
}
