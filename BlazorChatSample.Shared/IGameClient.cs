using System.Collections.Generic;
using System.Threading.Tasks;

namespace BlazorChatSample.Shared
{
    public interface IGameClient
    {
         Task ReceiveMessage(string user, string message);
         Task NewGroupAdded(Group @group);
         Task OnClientJoined(string username);
         Task OnGroupDataInfo(List<Group> groups);
         Task OnClientLeaved(string username);
         Task OnGroupRemoved(string groupName);
    }
}