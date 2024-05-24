using System.Text.Json;
using IdentityModel.Client;
using Serilog;

Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .CreateBootstrapLogger();

var client = new HttpClient();

var discovery = await client.GetDiscoveryDocumentAsync("https://localhost:5001");

if (discovery.IsError)
{
    Log.Error(discovery.Error!);
    Log.Error(discovery.Exception!.InnerException!.Message);
    return;
}

// var tokenResponse = await client.RequestClientCredentialsTokenAsync(new ClientCredentialsTokenRequest
// {
//     Address = discovery.TokenEndpoint,
//     ClientId = "devClient",
//     ClientSecret = "devSecret",
//     Scope = "tournamentAPI"
// });

// if (tokenResponse.IsError)
// {
//     Log.Error(tokenResponse.Error!);
//     Log.Error(tokenResponse.ErrorDescription!);
//     Log.Error(JsonSerializer.Serialize(tokenResponse.ErrorType, JsonWriteOptions()));
//     return;
// }

// var apiClient = new HttpClient();

// apiClient.SetBearerToken(tokenResponse.AccessToken!);

// var response = await apiClient.GetAsync("https://localhost:3000/games");

// if (!response.IsSuccessStatusCode)
// {
//     Log.Information(response.StatusCode.ToString());
// }
// else
// {
//     var doc = JsonDocument.Parse(await response.Content.ReadAsStringAsync());

//     Log.Error(JsonSerializer.Serialize(doc, JsonWriteOptions()));
// }