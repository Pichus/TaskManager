using System.Text.Json.Serialization;
using TaskManager.Core.ProjectAggregate;

namespace TaskManager.ProjectMembers.Update;

public class UpdateProjectMemberRequest
{
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public Role Role { get; set; }
}