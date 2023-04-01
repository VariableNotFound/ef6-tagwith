namespace EF6.TagWith
{
    public class TaggingOptions
    {
        /// <summary>
        /// Determines if the tag will be added as a prefix or as a suffix.
        /// </summary>
        public TagMode TagMode { get; set; } = TagMode.Prefix;

        /// <summary>
        /// Determines the type of predicate that will be included in the query
        /// to mark it as a tagged query.
        /// </summary>
        public PredicateExpression PredicateExpression { get; set; } = PredicateExpression.Equals;

    }
}