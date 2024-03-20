using Newtonsoft.Json;

namespace Book.Shared
{
    public class VisitsCounter (HttpClient HttpClient, string Host)
    {
        HttpResponseMessage HttpResponse { get; set; }

        record CounterContent(int Id, string Name, int Count);

        public async Task UpdateVisitsCount()
        {
            using var cts = new CancellationTokenSource();
            cts.CancelAfter(TimeSpan.FromSeconds(15));

            try
            {
                _ = await HttpClient.GetAsync($"{Constants.VisitsCountBaseUri}{Host}/up", cts.Token);
            }
            catch (Exception ex)
            {
#if DEBUG
                Console.WriteLine(ex.ToString());
#endif
            }
        }

        public async Task<int> GetVisitsCount()
        {
            using var cts = new CancellationTokenSource();
            cts.CancelAfter(TimeSpan.FromSeconds(15));

            try
            {
                HttpResponse = await HttpClient.GetAsync($"{Constants.VisitsCountBaseUri}{Host}", cts.Token);
            }
            catch (Exception ex)
            {
#if DEBUG
                Console.WriteLine(ex.ToString());
#endif
                return -1;
            }

            if (HttpResponse.IsSuccessStatusCode)
            {
                return JsonConvert.DeserializeObject<CounterContent>(await HttpResponse.Content.ReadAsStringAsync()).Count;
            }

            return 0;
        }
    }
}
