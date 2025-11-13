using Azure.Identity;
using System.IdentityModel.Tokens.Jwt;

var scope = args.Length > 0 ? args[0] : "https://vault.azure.net/.default";
string token;

if (scope == "SYSTEM_ACCESSTOKEN")
{
    token = Environment.GetEnvironmentVariable("SYSTEM_ACCESSTOKEN") ?? throw new InvalidOperationException("SYSTEM_ACCESSTOKEN environment variable is not set.");
}
else
{
    var credential = new DefaultAzureCredential();
    var tokenRequestContext = new Azure.Core.TokenRequestContext(new[] { scope });
    var response = await credential.GetTokenAsync(tokenRequestContext);
    token = response.Token;
}

var handler = new JwtSecurityTokenHandler();
JwtSecurityToken jwtToken = handler.ReadJwtToken(token);

Console.WriteLine("JWT Token Payload:");
Console.WriteLine("==================");
foreach (var claim in jwtToken.Claims)
{
    Console.WriteLine($"{claim.Type}: {claim.Value}");
}
Console.WriteLine();
Console.WriteLine($"Issued At: {jwtToken.IssuedAt}");
Console.WriteLine($"Valid From: {jwtToken.ValidFrom}");
Console.WriteLine($"Valid To: {jwtToken.ValidTo}");
Console.WriteLine();

var now = DateTimeOffset.UtcNow;
var start = jwtToken.IssuedAt > jwtToken.ValidFrom ? jwtToken.IssuedAt : jwtToken.ValidFrom;
var end = jwtToken.ValidTo;

var tokenAge = now - jwtToken.IssuedAt;
var remainingLife = end - now;
var lifespan = end - start;
Console.WriteLine($"Token Lifespan: {lifespan}");
Console.WriteLine($"Token Age: {tokenAge}");
Console.WriteLine($"Remaining Life: {remainingLife}");
