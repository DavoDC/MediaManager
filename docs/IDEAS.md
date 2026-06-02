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

**Current coverage:** MediaFile regex patterns and GetGroupValue (comprehensive as of 2026-06-02, all tests green).

**Next expansion candidates (in value order):**
1. **StatList** - likely same as AudioManager's StatList (GetSortedFreqDist, GetDecadeFreqDist). Direct port.
2. **LibChecker** - same injection pattern as AudioManager. Start with rules that have no external deps.
3. **Expansion rule:** Add a test when a real bug escapes current coverage. Not before.
