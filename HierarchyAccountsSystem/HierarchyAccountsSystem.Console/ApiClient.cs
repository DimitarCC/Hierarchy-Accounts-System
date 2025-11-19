using System;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;

namespace HierarchyAccountsSystem.ConsoleApp;

internal sealed class ApiClient : IDisposable {
  private readonly HttpClient _Http;

  public ApiClient(string baseUrl, bool ignoreServerCertificate = false) {
    var handler = new HttpClientHandler();
    if (ignoreServerCertificate) {
      handler.ServerCertificateCustomValidationCallback = HttpClientHandler.DangerousAcceptAnyServerCertificateValidator;
    }
    this._Http = new HttpClient(handler) { BaseAddress = new Uri(baseUrl) };
  }

  public async Task<HierarhycalAccountDto?> GetAccountTreeAsync(int? accountId) {
    var reqUri = "api/v1/account/gettree";
    if (accountId.HasValue) reqUri += $"?accountId={accountId.Value}";

    using var resp = await _Http.GetAsync(reqUri);
    var content = await resp.Content.ReadAsStringAsync();

    if (!resp.IsSuccessStatusCode) {
      throw new InvalidOperationException($"Request failed: {(int)resp.StatusCode} {resp.ReasonPhrase} - {content}");
    }

    var jsonOpts = new JsonSerializerOptions {
      PropertyNameCaseInsensitive = true,
      WriteIndented = true
    };

    return JsonSerializer.Deserialize<HierarhycalAccountDto>(content, jsonOpts);
  }

  public void Dispose() => this._Http.Dispose();
}