# MediaManager - History

Completed features, settled design decisions, resolved tasks. Active work -> `docs/IDEAS.md`.

---

## 2026-06-02 - Thin bat: vswhere MSBuild discovery in test.bat

`PROJECT/test.bat` had a hardcoded `C:\Program Files\Microsoft Visual Studio\18\Community\MSBuild\Current\Bin\MSBuild.exe` path. Replaced with dynamic vswhere discovery (same pattern as AudioManager's `scripts/dev/build.bat`): finds MSBuild from any VS 2017+ installation regardless of edition or install path.

## 2026-06-02 - Test infrastructure bootstrapped

Assert.cs + TestRunner.cs + `--test` flag + test.bat. DIY framework matching AudioManager (no xUnit, no separate project, old-style csproj). Initial coverage: MediaFile, StatList, LibChecker, Reflector, AgeChecker, EpisodeFile, MovieFile - all tests green.
