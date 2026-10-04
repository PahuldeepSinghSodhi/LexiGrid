# QA Test Plan: LexiGrid

**Project Name**: LexiGrid  
**Version**: 1.0.0  
**Date**: 2026-08-29  
**Tester Name**: [Placeholder]  
**Environment**: Unity Editor, Windows  

## Scope
**In Scope**: Word validation, scoring, leaderboard, grid generation, persistence, UI feedback.  
**Out of Scope**: Multiplayer, cloud sync, localization, accessibility, audio.  

## Test Cases

| Test ID | Category | Description | Preconditions | Steps | Expected Result | Actual Result | Status |
| :--- | :--- | :--- | :--- | :--- | :--- | :--- | :--- |
| TC-001 | Smoke Test | App launches without crash | Unity Editor running | 1. Press Play in Editor | Game starts, main menu/grid appears, no errors in console | | Not Run |
| TC-002 | Smoke Test | Grid renders with letters visible | Game launched | 1. Observe the 4x4 grid | 16 letters are visible and rendered properly | | Not Run |
| TC-003 | Smoke Test | At least one valid word can be found and scored | Grid rendered | 1. Swipe a known valid word | Word is accepted and score increases | | Not Run |
| TC-004 | Functional - Word Validation | Valid 3-letter word accepted and scored | Grid loaded, word exists | 1. Swipe a 3-letter dictionary word | Word turns green, score is awarded | | Not Run |
| TC-005 | Functional - Word Validation | Valid 5-letter word accepted and scored correctly | Grid loaded, word exists | 1. Swipe a 5-letter dictionary word | Word turns green, score is awarded | | Not Run |
| TC-006 | Functional - Word Validation | Invalid word (not in dictionary) rejected, no score awarded | Grid loaded | 1. Swipe a non-word | Word turns red, score does not increase | | Not Run |
| TC-007 | Functional - Word Validation | Already-found word rejected on reuse attempt | Valid word already found | 1. Swipe the same word again | Word is rejected, no duplicate score awarded | | Not Run |
| TC-008 | Functional - Word Validation | Real-time prefix highlighting: valid prefix shows green | Grid loaded | 1. Swipe first 2 letters of a valid word | Line/Tiles highlight in green | | Not Run |
| TC-009 | Functional - Word Validation | Real-time prefix highlighting: invalid prefix shows red | Grid loaded | 1. Swipe 2 letters that form no valid prefix | Line/Tiles highlight in red | | Not Run |
| TC-010 | Functional - Word Validation | Non-adjacent tile swipe does not register | Grid loaded | 1. Select tile A, then select non-adjacent tile B | Selection of B is ignored, swipe chain remains at A | | Not Run |
| TC-011 | Functional - Scoring | 3-letter word scores 100 base points | Grid loaded | 1. Swipe valid 3-letter word | Score increases by 100 points | | Not Run |
| TC-012 | Functional - Scoring | 4-letter word scores 400 base points | Grid loaded | 1. Swipe valid 4-letter word | Score increases by 400 points | | Not Run |
| TC-013 | Functional - Scoring | Fast word (under 5s) applies 1.5x multiplier | Grid loaded | 1. Find word in < 5 seconds | Score = Base * 1.5 | | Not Run |
| TC-014 | Functional - Scoring | Medium speed word (5-10s) applies 1.25x multiplier | Grid loaded | 1. Find word between 5 and 10 seconds | Score = Base * 1.25 | | Not Run |
| TC-015 | Functional - Scoring | Slow word (over 10s) applies 1.0x multiplier | Grid loaded | 1. Find word after 10 seconds | Score = Base * 1.0 | | Not Run |
| TC-016 | Functional - Leaderboard | New score inserted when leaderboard has fewer than 10 entries | Leaderboard has <10 entries | 1. Finish game with >0 score | Score is added to leaderboard | | Not Run |
| TC-017 | Functional - Leaderboard | Higher score replaces lowest when leaderboard is full | Leaderboard has 10 entries | 1. Finish game with score > lowest entry | Lowest entry is removed, new score inserted | | Not Run |
| TC-018 | Functional - Leaderboard | Lower score does not replace any entry when leaderboard is full | Leaderboard has 10 entries | 1. Finish game with score < lowest entry | Leaderboard remains unchanged | | Not Run |
| TC-019 | Functional - Leaderboard | Leaderboard persists after app restart | Leaderboard populated | 1. Restart game, 2. Open Leaderboard | Previous scores remain visible | | Not Run |
| TC-020 | Boundary/Edge Cases | 2-letter word rejected (below minimum length) | Grid loaded | 1. Swipe a 2-letter valid English word | Word is rejected (min length is 3) | | Not Run |
| TC-021 | Boundary/Edge Cases | Timer reaches zero mid-swipe — game ends, no score for partial word | Game running, Timer < 2s | 1. Begin swipe, 2. Timer hits 0 | Game ends immediately, word not scored | | Not Run |
| TC-022 | Boundary/Edge Cases | Grid with no possible valid words triggers regeneration | Grid generation triggered | 1. Force grid with 0 words (test hook) | Grid automatically regenerates until solvable | | Not Run |
| TC-023 | Boundary/Edge Cases | 7+ letter word scores 2000 base points | Grid loaded, 7+ letter word exists | 1. Swipe 7 (or more) letter word | Score increases by 2000 points | | Not Run |
| TC-024 | Regression | After adding time-bonus, base scoring for slow words still returns correct base score | Time-bonus enabled | 1. Find 3-letter word after 11s | Score increases by exactly 100 points | | Not Run |
| TC-025 | Regression | After adding leaderboard persistence, game round flow still works correctly | Persistence enabled | 1. Play round to completion, 2. Start new round | Game transitions correctly without freezing | | Not Run |
