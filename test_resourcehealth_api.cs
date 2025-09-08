using Azure.ResourceManager.ResourceHealth;
using Azure.ResourceManager;
using System;

// Test to explore Azure ResourceHealth API
// This is to understand what Service Health Event APIs are available

var armClient = new ArmClient(new Azure.Identity.DefaultAzureCredential());

// Check available methods on subscription resource for ResourceHealth
var subscription = armClient.GetSubscriptionResource(Azure.Core.ResourceIdentifier.Parse("/subscriptions/00000000-0000-0000-0000-000000000000"));

// Look for ServiceIssue related methods
// subscription.GetServiceIssues() -- Does this exist?
// subscription.GetServiceHealthEvents() -- Does this exist?

Console.WriteLine("Available ResourceHealth methods:");
Console.WriteLine("- GetAvailabilityStatusesBySubscription()");

// Check what's available in the ResourceHealth extension
// var serviceIssues = subscription.GetServiceIssues(); 

Console.WriteLine("Completed API exploration");
