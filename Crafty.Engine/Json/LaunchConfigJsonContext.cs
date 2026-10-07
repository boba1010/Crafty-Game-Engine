using Crafty.Engine.Dtos;
using System.Text.Json.Serialization;

namespace Crafty.Engine.Json;

[JsonSerializable(typeof(GameLaunchConfigDto))]
public partial class LaunchConfigJsonContext : JsonSerializerContext
{
}
