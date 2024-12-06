namespace Silo.Models
{
    [GenerateSerializer()]
    public sealed class ChatRoomState
    {
        #region Fields

        private readonly List<ChatMessage> _messages;
        private readonly List<string> _participants;

        #endregion

        #region Ctor

        /// <summary>
        /// Initializes a new instance of the <see cref="ChatRoomState"/> class.
        /// </summary>
        public ChatRoomState(IReadOnlyCollection<ChatMessage>? messages, IReadOnlyCollection<string>? participants, string? etag)
        {
            this._messages = messages?.ToList() ?? new List<ChatMessage>();
            this.Messages = this._messages;

            this._participants = participants?.ToList() ?? new List<string>();
            this.Participants = this._participants;

            this.Etag = etag!;
            if (string.IsNullOrEmpty(this.Etag))
                UpdateEtag(this);
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets the messages.
        /// </summary>
        [Id(0)]
        public IReadOnlyCollection<ChatMessage> Messages { get; }

        /// <summary>
        /// Gets the participants.
        /// </summary>
        [Id(1)]
        public IReadOnlyCollection<string> Participants { get; }

        /// <summary>
        /// Gets or sets the e tag.
        /// </summary>
        [Id(2)]
        public string Etag { get; set; }

        #endregion

        #region Methods

        /// <summary>
        /// Updates the etag.
        /// </summary>
        public static void UpdateEtag(ChatRoomState chatRoom)
        {
            chatRoom.Etag = Guid.NewGuid().ToString().ToLower().Replace("-", "")[..10];
        }

        /// <summary>
        /// Adds the message.
        /// </summary>
        internal ChatMessage AddMessage(string username, string message)
        {
            var msg = new ChatMessage(Guid.NewGuid(), username, message, DateTime.UtcNow);

            this._messages.Add(msg);
            UpdateEtag(this);

            return msg;
        }

        /// <summary>
        /// Joins the specified user name.
        /// </summary>
        internal bool Join(string userName)
        {
            if (this._participants.Contains(userName))
                return false;

            this._participants.Add(userName);
            UpdateEtag(this);
            return true;
        }

        /// <summary>
        /// Leaves the specified user name.
        /// </summary>
        internal bool Leave(string userName)
        {
            return this._participants.Remove(userName);
        }

        #endregion
    }
}
