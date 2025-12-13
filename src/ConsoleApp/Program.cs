using LogPort.Core;

var normalizer = new LogNormalizer();

var samples = new[]
{
    "User JohnDoe failed to login from IP 123.45.67.89",
    "2025-01-12 14:33:21.456 Request completed in 123ms",
    "2025-01-12T14:33:21Z CorrelationId=3f2504e0-4f89-41d3-9a0c-0305e82c3301",
    "Error in C:\\Services\\Auth\\LoginService.cs at line 248",
    "2025-01-12T14:33:21.987Z Failed to open /var/log/nginx/access.log",
    "Job 42 failed after 3 retries (JobId=6fa459ea-ee8a-3ca4-894e-db77e160355e)",
    "2025-01-12 14:33:21,123 [ERROR] Request 9812 failed in C:\\Api\\OrdersController.cs:87",
    "at MyApp.Services.UserService.Login(User user) in /src/UserService.cs:line 152",
    "User authentication failed"
};

var metadata = new Dictionary<string, object>
{
    ["username"] = "JohnDoe",
    ["ip_address"] = "123.45.67.89"
};

foreach (var message in samples)
{
    Console.WriteLine(normalizer.NormalizeMessage(message, metadata));
    Console.WriteLine(LogNormalizer.ComputePatternHash(normalizer.NormalizeMessage(message, metadata)));
}