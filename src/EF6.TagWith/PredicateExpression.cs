namespace EF6.TagWith
{
    /// <summary>
    /// Defines the type of predicate that will be used to identify that the query must be tagged.
    /// </summary>
    public enum PredicateExpression
    {
        /// <summary>
        /// The predicate included in the query will use the equals operator:
        /// Example: SELECT ... (N'!__tag!' = N'Your comment') ...
        /// This is the default value.
        /// </summary>
        Equals = 0,
        /// <summary>
        /// The predicate included in the query will use the not equals operator:
        /// Example: SELECT ... (N'!__tag!' <> N'Your comment') ...
        /// This is the recommended options because it will help if you are interested
        /// in getting the generated SQL (for example, using ToString()) and executing it directly in the database.
        /// </summary>
        NotEquals = 1
    }
}