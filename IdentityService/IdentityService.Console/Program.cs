using System.Text.Json;
using IdentityModel.Client;
using Serilog;

Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Debug()
    .WriteTo.Console(new Serilog.Formatting.Json.JsonFormatter())
    .CreateLogger();

var handler = new HttpClientHandler()
{
    ServerCertificateCustomValidationCallback = (message, cert, chain, errors) => { return true; }
};

var client = new HttpClient(handler);

var discovery = await client.GetDiscoveryDocumentAsync("https://localhost:5001");

if (discovery.IsError)
{
    Log.Error(discovery.Error!);
    Log.Error(discovery.Exception!.InnerException!.Message);
    return;
}

var tokenResponse = await client.RequestClientCredentialsTokenAsync(new ClientCredentialsTokenRequest
{
    Address = discovery.TokenEndpoint,
    ClientId = "dev-console",
    ClientSecret = "dev-console-secret",
    Scope = "tournamentAPI"
});

if (tokenResponse.IsError)
{
    Log.Error(tokenResponse.Error!);
    Log.Error(tokenResponse.Exception!.InnerException!.Message!);
    return;
}

var apiClient = new HttpClient();

apiClient.SetBearerToken(tokenResponse.AccessToken!);

var response = await apiClient.GetAsync("https://localhost:3000/Games");

if (!response.IsSuccessStatusCode)
{
    Log.Error(response.StatusCode.ToString());
}
else
{
    var doc = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
    Console.WriteLine(JsonSerializer.Serialize(doc, new JsonSerializerOptions { WriteIndented = true }));
}