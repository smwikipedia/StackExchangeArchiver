# StackExchangeArchiver
A tool to archive Stack Exchange sites posts for offline browsing.

If used unchanged, only `stackoverflow.com` is supported. But you can easily change the source code to support other site like `math.stackexchange.com`. Just change the URL pattern.

This tool archive questions or answers for a specific user. All you need to know is the user ID and the user name.

Usage:

`StackOverflowArchiver <UserID> <UserName> <q/a> <ArchiveFolder> [StartPage] [EndPage]`

  `<UserID>`: Required. The Stack Exchange site user id. It can be identified from the URL.

  `<UserName>`: Requried. The Stack Exchange site user name. It can be identified from the URL.

  `<q/a>`: Requried. `q` for archiving questions, `a` for answers.

  `<ArchiveFolder>`: Requried. Path to a local folder to store the archived posts.

  `<StartPage/EndPage>`: Optional. The start and end page number of the question/answer list in the Stack Exchange user activity page. If specified, only the posts within the page range will be archived. The post pages are sorted by date.
