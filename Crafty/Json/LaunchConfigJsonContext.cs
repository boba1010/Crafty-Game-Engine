using Crafty.Dtos;
using System.Text.Json.Serialization;

namespace Crafty.Json;

[JsonSerializable(typeof(GameLaunchConfigDto))]
public partial class LaunchConfigJsonContext : JsonSerializerContext
{
}
