namespace Silo.Implementations
{
    using Silo.Grains;
    using Silo.Models;

    using System;
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;

    internal sealed class ChatRoomGrain : Grain<ChatRoomState>, IChatRoomGrain
    {
        #region Fields

        private readonly IGrainFactory _grainFactory;

        #endregion

        #region Ctor

        /// <summary>
        /// Initializes a new instance of the <see cref="ChatRoomGrain"/> class.
        /// </summary>
        public ChatRoomGrain([PersistentState("ChatRooms")] IPersistentState<ChatRoomState> storage,
                              IGrainFactory grainFactory)
            : base(storage)
        {
            this._grainFactory = grainFactory;
        }

        #endregion

        #region Methods

        /// <inheritdoc />
        public Task<IReadOnlyCollection<ChatMessage>> GetMissingMessage(Guid? lastReceivedMessageId)
        {
            var lastMsg = this.State.Messages.FirstOrDefault(m => m.MessageId == lastReceivedMessageId);

            IEnumerable<ChatMessage> msg = this.State.Messages;

            if (lastMsg != null)
                msg = msg.Where(d => d.UTCCreationTime >= lastMsg.UTCCreationTime);

            var missings = msg.ToArray();
            return Task.FromResult<IReadOnlyCollection<ChatMessage>>(missings);
        }

        /// <inheritdoc />
        public Task<IReadOnlyCollection<string>> GetParticipants()
        {
            return Task.FromResult(this.State.Participants);
        }

        /// <inheritdoc />
        public Task<string> GetRoomEtag()
        {
            return Task.FromResult(this.State.Etag);
        }

        /// <inheritdoc />
        public async Task Join(string userName)
        {
            var added = this.State.Join(userName);
            if (added)
                await WriteStateAsync();
        }

        /// <inheritdoc />
        public async Task Leave(string userName)
        {
            var leaved = this.State.Leave(userName);
            if (leaved)
                await WriteStateAsync();
        }

        /// <inheritdoc />
        public async Task<ChatMessage> SendMessage(string username, string message)
        {
            var msg = this.State.AddMessage(username, message);
            await WriteStateAsync();

            return msg;
        }

        /// <summary>
        /// This method is called at the end of the process of activating a grain.
        /// It is called before any messages have been dispatched to the grain.
        /// For grains with declared persistent state, this method is called after the State property has been populated.
        /// </summary>
        public override async Task OnActivateAsync(CancellationToken cancellationToken)
        {
            if (string.IsNullOrEmpty(this.State.Etag))
            {
                this.State = new ChatRoomState(null, null, null);
                await WriteStateAsync();
            }

            await base.OnActivateAsync(cancellationToken);
        }

        #endregion
    }
}
