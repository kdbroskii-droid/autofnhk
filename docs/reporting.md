# AutoFnhk test reports

AutoFnhk produces structured JSON reports for Fortnoob private playtesting.

## Report flow

1. A test run creates a `TestReport`.
2. Gameplay/test events are recorded as timestamped events.
3. The run finishes with a result and findings.
4. The report is saved as JSON so it can be attached or committed to the `autofnhk` repository.
5. When a report is available in the repository, ChatGPT can inspect it through the GitHub connection and use the findings to make the next code change.

## Important limitation

The GitHub connection does not provide a background channel from the running Windows app directly into ChatGPT. The app therefore does not silently upload reports to ChatGPT. Reports must be made available through GitHub (for example by a later upload/commit workflow), after which ChatGPT can read them during a conversation.

Reports are intended for the user's own Fortnoob/private test environment.
