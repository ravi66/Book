using Newtonsoft.Json;

namespace Book.Shared
{
    public class VisitsCounter (HttpClient HttpClient, string Host)
    {
        public string VisitsCountUriBase = "https://api.counterapi.dev/v1/RavsBook/";

        HttpResponseMessage HttpResponse { get; set; }

        record CounterContent(int Id, string Name, int Count);

        public async Task UpdateVisitsCount()
        {
            using var cts = new CancellationTokenSource();
            cts.CancelAfter(TimeSpan.FromSeconds(15));
            _ = await HttpClient.GetAsync($"{VisitsCountUriBase}{Host}/up", cts.Token);
        }

        public async Task<int> GetVisitsCount()
        {
            using var cts = new CancellationTokenSource();
            cts.CancelAfter(TimeSpan.FromSeconds(15));
            HttpResponse = await HttpClient.GetAsync($"{VisitsCountUriBase}{Host}", cts.Token);

            if (HttpResponse.IsSuccessStatusCode)
            {
                return JsonConvert.DeserializeObject<CounterContent>(await HttpResponse.Content.ReadAsStringAsync()).Count;
            }

            return 0;
        }
    }
}
