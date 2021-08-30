using System.Net;
using System.Net.Http.Headers;
using System.Text;

//В цикле перебирается вся подсеть и по очереди перезагружаются телефонные аппараты
for (int i = 1; i < 255; i++)
{
    Console.WriteLine("192.168.1." + i);
    var answer = RedirectRequest("http://192.168.1." + i);
    //На некоторых объектах аппараты игнорировали первую команду перезагрузки, 
    //поэтому на каждый адрес сделал по два запроса, решение не самое изящное, но работает безотказно
    var answer2 = RedirectRequest("http://192.168.1." + i);
    Console.WriteLine("Аппарат успешно перезагружен");
}



static async Task<HttpResponseMessage> RedirectRequest(string address)
{
    var client = new HttpClient();

    try
    {

        HttpResponseMessage response;

        //Блок авторизации, генерирующий заголовок авторизации с парой логин\пароль
        client.Timeout = TimeSpan.FromSeconds(1);
        var clientMethod = "/goform/Reboot";
        var user = "login";
        var password = "password";
        var base64String = Convert.ToBase64String(Encoding.ASCII.GetBytes($"{user}:{password}"));
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", base64String);

        //Непосредственно POST запрос
        var requestUri = new Uri(address + clientMethod);
        using (var requestMessage = new HttpRequestMessage(HttpMethod.Post, address + clientMethod))
        {
            response = client.SendAsync(requestMessage).Result;
        }

        //На этом этапе происходит редирект на страницу, которая отправляет команду перезагрузки,
        //но заголовки авторизации при этом теряются, поэтому пришлось самому сделать необходимый GET запрос
        if (response.StatusCode == HttpStatusCode.Unauthorized)
        {
            var finalRequestUri = response.RequestMessage.RequestUri;
            if (finalRequestUri != requestUri)
            {
                if (("http://" + finalRequestUri.Host) == address)
                {
                    return response = client.GetAsync(finalRequestUri).Result;
                }
            }
            else
            {
                //Изначально была идея запускать здесь циклично метод до победного, но это оказалось лишним
                return null;
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