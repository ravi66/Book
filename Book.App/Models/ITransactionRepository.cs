﻿namespace Book.Models
{
    interface ITransactionRepository
    {
        public Task<Transaction?> GetTransactionById(int transactionId);
        public Task AddTransaction(Transaction transaction);
        public Task AddTransactions(IEnumerable<Transaction> transactions);
        public Task UpdateTransaction(Transaction transaction);
        public Task DeleteTransaction(int transactionId);
        public Task<IEnumerable<Transaction>> GetTransactionsByTypeMonth(List<int>? types, int year, int month);
        public Task<IEnumerable<Transaction>> GetTransactionsBySummary(List<int>? types);
        public Task<IEnumerable<Transaction>> GetTransactionsByType(int typeId);
        public Task<List<Transaction>> Export();
        public Task<DateTime?> GetLastUpdDt();
        public Task<int> DeleteAllTransactions();
    }
}