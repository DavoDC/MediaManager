# MediaManager - Ideas and TODOs

## Overview

MediaManager extends AudioManager patterns to other media types (video, photos, documents).

## Current Status

Work on this after:
- AudioManager (P2) - complete CLI integration pipeline
- Shared Modules (P6) - extract common code first

## Approach

1. Survey current state: what does it do, what's broken, check TTD.md for open issues
2. Apply same CLI API design pattern as AudioManager if it fits
3. Evaluate any AudioManager improvements (module separation, batch launchers, auto-compile) for applicability here

## Next Steps

- Understand current architecture (read MediaFile.cs, Analyser.cs, LibChecker.cs hierarchy)
- Identify shared patterns with AudioManager (same DIY test framework, same injection refactor pattern)

## Automated Tests

**Infrastructure in place as of 2026-06-02.** Assert.cs + TestRunner.cs + `--test` flag + test.bat.

**Current coverage (comprehensive as of 2026-06-02, all tests green):**
- MediaFile: folderRegex + conciseQualityTitleRegex patterns, GetGroupValue (18 tests)
- StatList: GetSortedFreqDist + GetDecadeFreqDist (9 tests)
- LibChecker: RemovePrefix, RemoveBracedPrefix, RemoveBracketedPrefix, chained removal (12 tests)
- Reflector: SanitiseFilename, FixLongPath, GetRelativePath (11 tests)
- AgeChecker: all 5 branches via path injection (5 tests)

**Next expansion candidates (in value order):**
1. **EpisodeFile/AnimeFile/ShowFile regex** - showEpRegex and anime-specific patterns. Test by replicating patterns inline (same approach as MediaFile folder/quality regex tests).
2. **MovieFile regex** - similar Radarr filename pattern.
3. **Expansion rule:** Add a test when a real bug escapes current coverage. Not before.
