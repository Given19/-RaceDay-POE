using Microsoft.AspNetCore.Mvc;
using RaceDay.Mvc.Services;

namespace RaceDay.Mvc.Controllers;

public class ApiTestController : Controller
{
    private readonly ApiService _apiService;

    public ApiTestController(ApiService apiService)
    {
        _apiService = apiService;
    }

    public async Task<IActionResult> Index()
    {
        try
        {
            var result = await _apiService.GetAsync("api/");

            ViewBag.ApiResponse = result.ToString();
            ViewBag.ApiStatus = "API connected successfully!";
        }
        catch (Exception ex)
        {
            ViewBag.ApiStatus = "API connection failed: " + ex.Message;
        }

        return View();
    }
}