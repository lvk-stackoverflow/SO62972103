This is a prototype implementation of a "moved lines only" git difftool
as an example for a question on Stack Overflow:

[Verify that Git commit only moves lines](https://stackoverflow.com/questions/62972103/verify-that-git-commit-only-moves-lines)

To use it, first clone the repository and build it with .NET Core 3.1, then
edit your global .gitconfig file to set it up as a new git difftool.

Here's an example for Windows:

    [difftool "OnlyMovedLines"]
        cmd = 'D:\\path\\to\\netcoreapp3.1\\OnlyLinesMovedDiffTool.exe' $LOCAL $REMOTE

Then to use it:

    git difftool -t OnlyMovedLines HEAD~1 HEAD
    
 This will use the new difftool on a per-file basis and report whether there were only
 moved lines, or if other types of operations also took place.
 
 Sample output when just moved lines were present:
 
     ❯ git difftool -t OnlyMovedLines HEAD~1 HEAD
     Line 3 moved to 8
        3
     Line 9 moved to 3
        9
        
Sample output when other operations were also present:

    ❯ git difftool -t OnlyMovedLines HEAD~2 HEAD
    Diff consisted of other operations than just moves:
      1 line was deleted
      1 line was inserted
      1 line was modified in-place