﻿namespace Book.Services
{
    public class MessageSvc
    {
        public event Action? TransactionsChanged;

        public List<int> TransactionYears = new();

        public void ChangeTransactions (List<int> transactionYears)
        {
            TransactionYears = transactionYears;
            TransactionsChanged?.Invoke();
        }

    }
}
