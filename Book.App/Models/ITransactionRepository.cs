namespace Book.Models
{
    interface ITransactionRepository
    {
        public Task<Transaction?> GetTransactionById(int transactionId);
        public Task<Transaction> AddTransaction(Transaction transaction);
        public Task AddTransactions(IEnumerable<Transaction> transactions);
        public Task<Transaction> UpdateTransaction(Transaction transaction);
        public Task DeleteTransaction(int transactionId);
        public Task<IEnumerable<Transaction>> GetTransactionsByTypeMonth(List<int>? types, int year, int month);
        public Task<IEnumerable<Transaction>> GetTransactionsBySummary(List<int>? types, DateTime startDate, DateTime endDate);
        public Task<IEnumerable<Transaction>> GetTransactionsByType(int typeId, DateTime startDate, DateTime endDate);
    }
}
