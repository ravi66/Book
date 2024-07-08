namespace Book.Services
{
    public interface INotifierSvc
    {
        public event EventHandler<bool> ThemeChanged;
        public event EventHandler<int> TransactionTypeDeleted;
        public event EventHandler<TransactionsChangedEventArgs> TransactionsChanged;
        public event EventHandler<int> TransactionTypeCreated;
        public event Action? SummaryTypeDeleted;
        public event EventHandler<int> SummaryTypeCreated;

        public void OnThemeChanged(object sender, bool darkMode);
        public void OnTransactionsChanged(object sender, List<int> years, int SummaryTypeId);
        public void OnTransactionTypeDeleted(object sender, int summaryTypeId);
        public void OnTransactionTypeCreated(object sender, int summaryTypeId);
        public void OnSummaryTypeDeleted();
        public void OnSummaryTypeCreated(object sender, int summaryTypeId);
    }
}
