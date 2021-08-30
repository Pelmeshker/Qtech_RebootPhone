using System.Net;
using System.Net.Http.Headers;
using System.Text;

for (int i = 1;  i<255; i++)
{
    Console.WriteLine("192.168.1." + i);
    var answer = RedirectRequest("http://192.168.1." + i);
}

static async Task<HttpResponseMessage> RedirectRequest(string address)
{
    var client = new HttpClient();

    try
    {
        HttpResponseMessage response;
        client.Timeout = TimeSpan.FromSeconds(1);
        var clientMethod = "/goform/Reboot";
        var user = "login";
        var password = "password";
        var base64String = Convert.ToBase64String(Encoding.ASCII.GetBytes($"{user}:{password}"));
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", base64String);
        var requestUri = new Uri(address + clientMethod);

        using (var requestMessage = new HttpRequestMessage(HttpMethod.Post, address + clientMethod))
        {
            response = client.SendAsync(requestMessage).Result;
        }

        if (response.StatusCode == HttpStatusCode.Unauthorized)
        {
            var finalRequestUri = response.RequestMessage.RequestUri; 
            Console.WriteLine(finalRequestUri);
            if (finalRequestUri != requestUri) 
            {

                Console.WriteLine(finalRequestUri.Host);
                if (("http://" + finalRequestUri.Host) == address) 
                {
                    return response = client.GetAsync(finalRequestUri).Result;
                }
            } else
            {
                Console.WriteLine("Аппарат не отработал, повторный запрос");
                return await RedirectRequest(address);
            }
        }

    }
    catch (Exception e)
    {

    }
    finally
    {
        client.Dispose();
    }

    return null;

}