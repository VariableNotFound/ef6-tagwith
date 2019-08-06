# EF6-TagWith

Tag your Entity Framework 6 LINQ queries, and see these tags in the SQL commands sent to the database. This will allow you to easily 
identify queries while using tools like SQL Profiler or Azure's performance and diagnostic features.

(Yes, it is a simple implementation of [Entity Framework Core's query tags feature](https://docs.microsoft.com/en-us/ef/core/querying/tags).)

## Usage (SQL Server)

1. Install the [NuGet package](https://www.nuget.org/packages/EF6.TagWith/) in your project.

```
PM> install-package EF6.TagWith
```

2. Add the query interceptor. The easiest way is just to add the following code
   somewhere in your application startup code:

```cs
DbInterception.Add(new QueryTaggerInterceptor(new SqlServerTagger()));
```

3. Tag your queries this way:

```cs
var query = context.Friends
    .OrderByDescending(friend => friend.Age)
    .Take(10)
    .Select(friend => new { FriendName = friend.Name, friend.Age, CountryName = friend.Country.Name })
    .TagWith("Get top 10 older friends with country");
```
4. The query sent to the database will be as follows:
```sql
-- Get top 10 older friends with country

SELECT TOP(@__p_0) [friend].[Name] AS [FriendName], [friend].[Age], 
                   [friend.Country].[Name] AS [CountryName]
FROM [Friends] AS [friend]
LEFT JOIN [Countries] AS [friend.Country] ON [friend].[CountryId] = [friend.Country].[Id]
ORDER BY [friend].[Age] DESC
```

## How this works

The `TagWith()` extension method adds a special predicate to the query, so it can be easily identified in the final SQL. 

Later on, just before sending the SQL command to the database, we use EF 6 interceptors to extract this predicate and insert the tag as a comment, just using a bit of string wizardry.

## Known issues

* The component only supports SQL Server, but can be easily adapted to support another providers just creating a new implementation of `ISqlTagger` and using it in the interceptor configuration.

## Contributions

Feel free to send issues, comments or pull requests. The following users have contributed to this project so far:

* [José M. Aguilar](https://github.com/jmaguilar)
* [Henry Malthus](https://github.com/henrym)

Thank you!
