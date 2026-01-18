using Microsoft.AspNetCore.Components;

namespace DigitalVault.Client.Shared.Layouts
{
    public partial class DashboardLayout
    {
        private bool _showModal = false;
        private bool _showDrawer = false;
        private bool _sidebarCollapsed = false;
        private string? _activePanelContent = null;
        private string _activeTab = "หน้าแรก";

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
