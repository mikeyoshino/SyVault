using Microsoft.AspNetCore.Components;

namespace DigitalVault.Client.Shared.Layouts
{
    public partial class MainLayout
    {
        private bool _showModal = false;

        public void OpenModal()
        {
            _showModal = true;
            StateHasChanged();
        }

        public void CloseModal()
        {
            _showModal = false;
            StateHasChanged();
        }

        private void SaveNote()
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
