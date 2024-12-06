namespace Silo.Grains
{
    /// <summary>
    /// Grain dedicated to match one chat user
    /// </summary>
    public interface IUserGrain : IGrainWithStringKey
    {
        /// <summary>
        /// Log a userand return all the room list it participate
        /// </summary>
        Task<IReadOnlyCollection<string>> LoginAsync();

        /// <summary>
        /// Determines whether this user is logged.
        /// </summary>
        Task<bool> IsLogged();

        /// <summary>
        /// Joins a room.
        /// </summary>
        Task JoinRoom(string roomName);

        /// <summary>
        /// Leave a room.
        /// </summary>
        Task LeaveRoom(string roomName);

        /// <summary>
        /// Gets the chat room list.
        /// </summary>
        Task<IReadOnlyCollection<string>> GetChatRoomList();

        /// <summary>
        /// Logouts this user.
        /// </summary>
        Task Logout();
    }
}
