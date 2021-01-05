namespace EF6.TagWith
{
    /// <summary>
    /// Defines how the tag should be added to the SQL query
    /// </summary>
    public enum TagMode
    {
        /// <summary>
        /// Default value. The tag will be inserted BEFORE the SELECT command. Example: <br/>
        /// -- Your tag here<br />
        /// SELECT ...
        /// </summary>
        Prefix = 0,
        /// <summary>
        /// The tag will be inserted just AFTER the SELECT command. Use this mode when you are using the
        /// SQL Server Query Store or other tracing tool that eliminates comments outside the query text.
        /// Example:<br/>
        /// SELECT /* Your tag here */<br/>
        /// ...
        /// </summary>
        Infix = 1
    }
}