# StackExchangeArchiver
A tool to archive Stack Exchange sites posts for offline browsing.

This tool archive questions or answers for a specific user. All you need to know is the user ID and the user name.

Usage:

`StackOverflowArchiver <UserID> <UserName> <q/a> <ArchiveFolder> [StartPage] [EndPage]`

  `<UserID>`: The Stack Exchange site user id. It can be identified from the URL. Requried.
  
  `<UserName>`: The Stack Exchange site user name. It can be identified from the URL. Requried.
  
  `<q/a>`: q for archiving questions, a for answers. Requried.
  
  `<ArchiveFolder>`: Path to a local folder to store the archived posts. Requried.
  
  `<StartPage/EndPage>`: The start and end page number of the question/answer list in the Stack Exchange user activity page. If specified, only the posts within the page range will be archived. The post pages are sorted by date. Optional.
