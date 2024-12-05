namespace Silo.Implementations
{
    using Silo.Grains;
    using Silo.Models;

    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;

    /// <summary>
    /// Grain implementation of <see cref="IUserGrain"/>
    /// </summary>
    /// <seealso cref="Silo.Grains.IUserGrain" />
    internal sealed class UserGrain : Grain<UserState>, IUserGrain
    {
        #region Fields

        private readonly IGrainFactory _grainFactory;

        #endregion

        #region Ctor

        /// <summary>
        /// Initializes a new instance of the <see cref="UserGrain"/> class.
        /// </summary>
        public UserGrain([PersistentState("Users")] IPersistentState<UserState> storage,
                         IGrainFactory grainFactory)
            : base(storage)
        {
            this._grainFactory = grainFactory;
        }

        #endregion

        #region Methods

        /// <inheritdoc />
        public Task<IReadOnlyCollection<string>> GetChatRoomList()
        {
            return Task.FromResult(this.State.Rooms);
        }

        /// <inheritdoc />  
        public Task<bool> IsLogged()
        {
            return Task.FromResult(this.State.IsLogged);
        }

        /// <inheritdoc />
        public async Task JoinRoom(string roomName)
        {
            this.State.JoinRoom(roomName);

            var room = this._grainFactory.GetGrain<IChatRoomGrain>(roomName);
            await room.Join(this.State.UserName);
        }

        /// <inheritdoc />
        public async Task LeaveRoom(string roomName)
        {
            this.State.LeaveRoom(roomName);

            var room = this._grainFactory.GetGrain<IChatRoomGrain>(roomName);
            await room.Leave(this.State.UserName);
        }

        /// <inheritdoc />
        public async Task<IReadOnlyCollection<string>> LoginAsync()
        {
            if (this.State.IsLogged == false)
            {
                this.State.IsLogged = true;
                await WriteStateAsync();
            }

            return this.State.Rooms;
        }

        /// <inheritdoc />
        public async Task Logout()
        {
            if (this.State.IsLogged == true)
            {
                this.State.IsLogged = false;
                await WriteStateAsync();
            }
        }

        /// <inheritdoc />
        public override async Task OnActivateAsync(CancellationToken cancellationToken)
        {
            if (string.IsNullOrEmpty(this.State.UserName))
            {
                var grainId = this.GetGrainId();
                this.State = new UserState(this.State?.Rooms, grainId.Key!.ToString()!, this.State.IsLogged);
                await WriteStateAsync();
            }

            await base.OnActivateAsync(cancellationToken);
        }

        #endregion
    }
}
