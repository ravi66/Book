namespace Book.Models
{
    interface ISummaryTypeRepository
    {
        public Task<List<SummaryType>> GetAllSummaryTypes();
        public Task<List<SummaryType>> GetAutoCompleteList();
        public Task<SummaryType> GetSummaryTypeById(int summaryTypeId);
        public Task<SummaryType> AddSummaryType(SummaryType summaryType);
        public Task<SummaryType?> UpdateSummaryType(SummaryType summaryType);
        public Task DeleteSummaryType(int summaryTypeId);
        public Task<List<SummaryType>> LoadSummary();
        public Task<string> GetColour(int summaryTypeId);
        public Task<List<SummaryType>> Export();
        public Task<DateTime?> GetLastUpdDt();
        public Task<bool> IsEmptyDb();
    }
}