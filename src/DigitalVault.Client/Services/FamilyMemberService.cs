using System.Net.Http.Json;
using DigitalVault.Shared.DTOs.FamilyMembers;

namespace DigitalVault.Client.Services;

public class FamilyMemberService
{
    private readonly HttpClient _httpClient;

    public FamilyMemberService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<List<FamilyMemberDto>> GetFamilyMembersAsync()
    {
        return await _httpClient.GetFromJsonAsync<List<FamilyMemberDto>>("api/familymembers") ?? new List<FamilyMemberDto>();
    }

    public async Task<FamilyMemberDto?> GetFamilyMemberAsync(Guid id)
    {
        try
        {
            return await _httpClient.GetFromJsonAsync<FamilyMemberDto>($"api/familymembers/{id}");
        }
        catch (HttpRequestException ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
        {
            return null;
        }
    }
}
