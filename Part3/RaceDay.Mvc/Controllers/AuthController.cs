using Microsoft.AspNetCore.Mvc;
using RaceDay.Mvc.Models;
using RaceDay.Mvc.Services;

namespace RaceDay.Mvc.Controllers;

public class AuthController : Controller
{
    private readonly ApiService _api;

    public AuthController(ApiService api)
    {
        _api = api;
    }

    [HttpGet]
    public IActionResult Register() => View();

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Register(RegisterViewModel model)
    {
        Console.WriteLine("REGISTRATION BUTTON WAS CLICKED!");
        if (!ModelState.IsValid)
            return View(model);

        try
        {
            var result = await _api.PostAsync("api/auth/register", new
            {
                model.Name,
                model.Email,
                model.Password,
                model.Role
            });

            TempData["Success"] = "Account created. You can now log in.";
            return RedirectToAction("Login");
        }
        catch (HttpRequestException)
        {
            ModelState.AddModelError("", "Registration failed. Check whether the email is already registered and try again.");
            return View(model);
        }
    }

    [HttpGet]
    public IActionResult Login() => View();

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(LoginViewModel model)
    {
        if (!ModelState.IsValid)
            return View(model);

        try
        {
            var result = await _api.PostAsync("api/auth/login", new
            {
                model.Email,
                model.Password
            });

            HttpContext.Session.SetInt32(
                "UserId", result.GetProperty("userId").GetInt32());

            HttpContext.Session.SetString(
                "Name", result.GetProperty("name").GetString() ?? "");

            HttpContext.Session.SetString(
                "Role", result.GetProperty("role").GetString() ?? "");

            HttpContext.Session.SetString(
                "Email", result.GetProperty("email").GetString() ?? "");

            return RedirectToAction("Index", "Home");
        }
        catch (HttpRequestException)
        {
            ModelState.AddModelError("", "Invalid email or password, or the API is unavailable.");
            return View(model);
        }
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Logout()
    {
        try
        {
            await _api.PostAsync<object?>("api/auth/logout", null);
        }
        catch
        {
            // Clear the MVC login even if the API logout request fails.
        }

        HttpContext.Session.Clear();
        return RedirectToAction("Index", "Home");
    }
}