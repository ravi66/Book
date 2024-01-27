namespace Book.Models
{
    interface ITransactionTypeRepository
    {
        public Task<IEnumerable<TransactionType>> GetAllTransactionTypes();
        public Task<TransactionType?> GetTransactionTypeById(int transactionTypeId);
        public Task<TransactionType> AddTransactionType(TransactionType transactionType);
        public Task<TransactionType?> UpdateTransactionType(TransactionType transactionType);
        public Task DeleteTransactionType(int transactionTypeId);
    }
}
