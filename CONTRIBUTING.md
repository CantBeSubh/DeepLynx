
# How to Contribute

## Coding Guidelines
* Follow the .NET Core coding conventions.
* Write clear and concise commit messages.
* Ensure your code is well-documented and includes necessary comments.
* Maintain consistency with the existing codebase.
### Testing
* Write unit tests for any new functionality within the Testing folder.
* Ensure all existing and new tests pass before submitting your pull request.
* Run the tests using the following command from the root folder:

```
dotnet test
```

## Submitting Pull Requests
When you're ready to submit your changes, follow these steps:

1. Create a new branch associated with a jira ticket: `git checkout -b DL-100`
    - If there are multiple jira tickets you're covering, do your best to split them into separate PRs to make them digestable.
    - If you must tackle multiple tickets in a single PR, just choose one ticket number for your branch name.

2. Make your changes: Ensure your changes include appropriate documentation and tests.

3. Build the app: Ensure the app builds properly with your changes.
    - Using Rider, this can be performed with the play button in the top right: ![alt text](markdown-assets/buildApp.png)

4. Create a pull request: Provide a clear description of your changes and any related issues.

### Communication
If you need any help or have questions, feel free to reach out. 

# The Meat and Potatoes

Below is the home for several technical "gotchas" that may be useful for other project developers. If you learn something new as you develop, feel free to add a section here.

## Making Database Changes

Making changes to the data layer and underlying database can feel quite involved, but when done right it will facilitate easy transfer of changes from developer to developer. Here are some tips:

### Adding a column

In the table that you are adjusting, you can add a column like so:

```
[Column("new_column")]
public dataType NewColumn { get; set; }
```

If the column is nullable, be sure to add a ? after the dataType. So for example, `long` becomes `long?`

### Adding an index

Indexes can be added at the top of a table file like so:

```
[Index("ColumnName", Name = "idx_table_name_column_name")]
[Index("OtherColumn", Name = "idx_table_name_other_column")]
public partial class TableName
```

### Adding a foreign key

Foreign keys need to be referenced in both the table where the key resides, and the table being referenced. Changes to DeepLynx context will also need to be made. For example, if I was making a foreign key to represent a one-to-many relationship between Books and Authors:

Book.cs:
```
[Table("books", Schema = "deeplynx")]
[Index("AuthorId", Name = "idx_books_author_id")]
public partial class Book
{
    [Key]
    [Column("id")]
    public long Id { get; set; }

    [Column("author_id")]
    public long AuthorId { get; set; }

    [ForeignKey("AuthorId")]
    [InverseProperty("Books")]
    public virtual Author Author { get; set; } = null!;
}
```

Author.cs:
```
[Table("authors", Schema = "deeplynx")]
public partial class Author
{
    [Key]
    [Column("id")]
    public long Id { get; set; }

    [InverseProperty("Author")]
    public virtual ICollection<Book> Books { get; set; } = new List<Book>();
}
```

DeeplynxContext.cs:
```
public partial class DeeplynxContext : DbContext
{
    public DeeplynxContext()
    {
    }

    public DeeplynxContext(DbContextOptions<DeeplynxContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Author> Authors { get; set; }

    public virtual DbSet<Book> Books { get; set; }

    modelBuilder.Entity<Author>(entity =>
    {
        entity.HasKey(e => e.Id).HasName("authors_pkey");
    });

    modelBuilder.Entity<Book>(entity =>
    {
        entity.HasKey(e => e.Id).HasName("books_pkey");

        entity.HasOne(b => b.Author).WithMany(a => a.Books).HasConstraintName("books_author_id_fkey");
    });

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
```

### Making a migration

To create a migration reflecting your changes, run the following command (replacing <MIGRATION_NAME>):

```
dotnet ef migrations add <MIGRATION_NAME> -c DeeplynxContext --verbose --project deeplynx.datalayer --startup-project deeplynx.api
```

### Updating the database

This can be done after you've made your migration to verify that the changes are accurately reflected in the DB. Use this command:

```
dotnet ef database update -c DeeplynxContext --verbose --project deeplynx.datalayer --startup-project deeplynx.api
```