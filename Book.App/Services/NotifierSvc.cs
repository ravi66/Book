namespace Book.Services
{
    public class NotifierSvc : INotifierSvc
    {
        public event EventHandler<bool>? ThemeChanged;

        public event EventHandler<int>? TransactionTypeDeleted;

        public event EventHandler<TransactionsChangedEventArgs>? TransactionsChanged;

        public event Action? SummaryTypeDeleted;

        public virtual void OnThemeChanged(object sender, bool darkMode)
        {
            ThemeChanged?.Invoke(sender, darkMode);
        }

        public virtual void OnTransactionsChanged(object sender, List<int> years)
        {
            TransactionsChangedEventArgs args = new() { Years = years };
            TransactionsChanged?.Invoke(sender, args);
        }

        public virtual void OnSummaryTypeDeleted()
        {
            SummaryTypeDeleted?.Invoke();
        }

        public virtual void OnTransactionTypeDeleted(object sender, int summaryTypeId)
        {
            TransactionTypeDeleted?.Invoke(sender, summaryTypeId);
        }
    }

    public class TransactionsChangedEventArgs : EventArgs
    {
        public List<int> Years { get; set; }
    }
}
