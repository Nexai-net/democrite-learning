namespace Silo.Grains
{
    using Silo.Models;

    using System.Threading.Tasks;

    public interface IChatRoomGrain : IGrainWithStringKey
    {
        /// <summary>
        /// Gets the room etag.
        /// </summary>
        /// <remarks>
        ///     This technique allow regular check, if etag change then room data have changed
        /// </remarks>
        Task<string> GetRoomEtag();

        /// <summary>
        /// A specific user join the room
        /// </summary>
        Task Join(string userName);

        /// <summary>
        /// Gets the room participants.
        /// </summary>
        Task<IReadOnlyCollection<string>> GetParticipants();

        /// <summary>
        /// A specific user leave the room
        /// </summary>
        Task Leave(string userName);

        /// <summary>
        /// Gets the missing messages.
        /// </summary>
        Task<IReadOnlyCollection<ChatMessage>> GetMissingMessage(Guid? lastReceivedMessageId);

        /// <summary>
        /// Sends the message.
        /// </summary>
        Task<ChatMessage> SendMessage(string username, string message);
    }
}
