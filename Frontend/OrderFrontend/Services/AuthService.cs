using Microsoft.JSInterop;

namespace OrderFrontend.Services;

public class AuthService
{
    private readonly IJSRuntime _jsRuntime;
    private bool _initialized;

    public bool IsLoggedIn { get; private set; }
    public bool IsInitialized => _initialized;

    public event Action? OnChange;

    public AuthService(IJSRuntime jsRuntime)
    {
        _jsRuntime = jsRuntime;
    }

    public async Task InitializeAsync()
    {
        if (_initialized)
            return;

        var savedValue = await _jsRuntime.InvokeAsync<string?>("localStorage.getItem", "isLoggedIn");
        IsLoggedIn = savedValue == "true";
        _initialized = true;

        NotifyStateChanged();
    }

    public async Task<bool> LoginAsync(string email, string password)
    {
        var isValid =
            email.Trim().Equals("admin@demo.com", StringComparison.OrdinalIgnoreCase) &&
            password == "admin123";

        if (!isValid)
            return false;

        IsLoggedIn = true;
        _initialized = true;

        await _jsRuntime.InvokeVoidAsync("localStorage.setItem", "isLoggedIn", "true");
        NotifyStateChanged();

        return true;
    }

    public async Task LogoutAsync()
    {
        IsLoggedIn = false;
        _initialized = true;

        await _jsRuntime.InvokeVoidAsync("localStorage.removeItem", "isLoggedIn");
        NotifyStateChanged();
    }

    private void NotifyStateChanged()
    {
        OnChange?.Invoke();
    }
}