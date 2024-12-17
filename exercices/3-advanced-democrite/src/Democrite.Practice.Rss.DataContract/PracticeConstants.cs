namespace Democrite.Practice.Rss.DataContract
{
    using Democrite.Framework.Core.Abstractions.Signals;

    public static class PracticeConstants
    {
        #region Ctor

        /// <summary>
        /// Initializes the <see cref="PracticeConstants"/> class.
        /// </summary>
        static PracticeConstants()
        {
            ImportRssSequence = new Guid("D4143CCD-ACDB-431A-A7CC-9A7A60191804");
            RssItemUpdatedSignalId = new SignalId(new Guid("7DC69955-7751-45AA-975D-1D98886BF76C"), "rss-item-updated");
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets the import RSS sequence.
        /// </summary>
        public static Guid ImportRssSequence { get; }

        /// <summary>
        /// Gets the signal id raised when an Rss Item have been created or updated
        /// </summary>
        public static SignalId RssItemUpdatedSignalId { get; }

        #endregion
    }
}
