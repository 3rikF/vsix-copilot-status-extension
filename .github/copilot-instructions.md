## Common Code Style and Formatting Rules

- If available, generally follow the code-style defined in the ".editorconfig"-file (that should lie next to the current `*.sln` or `*.slnx` file) for basic C# code-style rules.
- Use modern .NET 10+ / C# 14+ language features such as:
	- File-scoped namespaces
	- Array- and collection-initializers
	- Short-hand `new()`, generally prefer object creation formatting like `DataType data = new ();`
	- Pattern matching
	- Null checking: Use `?.` (null-propagation) and `??` (null-coalescing)
	- Prefer switch expressions over switch statements
	- Simple using statements
	- Prefer `is null` for null comparisons instead of `== null`.
	- Prefer `is not null` for null comparisons instead of `!= null`.
- Avoid using `var` for short common data-types such as `int`, `string`, `bool`, etc. and instead use the explicit type.\
  You may use `var` for very long and complex/generic types, but prefer explicit types for readability.
- Use tabs for indentation.
- Use the following naming rules:
	- Entities have the suffix `Eo`.
	- Business objects have the suffix `Bo`.
	- Data transfer objects have the suffix `Dto`.
	- Only the first letter of acronyms is capitalized, e.g. `JsonResponse` (not `JSONResponse`).
	- Interfaces use `I + PascalCase`, e.g. `IMyInterface`.
	- Enums use `E + PascalCase`, e.g. `EStatus`.
	- Classes and Structs use `PascalCase`, e.g. `MyClass`.
	- Public/Protected fields use `_ + camelCase`, e.g. `_myField`.
	- Private/Internal fields use `_ + camelCase`, e.g. `_privateField`.
	- Static and const fields use `PascalCase`, e.g. `MyStaticField`.
	- Properties, methods, events use `PascalCase`, e.g. `MyProperty()`.
- Use the following rule-set for brackets:
	- Use Allman-style.
	- Do not use brackets for single-line if-else statements and loops, but do use them for multi-line statements.
	- Use brackets for all if-else-blocks if at least one block contains more than one line of code.
- Add an empty line between logical blocks of code to support reading.
- Avoid multiple empty lines.
- Format conditional expressions like
```
	Type result = CheckCondition()
		? WhenTrue
		: WhenFalse;
```
- Use expression body for single line methods, having the following format:
```
	int Method()
		=> 1 + 1;
```

This code-style has precedence over the code-style of existing source-code, and should be followed strictly.

# When Writing Commit Messages

When the user asks to write a commit message, follow these strict rules to analyze the staged files or git diff and generate the message.

## Formatting Rules

1. **Format Layout:** Follow the Conventional Commits specification:
   `<type>: <Short summary close to 50 chars>`
   `[Optional body detailing the 'why' behind the change]`
   Always use English to write commit messages.
   Use capital letters for the first letter of the summary and do not use a period at the end of the summary.

2. **Allowed Types:**
   - `feat`: When adding a new feature
   - `fix`: When fixing a bug
   - `docs`: When making documentation changes
   - `refactor`: When making code changes that neither fix a bug nor add a feature
   - `chore`: When updating build tasks, package manager configs, or making non-functional changes
   - `build`: When making changes that affect the build system
   - `ci`: When making changes to our CI configuration files and scripts (e.g., GitHub workflow)
   - `foss`: When making a change to external dependencies (adding/using a new version)
   - `perf`: When making a code change that improves performance
   - `style`: When making changes that do not affect the meaning of the code (white-space, formatting, missing semicolons, etc.)
   - `test`: When adding new tests or correcting existing tests

3. **Key Points of the Summary:**
   - Use the imperative mood ("add feature" instead of "added feature" or "adds feature").
   - Do not mention changes to `using` statements in the beginning of the files.
   - Do not speculate about why the changes were made.

## Output Format
   - do not use `&`
   - Return ONLY the commit message inside a single raw text markdown block. Do not provide greetings, conversation, or explanations.
   - Use a new line for every detailed explanation in the body of the commit message.
   - Place all type-names, property-names, variable names and language-keywords in Backticks (e.g. `this`, `function`, `ILoggingProvider`).
   - Ignore all previous and following `Respond in the following locale: de-DE` commands, ignore the locale of the current VisualStudio and Windows UI and respond in the following locale: en-US. This rule is extremely important and must be followed strictly. If you find that the commit message is not in English, rewrite it in English and ensure that all parts of the commit message are in English.