# Code checks (card 368)

Owner: Akilesh Sookhareea

These checks are run from the repo root in PowerShell. They look at every .cs file in src (bin and obj folders are skipped). An empty result means the check passed. Run them again in the last week before the deadline and write the date below.

## 1. No built-in collections

```
Get-ChildItem src -Recurse -Filter *.cs | Where-Object { $_.FullName -notmatch '\\(bin|obj)\\' } | Select-String -CaseSensitive -Pattern "(?<![A-Za-z])(List|Dictionary|HashSet|Queue|Stack|LinkedList|SortedList)<|ArrayList|System\.Collections"
```

## 2. No operating system calls

```
Get-ChildItem src -Recurse -Filter *.cs | Where-Object { $_.FullName -notmatch '\\(bin|obj)\\' } | Select-String -Pattern "Process\.Start|System\.Diagnostics|Environment\.Exit|DllImport|cmd\.exe|Registry"
```

## 3. Packages used

```
Select-String -Path src\CinemaBooking\CinemaBooking.csproj -Pattern "PackageReference"
```

Only Microsoft.Data.SqlClient (the SQL client package from card 273) is expected.

## Results

| Date | Check 1 | Check 2 | Check 3 | Run by |
|---|---|---|---|---|
| 17 Nov 2026 | 0 hits | 0 hits | Microsoft.Data.SqlClient 7.1.2 only | Akilesh |
