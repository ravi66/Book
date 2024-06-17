namespace Book.Services
{
    public interface INotifierSvc
    {
        public event EventHandler<bool> ThemeChanged;
        public event EventHandler<int> TransactionTypeDeleted;
        public event EventHandler<TransactionsChangedEventArgs> TransactionsChanged;
        public event Action? SummaryTypeDeleted;

        public void OnThemeChanged(object sender, bool darkMode);
        public void OnTransactionsChanged(object sender, List<int> years);
        public void OnSummaryTypeDeleted();
        public void OnTransactionTypeDeleted(object sender, int summaryTypeId);
    }
}
