using LogPort.Core;

var normalizer = new LogNormalizer();

var message = "User JohnDoe failed to login from IP 123.45.67.89";
var metadata = new Dictionary<string, object>
{
    ["username"] = "JohnDoe",
    ["ip_address"] = "123.45.67.89"
};
var normalizedMessage = normalizer.NormalizeMessage(message, metadata);
Console.WriteLine("Original Message: " + message);
Console.WriteLine("Normalized Message: " + normalizedMessage);

var hash = LogNormalizer.ComputePatternHash(normalizedMessage);
Console.WriteLine("Pattern Hash: " + hash);