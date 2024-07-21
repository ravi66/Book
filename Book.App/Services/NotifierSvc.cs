namespace Book.Services
{
    public class NotifierSvc : INotifierSvc
    {
        public event EventHandler<bool>? ThemeChanged;

        public event EventHandler<int>? TransactionTypeDeleted;

        public event EventHandler<TransactionsChangedEventArgs>? TransactionsChanged;

        public event EventHandler<int>? TransactionTypeCreated;

        public event Action? SummaryTypeDeleted;

        public event EventHandler<int>? SummaryTypeCreated;

        public virtual void OnThemeChanged(object sender, bool darkMode) => ThemeChanged?.Invoke(sender, darkMode);

        public virtual void OnTransactionsChanged(object sender, List<int> years, int summaryTypeId)
        {
            TransactionsChangedEventArgs args = new() { Years = years, SummaryTypeId = summaryTypeId };
            TransactionsChanged?.Invoke(sender, args);
        }

        public virtual void OnSummaryTypeDeleted() => SummaryTypeDeleted?.Invoke();

        public virtual void OnSummaryTypeCreated(object sender, int summaryTypeId) => SummaryTypeCreated?.Invoke(sender, summaryTypeId);

        public virtual void OnTransactionTypeDeleted(object sender, int summaryTypeId) => TransactionTypeDeleted?.Invoke(sender, summaryTypeId);

        public virtual void OnTransactionTypeCreated(object sender, int summaryTypeId) => TransactionTypeCreated?.Invoke(sender, summaryTypeId);
    }

    public class TransactionsChangedEventArgs : EventArgs
    {
        public List<int> Years { get; set; } = [];
        public int SummaryTypeId { get; set; }
    }
}
