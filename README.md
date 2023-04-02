# EF6-TagWith

Tag your Entity Framework 6 LINQ queries, and see these tags in the SQL commands sent to the database. This will allow you to easily 
identify queries while using tools like SQL Profiler or Azure's performance and diagnostic features.

(Yes, it is a simple implementation of [Entity Framework Core's query tags feature](https://docs.microsoft.com/en-us/ef/core/querying/tags).)

## Usage (SQL Server)

1. Install the [NuGet package](https://www.nuget.org/packages/EF6.TagWith/) in your project.

    ```
    PM> install-package EF6.TagWith
    ```

2. Add the query interceptor. The easiest way is just to add the following code somewhere in your application startup code:

    ```cs
    DbInterception.Add(new QueryTaggerInterceptor(new SqlServerTagger()));
    ```

    Starting at version 1.2, you may also use the `TagWith.Initialize()` helper method as follows:

    ```cs
    TagWith.Initialize<SqlServerTagger>();
    ```

    This method is the recommended option because it allows you to specify some tagging options, as explained below.

3. Use the `TagWith()` extension to tag your queries:

    ```cs
    var query = context.Friends
        .OrderByDescending(friend => friend.Age)
        .Take(10)
        .Select(friend => new { 
            FriendName = friend.Name, 
            friend.Age, 
            CountryName = friend.Country.Name }
        )
        .TagWith("Get top 10 older friends with country");
    ```

4. By default, the query sent to the database will be as follows:

    ```sql
    -- Get top 10 older friends with country

    SELECT TOP(@__p_0) [friend].[Name] AS [FriendName], [friend].[Age], 
                       [friend.Country].[Name] AS [CountryName]
    FROM [Friends] AS [friend]
    LEFT JOIN [Countries] AS [friend.Country] 
         ON [friend].[CountryId] = [friend.Country].[Id]
    ORDER BY [friend].[Age] DESC
    ```

## How this works

The `TagWith()` extension method adds a special predicate to the query, so it can be easily identified in the final SQL. 

Later on, just before sending the SQL command to the database, we use EF 6 interceptors to extract this predicate and insert the tag as a comment, just using a bit of string wizardry.


## Prefix vs Infix

By default, TagWith inserts the tags _before the SQL command_. However, there are some query tracing tools, such as SQL Query Store or Azure SQL Performance Monitor, that remove initial comments so the tags are not shown. For these cases, we can force the tags (comments) to be inserted after the `SELECT` command.

For example, the above SQL command would look like this:

```sql
SELECT -- Get top 10 older friends with country

TOP(@__p_0) [friend].[Name] AS [FriendName], [friend].[Age], 
                    [friend.Country].[Name] AS [CountryName]
FROM [Friends] AS [friend]
LEFT JOIN [Countries] AS [friend.Country] 
        ON [friend].[CountryId] = [friend.Country].[Id]
ORDER BY [friend].[Age] DESC
```

The "infix" mode must be specified during the component initialization this way:

```cs
TagWith.Initialize<SqlServerTagger>(
    new TaggingOptions() { 
        TagMode = TagMode.Infix 
    });
```

## Predicate expression

By default, the predicate included in the query specification when you use the method `TagWith()` looks like this:

```cs
// LINQ query:
var query = _ctx.Friends.TagWith("My friends").Where(f => f.Id < 10);
Console.WriteLine(query.ToString());

// Output:
SELECT 
    [Extent1].[Id] AS [Id], 
    [Extent1].[Name] AS [Name], 
    [Extent1].[Country_Id] AS [Country_Id]
    FROM [dbo].[Friends] AS [Extent1]
    WHERE (N'!__tag!' = N'My friends') AND ([Extent1].[Id] < 10)
```

Later on, just before the query is sent to the database, the predicate will be removed and the tag will be added as a comment.

As suggested by [d-a-s](https://github.com/d-a-s), the problem with this approach is that if you try to get the SQL using `ToString()`, as seen above, and try to execute it against the database, you'll never get any results because the predicate is always evaluated as `false`.

Starting at version 1.3.0, you can change this behaviour by specifying a different predicate expression during the component initialization.

```cs
TagWith.Initialize<SqlServerTagger>(
    new TaggingOptions() { 
        PredicateExpression = PredicateExpression.NotEquals
    });
```

In this case, the query specification will be as follows, so you could run it against the database because the inserted predicate will be always `true` (note the distinct '<>' operator usage):

```cs
// LINQ query:
var query = _ctx.Friends.TagWith("My friends").Where(f => f.Id < 10);
Console.WriteLine(query.ToString());

// Output:
SELECT 
    [Extent1].[Id] AS [Id], 
    [Extent1].[Name] AS [Name], 
    [Extent1].[Country_Id] AS [Country_Id]
    FROM [dbo].[Friends] AS [Extent1]
    WHERE (N'!__tag!' <> N'My friends') AND ([Extent1].[Id] < 10)
```


## Another tagging options

Apart from using `TagWith()` in the query, you may also use the extension method `TagWithSource()`, that will include automatically the caller member name and source code file, like in the following example:

```cs
// Initialization:
TagWith.Initialize<SqlServerTagger>();

// Query:
private List<Friend> GetFriends()
{
    var friends = ctx.Friends.OrderBy(f => f.Name).Take(10).TagWithSource("Getting friends").ToList();
    return friends;
}
```

Then, the SQL query sent to the database will be as follows:

```sql
-- Getting friends - GetFriends - C:\repos\TagWithDemo\FriendRepository.cs:20
SELECT 
    [Limit1].[Id] AS [Id], 
    [Limit1].[Name] AS [Name], 
    [Limit1].[Country_Id] AS [Country_Id]
    FROM ( SELECT TOP (10) [Extent1].[Id] AS [Id], [Extent1].[Name] AS [Name],
           [Extent1].[Country_Id] AS [Country_Id]
           FROM [dbo].[Friends] AS [Extent1]
           ORDER BY [Extent1].[Name] ASC
    )  AS [Limit1]
    
    ORDER BY [Limit1].[Name] ASC
```

## .NET versions supported

EF6.TagWith should work with any .NET Framework version higher than 4.0, as well as with any version of .NET Core thanks to .NET Standard. So in theory you can use it wherever you are using Entity Framework 6.x.

If you find issues, please [let me know](https://github.com/VariableNotFound/ef6-tagwith/issues), detailing what .NET version are you using and what kind of problems are you experiencing.

## Known issues

* The component only supports SQL Server, but can be easily adapted to support another providers just creating a new implementation of `ISqlTagger` and using it in the interceptor configuration.
* If you are mocking a `DbSet` that is queried using `TagWith()` you may find problems. Please read [this issue](https://github.com/VariableNotFound/ef6-tagwith/issues/4) from [@jsgoupil](https://github.com/jsgoupil) to see how to solve it.

## Contributions

Feel free to send issues, comments or pull requests. The following users have contributed to this project so far:

* [José M. Aguilar](https://github.com/jmaguilar)
* [Henry Malthus](https://github.com/henrym)
* [Željko Brkić](https://github.com/zbrkic)

Thank you!
