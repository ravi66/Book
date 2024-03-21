namespace Book.Services
{
    public interface INotifierSvc
    {
        public event EventHandler<TransactionsChangedEventArgs> TransactionsChanged;
        public event Action? SummaryTypeDeleted;
        public event Action? TransactionTypeDeleted;

        public void OnTransactionsChanged(object sender, List<int> years);
        public void OnSummaryTypeDeleted();
        public void OnTransactionTypeDeleted();
    }
}
