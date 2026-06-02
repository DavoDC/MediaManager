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

## Automated Tests - Gap

**Zero test coverage.** No test infrastructure exists. Priority order when tackling:

1. **Set up test infrastructure** - copy Assert.cs + TestRunner.cs pattern from AudioManager. Wire into a `--test` CLI flag in Prog.cs. Add test.bat script.
2. **MediaFile parsing** - `folderRegex` and `conciseQualityTitleRegex` are pure-function regex patterns in MediaFile.cs. These are the highest-value first tests: known-good folder names, edge cases (no year, no ID, anime vs movie vs show), quality title variations.
3. **StatList** - likely same as AudioManager's StatList (GetSortedFreqDist, GetDecadeFreqDist). Direct port once infrastructure is in place.
4. **LibChecker** - same pattern as AudioManager. Start with rules that have no external deps.

**Prerequisite:** Survey the architecture first. MediaManager shares AudioManager's structure but targets video/photos/documents. Confirm which modules are direct ports vs custom before writing tests.
