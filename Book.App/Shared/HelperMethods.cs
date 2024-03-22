namespace Book.Shared
{
    public static class HelperMethods
    {
        public static async Task MaximiseIfMobile(IBrowserViewportService browserViewportService, MudDialogInstance mudDialog)
        {
            var curBreakpoint = await browserViewportService.GetCurrentBreakpointAsync();
            if (curBreakpoint == Breakpoint.Xs)
            {
                mudDialog.Options.FullScreen = true;
                mudDialog.SetOptions(mudDialog.Options);
            }
        }
    }
}
