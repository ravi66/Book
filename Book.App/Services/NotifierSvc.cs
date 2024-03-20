using Book.Models;

namespace Book.Services
{
    public class NotifierSvc : INotifierSvc
    {
        public event EventHandler<TransactionsChangedEventArgs>? TransactionsChanged;

        public virtual void OnTransactionsChanged(object sender, List<int> years) // TransactionsChangedEventArgs args)
        {
            TransactionsChangedEventArgs args = new() { Years = years };
            TransactionsChanged?.Invoke(sender, args);
        }
    }

    public class TransactionsChangedEventArgs : EventArgs
    {
        public List<int> Years { get; set; }
    }
}
