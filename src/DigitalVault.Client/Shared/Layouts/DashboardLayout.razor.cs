using Microsoft.AspNetCore.Components;
using DigitalVault.Client.Services;

namespace DigitalVault.Client.Shared.Layouts
{
    public partial class DashboardLayout
    {
        [Inject] private VaultUnlockService VaultUnlockService { get; set; } = null!;
        // [Inject] private IDialogService DialogService { get; set; } = null!; // TODO: Migrate to Blazorise
        [Inject] private SecureStorageService SecureStorageService { get; set; } = null!;
        [Inject] private CryptoService CryptoService { get; set; } = null!;
        [Inject] private HttpClient HttpClient { get; set; } = null!;

        private bool _showModal = false;
        private bool _showDrawer = false;
        private bool _sidebarCollapsed = false;
        private string? _activePanelContent = null;
        private string _activeTab = "หน้าแรก";

        protected override async Task OnInitializedAsync()
        {
            await base.OnInitializedAsync();

            // Initialize Master Key if missing (for BFF authentication flow)
            await InitializeMasterKeyAsync();

            // TODO: Re-enable after migrating UnlockVaultModal to Blazorise
            // await CheckVaultLockStatusAsync();
        }


        private async Task InitializeMasterKeyAsync()
        {
            // Check if Master Key already exists in sessionStorage
            var existingKey = await SecureStorageService.GetMasterKeyAsync();
            if (!string.IsNullOrEmpty(existingKey))
            {
                Console.WriteLine("✅ Master Key already in sessionStorage");
                return;
            }

            Console.WriteLine("⚠️ Master Key not in sessionStorage - checking localStorage...");

            // DEVELOPMENT WORKAROUND: Check localStorage for a persistent dev key
            // In production, this should fetch encrypted key from database and decrypt with user password
            var persistentKey = await SecureStorageService.GetLocalItemAsync<string>("dev_master_key");

            if (!string.IsNullOrEmpty(persistentKey))
            {
                Console.WriteLine("✅ Found persistent dev master key in localStorage");
                await SecureStorageService.SaveMasterKeyAsync(persistentKey);
                Console.WriteLine("✅ Restored master key to sessionStorage");
            }
            else
            {
                Console.WriteLine("⚠️ No persistent key found - generating new master key");
                Console.WriteLine("⚠️ WARNING: Old encrypted documents will NOT be accessible with this new key!");

                // Generate a new master key
                var masterKey = await CryptoService.GenerateMasterKeyAsync();

                // Save to sessionStorage (current session)
                await SecureStorageService.SaveMasterKeyAsync(masterKey);

                // DEVELOPMENT WORKAROUND: Also save to localStorage (persists across sessions)
                // TODO: In production, encrypt this with user password and store in database
                await SecureStorageService.SetLocalItemAsync("dev_master_key", masterKey);

                Console.WriteLine("✅ New master key generated and saved to both sessionStorage and localStorage");
                Console.WriteLine("⚠️ TODO: Implement proper key retrieval from database with password encryption");
            }
        }

        /* TODO: Re-enable after migrating UnlockVaultModal to Blazorise
        private async Task CheckVaultLockStatusAsync()
        {
            // Check if vault is locked and encrypted key exists
            var isLocked = await VaultUnlockService.IsVaultLockedAsync();
            var hasEncryptedKey = await VaultUnlockService.HasEncryptedKeyAsync();

            if (isLocked && hasEncryptedKey)
            {
                // Show unlock dialog
                await ShowUnlockDialogAsync();
            }
        }

        private async Task ShowUnlockDialogAsync()
        {
            var options = new DialogOptions
            {
                CloseButton = false,
                MaxWidth = MaxWidth.Small,
                FullWidth = true
            };

            var dialog = await DialogService.ShowAsync<Components.UnlockVaultModal>(
                "Unlock Vault",
                options
            );

            var result = await dialog.Result;

            if (result.Canceled)
            {
                // User canceled - redirect to logout
                Navigation.NavigateTo("/logout", forceLoad: true);
            }
            else
            {
                // Vault unlocked successfully - refresh the page
                StateHasChanged();
            }
        }
        */

        public void OpenAddModal()
        {
            _showModal = true;
            StateHasChanged();
        }

        public void CloseModal()
        {
            _showModal = false;
            StateHasChanged();
        }

        public void ToggleDrawer()
        {
            _showDrawer = !_showDrawer;
            StateHasChanged();
        }

        public void CloseDrawer()
        {
            _showDrawer = false;
            StateHasChanged();
        }

        private void HandleMenuClick(string menuItem)
        {
            if (_activePanelContent == menuItem)
            {
                // Close panel if clicking the same item
                _sidebarCollapsed = false;
                _activePanelContent = null;
            }
            else
            {
                // Open panel with new content
                _sidebarCollapsed = true;
                _activePanelContent = menuItem;
            }
            StateHasChanged();
        }

        private void ClosePanel()
        {
            _sidebarCollapsed = false;
            _activePanelContent = null;
            StateHasChanged();
        }

        private void SetActiveTab(string tab)
        {
            _activeTab = tab;
            StateHasChanged();
        }

        private void SaveItem()
        {
            // Placeholder for saving logic
            CloseModal();
        }

        private void Logout()
        {
            Navigation.NavigateTo("/Logout", forceLoad: true);
        }
    }
}
