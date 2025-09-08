using Azure.ResourceManager.ResourceHealth;
using Azure.ResourceManager;

// Let me create a simple test to see what ServiceIssue methods are available

var armClient = new ArmClient(new Azure.Identity.DefaultAzureCredential());

// Look for service issues methods available in ResourceHealth
var subscription = armClient.GetSubscriptionResource(Azure.Core.ResourceIdentifier.Parse("/subscriptions/00000000-0000-0000-0000-000000000000"));

// Check if we have subscription.GetServiceIssues() available
// var serviceIssues = subscription.GetServiceIssues();

Console.WriteLine("Test complete");
