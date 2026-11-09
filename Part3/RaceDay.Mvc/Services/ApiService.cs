using System.Net.Http.Json;
using System.Text.Json;

namespace RaceDay.Mvc.Services;

public class ApiService
{
    private readonly HttpClient _http;
    private readonly IHttpContextAccessor _accessor;

    public ApiService(HttpClient http, IHttpContextAccessor accessor)
    {
        _http = http;
        _accessor = accessor;
    }

    private HttpRequestMessage CreateRequest(HttpMethod method, string endpoint)
    {
        var request = new HttpRequestMessage(method, endpoint);

        var cookie = _accessor.HttpContext?.Session
            .GetString("RaceDayApiCookie");

        if (!string.IsNullOrEmpty(cookie))
        {
            request.Headers.TryAddWithoutValidation("Cookie", cookie);
        }

        return request;
    }

    private void SaveApiCookie(HttpResponseMessage response)
    {
        if (response.Headers.TryGetValues("Set-Cookie", out var cookies))
        {
            var sessionCookie = cookies.FirstOrDefault(c =>
                c.StartsWith(".AspNetCore.Session=", StringComparison.OrdinalIgnoreCase));

            if (sessionCookie != null)
            {
                var cookieValue = sessionCookie.Split(';')[0];

                _accessor.HttpContext?.Session.SetString(
                    "RaceDayApiCookie", cookieValue);
            }
        }
    }

    public async Task<JsonElement> GetAsync(string endpoint)
    {
        using var request = CreateRequest(HttpMethod.Get, endpoint);
        using var response = await _http.SendAsync(request);

        SaveApiCookie(response);
        response.EnsureSuccessStatusCode();

        return await response.Content.ReadFromJsonAsync<JsonElement>();
    }

    public async Task<JsonElement> PostAsync<T>(string endpoint, T data)
    {
        using var request = CreateRequest(HttpMethod.Post, endpoint);

        request.Content = JsonContent.Create(data);

        using var response = await _http.SendAsync(request);

        SaveApiCookie(response);
        response.EnsureSuccessStatusCode();

        if (response.Content.Headers.ContentLength == 0 ||
            response.Content.Headers.ContentType == null)
        {
            return default;
        }

        return await response.Content.ReadFromJsonAsync<JsonElement>();
    }
}