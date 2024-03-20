namespace Book.Services
{
    public interface INotifierSvc
    {
        public event EventHandler<TransactionsChangedEventArgs> TransactionsChanged;
        public void OnTransactionsChanged(object sender, List<int> years); //TransactionsChangedEventArgs args);
    }
}
