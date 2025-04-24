
# How to Contribute

## Coding Guidelines
* Follow the .NET Core coding conventions.
* Write clear and concise commit messages.
* Ensure your code is well-documented and includes necessary comments.
* Maintain consistency with the existing codebase.
### Testing
* Write unit tests for any new functionality.
* Ensure all existing and new tests pass before submitting your pull request.
* Run the tests using the following command:
```
dotnet test
```

## Submitting Pull Requests
When you're ready to submit your changes, follow these steps:

1. Create a new branch associated with a jira ticket:

```
git checkout -b DL-100
```
2. Make your changes: Ensure your changes include appropriate documentation and tests.

** Do not check in manually altered deeplynx.datalayer files. Pull requests that contain manual datalayer changes will be rejected unless granted prior permission. Database changes are done via migrations, see README. **

3. Create a pull request: Provide a clear description of your changes and any related issues.

### Communication
If you need any help or have questions, feel free to reach out. 