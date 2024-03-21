﻿namespace Book.Services
{
    public class NotifierSvc : INotifierSvc
    {
        public event EventHandler<TransactionsChangedEventArgs>? TransactionsChanged;

        public event Action? SummaryTypeDeleted;

        public event Action? TransactionTypeDeleted;

        public virtual void OnTransactionsChanged(object sender, List<int> years)
        {
            TransactionsChangedEventArgs args = new() { Years = years };
            TransactionsChanged?.Invoke(sender, args);
        }

        public virtual void OnSummaryTypeDeleted()
        {
            SummaryTypeDeleted?.Invoke();
        }

        public virtual void OnTransactionTypeDeleted()
        {
            TransactionTypeDeleted?.Invoke();
        }

    }

    public class TransactionsChangedEventArgs : EventArgs
    {
        public List<int> Years { get; set; }
    }
}
