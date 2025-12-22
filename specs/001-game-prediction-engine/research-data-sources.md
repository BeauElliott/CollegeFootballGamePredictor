# Data Source Research: NCAA Football Game Outcome Predictor

## Key Data Types & Recommended Sources

| Data Type              | Recommended Source                              | Update Frequency | API Key Required | Notes |
|------------------------|--------------------------------------------------|------------------|------------------|-------|
| Schedule               | collegefootballdata.com API, NCAA.com           | Weekly           | Yes (CFD)        | CFD is free for non-commercial use |
| Team Statistics        | NCAA official stats, TeamRankings.com, CFD      | Weekly           | No/Yes           | TeamRankings is web scrape only |
| Rosters + DOB          | Sports-Reference, ESPN team pages, Ourlads      | Pre-season + updates | No             | May require scraping |
| Depth Charts / Snaps   | PFF (paid), or approximate via starters         | As available     | Yes (PFF)        | PFF is paid, others are public |

## API Reliability & Licensing
- **collegefootballdata.com**: Most reliable, free for non-commercial, requires API key
- **NCAA.com**: Official, but limited API, may require scraping
- **TeamRankings.com**: No public API, scraping required
- **Sports-Reference/ESPN/Ourlads**: No public API, scraping required
- **PFF**: Paid, commercial license required

## Configuration Recommendations
- Add a `DataSources` section to backend/appsettings.json:
  - Specify endpoints, API keys, refresh intervals, and fallback sources
  - Document licensing and usage restrictions
- Add logic to DataRetrieval project to load and validate config
- Document how to add/change sources in README.md

## Next Steps
- Validate API endpoints and keys for each source
- Document setup and configuration in quickstart.md and README.md
- Add tasks for config setup and research in tasks.md

_Last updated: 2025-12-22_
