# QA Regression Report

**Report Title**: Time-Bonus Multiplier Regression Test  
**Feature Under Test**: Time-Bonus Multiplier  
**Date**: 2026-08-29  
**Tester**: [Placeholder]  
**Build Version**: 1.0.1  

## Summary
Re-ran baseline scoring and game-flow test cases to confirm the addition of the time-bonus multiplier did not alter existing behavior for slow-speed words or break any game-loop functionality.

## Regression Test Results Table

| Test ID | Description | Pre-Feature Result | Post-Feature Result | Status |
| :--- | :--- | :--- | :--- | :--- |
| TC-011 | 3-letter word scores 100 base points | Passed (100 pts) | Passed (100 pts after 10s) | Pass |
| TC-012 | 4-letter word scores 400 base points | Passed (400 pts) | Passed (400 pts after 10s) | Pass |
| TC-023 | 7+ letter word scores 2000 base points | Passed (2000 pts) | Passed (2000 pts after 10s) | Pass |
| TC-001 | App launches without crash | Passed | Passed | Pass |
| TC-002 | Grid renders with letters visible | Passed | Passed | Pass |
| TC-003 | At least one valid word can be found and scored | Passed | Passed | Pass |
| TC-016 | New score inserted when leaderboard < 10 entries | Passed | Passed | Pass |
| TC-017 | Higher score replaces lowest when full | Passed | Passed | Pass |

## New Feature Verification

| Test ID | Description | Expected Result | Actual Result | Status |
| :--- | :--- | :--- | :--- | :--- |
| TC-013 | Fast word (< 5s) applies 1.5x multiplier | Score = Base * 1.5 | Score = Base * 1.5 | Pass |
| TC-014 | Medium speed word (5-10s) applies 1.25x multiplier | Score = Base * 1.25 | Score = Base * 1.25 | Pass |
| TC-015 | Slow word (> 10s) applies 1.0x multiplier | Score = Base * 1.0 | Score = Base * 1.0 | Pass |

## Conclusion
All 11 regression test cases passed. The time-bonus multiplier feature was integrated without regressions to base scoring, leaderboard operations, or core game-loop functionality.
