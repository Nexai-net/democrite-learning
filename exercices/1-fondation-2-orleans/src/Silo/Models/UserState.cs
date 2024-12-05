﻿namespace Silo.Models
{
    [GenerateSerializer]
    public sealed class UserState
    {
        #region Fields

        private readonly List<string> _rooms;

        #endregion

        #region Ctor

        /// <summary>
        /// Initializes a new instance of the <see cref="UserState"/> class.
        /// </summary>
        public UserState(IReadOnlyCollection<string>? rooms, string username, bool isLogged)
        {
            this._rooms = rooms?.ToList() ?? new List<string>();
            this.Rooms = this._rooms;

            this.UserName = username;
            this.IsLogged = isLogged;
        }

        #endregion

        #region Properties

        [Id(0)]
        public IReadOnlyCollection<string> Rooms { get; }

        [Id(1)]
        public string UserName { get; }

        [Id(2)]
        public bool IsLogged { get; set; }

        #endregion

        #region Methods

        /// <summary>
        /// Joins a room.
        /// </summary>
        internal bool JoinRoom(string roomName)
        {
            if (this._rooms.Contains(roomName))
                return false;

            this._rooms.Add(roomName);
            return true;
        }

        /// <summary>
        /// Leave a room.
        /// </summary>
        internal bool LeaveRoom(string roomName)
        {
            return this._rooms.Remove(roomName);
        }

        #endregion
    }
}