# EF6-TagWith

Tag your to Entity Framework 6 LINQ queries, and see these tags in the SQL commands sent to the database. This will allow you to easily 
identify queries while using tools like SQL Profiler or Azure's performance and diagnostic features.

(Yes, it is a simple implementation of [Entity Framework Core's query tags feature](https://docs.microsoft.com/en-us/ef/core/querying/tags).)

## Usage

1. Install the NuGet package in your project.

```
PM> install-package EF6-TagWith
```

2. Tag your queries this way:

```cs
var query = context.Friends
    .OrderByDescending(friend => friend.Age)
    .Take(10)
    .Select(friend => new { FriendName = friend.Name, friend.Age, CountryName = friend.Country.Name })
    .TagWith("Get top 10 older friends with country");
```
3. The query sent to the database will be as follows:
```sql
-- Get top 10 older friends with country

SELECT TOP(@__p_0) [friend].[Name] AS [FriendName], [friend].[Age], 
                   [friend.Country].[Name] AS [CountryName]
FROM [Friends] AS [friend]
LEFT JOIN [Countries] AS [friend.Country] ON [friend].[CountryId] = [friend.Country].[Id]
ORDER BY [friend].[Age] DESC
```

## How this works

The `TagWith()` extension method adds a special predicate to the query, so it can be easily identified in the final SQL. Later on, just before sending the SQL command to the database, we use EF 6 interceptors to extract this predicate and insert the tag as a comment, just using a bit of string wizardry.




