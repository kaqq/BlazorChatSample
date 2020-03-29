using System.Threading.Tasks;

namespace BlazorChatSample.Shared
{
    public interface IServerAgent
    {
        Task SendMessage(string message);
        Task ExitGroup(string name);
    }


}
