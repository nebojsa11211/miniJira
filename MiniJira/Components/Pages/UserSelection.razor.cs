using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.JSInterop;
using MiniJira.Models;
using System.Linq;
using System.Threading.Tasks;

namespace MiniJira.Components.Pages;

public partial class UserSelection : IDisposable
{
    private List<User> users = new();
    private List<User> filteredUsers = new();
    private bool isLoading = true;
    private string? errorMessage;
    private string searchTerm = string.Empty;
    private ElementReference searchInputElement;
    private Guid? selectedUserId;
    private int loadingProgress = 0;
    private System.Timers.Timer? loadingTimer;
    private int selectedUserIndex = -1;

    protected override async System.Threading.Tasks.Task OnInitializedAsync()
    {
        // If user is already selected, redirect to board
        if (CurrentUserService.IsUserSelected())
        {
            NavigationManager.NavigateTo("/board", forceLoad: false);
            return;
        }

        // Subscribe to culture changes to re-render when language changes
        LocalizationService.CultureChanged += OnCultureChanged;

        await LoadUsers();
    }

    private void OnCultureChanged(object? sender, EventArgs e)
    {
        // Re-render component when culture changes
        InvokeAsync(StateHasChanged);
    }

    protected override async System.Threading.Tasks.Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            try
            {
                await JSRuntime.InvokeVoidAsync("initializeUserSelection");
            }
            catch
            {
                // Silently handle if JS interop fails
            }
        }
    }

    private async System.Threading.Tasks.Task LoadUsers()
    {
        isLoading = true;
        errorMessage = null;
        loadingProgress = 0;
        
        // Start loading progress animation
        StartLoadingProgress();

        try
        {
            users = await UserService.GetUsersAsync();
            filteredUsers = users.OrderBy(u => u.FullName).ToList();
        }
        catch (Exception ex)
        {
            errorMessage = $"Failed to load users: {ex.Message}";
        }
        finally
        {
            isLoading = false;
            StopLoadingProgress();
            
            // Focus search input after loading
            if (!isLoading && errorMessage == null)
            {
                await FocusSearchInput();
            }
        }
    }

    private void StartLoadingProgress()
    {
        loadingTimer = new System.Timers.Timer(50);
        loadingTimer.Elapsed += (sender, e) =>
        {
            InvokeAsync(() =>
            {
                loadingProgress = Math.Min(loadingProgress + 2, 90);
                StateHasChanged();
            });
        };
        loadingTimer.Start();
    }

    private void StopLoadingProgress()
    {
        loadingTimer?.Stop();
        loadingTimer?.Dispose();
        loadingTimer = null;
        loadingProgress = 100;
    }

    private async System.Threading.Tasks.Task RetryLoadUsers()
    {
        await LoadUsers();
    }

    private void SelectUser(User user)
    {
        CurrentUserService.SetCurrentUser(user);
        NavigationManager.NavigateTo("/board");
    }

    private string GetInitials(string fullName)
    {
        if (string.IsNullOrWhiteSpace(fullName))
            return "?";

        var parts = fullName.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        if (parts.Length == 1)
            return parts[0].Substring(0, Math.Min(2, parts[0].Length)).ToUpper();

        return $"{parts[0][0]}{parts[^1][0]}".ToUpper();
    }

    private string GetAvatarGradient(Guid userId)
    {
        var gradients = new[]
        {
            "linear-gradient(135deg, #667eea 0%, #764ba2 100%)",
            "linear-gradient(135deg, #f093fb 0%, #f5576c 100%)",
            "linear-gradient(135deg, #4facfe 0%, #00f2fe 100%)",
            "linear-gradient(135deg, #43e97b 0%, #38f9d7 100%)",
            "linear-gradient(135deg, #fa709a 0%, #fee140 100%)",
            "linear-gradient(135deg, #a8edea 0%, #fed6e3 100%)",
            "linear-gradient(135deg, #ff9a9e 0%, #fecfef 100%)",
            "linear-gradient(135deg, #a18cd1 0%, #fbc2eb 100%)"
        };
        
        return gradients[Math.Abs(userId.GetHashCode()) % gradients.Length];
    }

    private void FilterUsers()
    {
        if (string.IsNullOrWhiteSpace(searchTerm))
        {
            filteredUsers = users.OrderBy(u => u.FullName).ToList();
        }
        else
        {
            var term = searchTerm.ToLower();
            filteredUsers = users
                .Where(u => u.FullName.ToLower().Contains(term) || 
                           u.Email.ToLower().Contains(term))
                .OrderBy(u => u.FullName)
                .ToList();
        }
        
        selectedUserIndex = -1;
        selectedUserId = null;
    }

    private void ClearSearch()
    {
        searchTerm = string.Empty;
        FilterUsers();
    }

    private async System.Threading.Tasks.Task FocusSearchInput()
    {
        try
        {
            await JSRuntime.InvokeVoidAsync("focusElement", searchInputElement);
        }
        catch
        {
            // Silently handle if JS interop fails
        }
    }

    private void HandleSearchKeyUp(KeyboardEventArgs e)
    {
        FilterUsers();
        
        if (e.Key == "ArrowDown" && filteredUsers.Any())
        {
            selectedUserIndex = 0;
            selectedUserId = filteredUsers[0].Id;
            StateHasChanged();
        }
        else if (e.Key == "Escape")
        {
            ClearSearch();
        }
    }

    private async System.Threading.Tasks.Task HandleUserCardKeyDown(KeyboardEventArgs e, User user)
    {
        switch (e.Key)
        {
            case "Enter":
                SelectUser(user);
                break;
            case "ArrowDown":
                NavigateUser(1);
                break;
            case "ArrowUp":
                NavigateUser(-1);
                break;
            case "Escape":
                selectedUserIndex = -1;
                selectedUserId = null;
                await FocusSearchInput();
                break;
        }
    }

    private void NavigateUser(int direction)
    {
        if (!filteredUsers.Any()) return;
        
        selectedUserIndex += direction;
        
        if (selectedUserIndex < 0)
        {
            selectedUserIndex = filteredUsers.Count - 1;
        }
        else if (selectedUserIndex >= filteredUsers.Count)
        {
            selectedUserIndex = 0;
        }
        
        selectedUserId = filteredUsers[selectedUserIndex].Id;
        StateHasChanged();
    }

    public void Dispose()
    {
        loadingTimer?.Dispose();
        LocalizationService.CultureChanged -= OnCultureChanged;
    }
}
