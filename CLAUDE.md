# PatternTrainer - Stock Pattern Analysis Console Application

## Overview
PatternTrainer is a .NET 8 C# console application that analyzes stock market data using machine learning to identify bullish candlestick patterns and technical indicators. It retrieves market data via the Schwab API (through SchwabLib DLL), applies ML models to detect trading setups, and writes results to Google Sheets for review.

## Core Purpose
- Scan a list of tickers for bullish gap patterns (2-bar, 3-bar, 4-bar, 5-bar setups)
- Detect RSI4 long patterns
- Identify DeMark pivot points and trendline breaks
- Calculate volume profiles from intraday data
- Generate PNG chart images for successful patterns
- Persist results to SQL database and Google Sheets

## Application Flow

### Entry Point
**Main.cs** - The application starts at `Program.Main()`, which calls `p.Run().Wait()`

### Main Execution (`Run.cs`)

1. **Initialize**
   - Get Schwab API authentication key
   - Clear charts directory (`c:\work\charts`)
   - Load pre-trained ML models from disk:
     - `TwoBarModel.zip`
     - `ThreeBarModel.zip`
     - `FourBarModel.zip`
     - `FiveBarModel.zip`

2. **Load Ticker List**
   - Instantiate `TickerListWorksheetModel` which reads the "Tickers" sheet from Google Sheets
   - Creates a `TickerListRowDataModel` for each ticker symbol

3. **Process Each Ticker** (main loop)
   
   For each ticker in the list:
   
   a. **Fetch Historical Data**
      - Call `oAPIWrapper.GetCandles()` to retrieve 400 days of daily candles
      - Skip ticker if insufficient data (< 15 candles)
   
   b. **DeMark Pivot Analysis**
      - Call `DemarkPivotModel.FindPivots()` to identify:
        - Latest and next-to-last pivot highs/lows
        - Trendline breaks (if price has crossed the trendline)
        - Forecasted trendline values (if not yet broken)
   
   c. **ML Pattern Detection**
      - `DoTwoBarLive()` - Checks for 2-bar gap + inside bar pattern
      - `DoThreeBarLive()` - Checks for 3-bar gap setup
      - `DoFourBarLive()` - Checks for 4-bar gap setup
      - `DoFiveBarLive()` - Checks for 5-bar gap setup
      - `DoRSI4Live()` - RSI4 indicator-based pattern
   
   d. **Volume Profile Analysis**
      - `ComputeVolumeProfilesLastFiveDays()` fetches 1-minute candles for last 6 days
      - Computes volume profiles for the last 5 completed sessions
      - Returns Point of Control (POC) values for each session
   
   e. **Persistence**
      - `SaveModelsToDatabase()` - Writes pattern results to SQL Server:
        - `SetupInstancesModel` (parent record)
        - Individual pattern models (TwoBarModel, ThreeBarModel, etc.)
        - TrendBreaksModel (for pivot trendline breaks)
      - `oSetupWorkSheetModel.AddTicker()` - Adds to in-memory worksheet model

4. **Generate Charts**
   - `GeneratePNG()` creates chart images for all successful patterns
   - Charts saved to `c:\work\charts\{ticker}.png`
   - Longer patterns overwrite shorter patterns (5-bar overwrites 2-bar)
   - Includes DeMark pivot high trendline visualization

5. **Write Results**
   - Save ticker list to text file (`c:\work\tickersinplay.txt`) for ThinkOrSwim import
   - `oTickerListWorksheetModel.UpdateTickerWorksheet()` - Updates "Tickers" sheet with pivot data
   - `WriteSetupsToWorksheet()` - Writes only triggered setups to "Sheet1" in Google Sheets

## Key Components

### Pattern Detection Example (2-Bar Setup)
**Location:** `GapSetups\2BarSetup\DoTwoBarLive.cs`

Logic:
1. Gets last 3 candles (need day before gap, gap day, and day after)
2. Validates gap didn't fail (subsequent closes above pre-gap close)
3. Checks gap size (logarithmic: >0.68% for green gap day, or >1.2% for red)
4. Verifies day after gap is an inside bar (high/low within gap day range)
5. Returns `MLResult` with success flag and confidence score

### DeMark Pivot Model
**Location:** `DeMark\DemarkPivotModel.cs`

- Finds pivot highs where `t-2.high > t0, t-1, t-3, t-4`
- Finds pivot lows where `t-2.low < t0, t-1, t-3, t-4`
- Identifies descending trendline from two pivot highs
- Detects when price breaks above/below trendlines
- Forecasts next trendline value if not yet broken

### ML Engine Wrapper
**Location:** `ML Code\MLEngineWrapper.cs`

- Loads serialized ML.NET FastTree models from `.zip` files
- Creates `PredictionEngine` instances for each pattern type
- Used by pattern detection methods to score candidate setups

### Google Sheets Integration
**Location:** `GoogleSheets\WorkSheetGoogleSync\`

- Authenticates using `credentials.json` service account
- `Synchronize()` - Overwrites entire worksheet with new data
- `WriteSetupsToWorksheet()` - Formats and writes results:
  - Row 0: Timestamp
  - Row 1+: Only tickers with at least one successful pattern
  - Columns: Ticker, TrendBreak Date, Forecasted Break, ML Results, Confidence Scores

### Volume Profile Analysis
**Location:** `VolumeProfileLastFiveDays.cs`

- Fetches 1-minute candles for last 6 days
- Groups by session date
- Calls `Studies.ComputeFullVolumeProfileForPreviousDays()` from SchwabLib
- Returns POC (Point of Control) values showing most-traded price levels
- Used in `WriteSetupsToWorksheet()` to compute consecutive up/down sessions

### Database Models
**Location:** `SQLModels\`

Models represent SQL Server tables:
- `SetupInstancesModel` - Parent record (ticker + date)
- `TwoBarModel`, `ThreeBarModel`, `FourBarModel`, `FiveBarModel` - Pattern results
- `RSI4LongModel` - RSI indicator results
- `TrendBreaksModel` - Trendline break information

## External Dependencies

### NuGet Packages
- `Microsoft.ML.FastTree` - Machine learning models
- `Google.Apis.Sheets.v4` / `Google.Apis.Auth` - Google Sheets API
- `System.Data.SqlClient` - SQL Server database access
- `MIConvexHull` - Convex hull computation (likely for pivot analysis)

### SchwabLib Project Reference
**Location:** `..\..\..\msirepos\SchwabLib\SchwabLib.csproj`

Provides:
- `APIWrapper.GetCandles()` - Retrieves historical candle data
- `Studies.ComputeFullVolumeProfileForPreviousDays()` - Volume profile calculations
- `Charting.GeneratePNG()` - Chart image generation
- `Candle`, `GetCandleModel`, `PivotPointModel` types

## Configuration

### App.config
- `spreadsheetid` - Google Sheets document ID

### Credentials
- `credentials.json` - Google Sheets API service account credentials

### Authentication
- `GetAuthKey()` - Retrieves Schwab API authentication key (implementation in `GetAuthKey.cs`)

## Output Files

### Generated Assets
- `c:\work\charts\{ticker}.png` - Chart images for successful patterns
- `c:\work\tickersinplay.txt` - List of tickers with setups (for TOS import)
- `c:\work\VolProfileList.txt` - Volume profile consecutive day counts
- `c:\work\dividends.txt` - Dividend yield analysis (separate feature)

### Google Sheets
- **"Tickers" sheet** - All processed tickers with pivot data
- **"Sheet1"** - Only tickers with triggered buy signals

### SQL Database
- Tables populated via `SaveModelsToDatabase()` method
- Connection string likely in App.config (not visible in code review)

## Training vs Live Modes

The application has two operational modes:

### Training Mode (Commented Out)
Methods like `DoTwoBarTraining()`, `DoThreeBarTraining()` generate synthetic training data and train ML models. These are called from Main but currently commented out.

### Live Mode (Active)
- Loads pre-trained models from disk
- Analyzes real market data from Schwab API
- Generates actionable trading signals

## Pattern Types

All patterns are bullish gap setups looking for continuation:

1. **2-Bar Pattern** - Gap up + inside bar
2. **3-Bar Pattern** - Gap up + 2-bar consolidation
3. **4-Bar Pattern** - Gap up + 3-bar consolidation
4. **5-Bar Pattern** - Gap up + 4-bar consolidation
5. **RSI4 Long** - RSI indicator-based entry signal

Common validation:
- Gap must not fail (closes stay above pre-gap close)
- Gap size thresholds (logarithmic calculations)
- Consolidation bars must respect gap day range

## Architecture Notes

- Partial classes split functionality across multiple files (Program, TickerListWorksheetModel, etc.)
- Heavy use of LINQ for data manipulation
- Synchronous operations with `.Wait()` on async methods
- Models stored in `Models\` directory with subdirectories per model type
- Gap setup code organized by bar count in `GapSetups\{N}BarSetup\` folders

## Code Organization

```
PatternTrainer/
├── Program.cs, Main.cs, Run.cs       # Entry points and main execution
├── GetAuthKey.cs                      # Schwab API authentication
├── Models/                            # Data models
│   ├── TickerListWorksheetModel/     # Ticker list from Google Sheets
│   ├── SetupWorkSheetModel.cs        # Results worksheet model
│   ├── MLResult.cs                   # ML prediction result
│   └── WorksheetBase.cs              # Base class for worksheet models
├── GapSetups/                         # Pattern detection logic
│   ├── 2BarSetup/, 3BarSetup/, etc.  # N-bar pattern implementations
│   └── GenerateGapSetupTrainingData/ # Training data generation
├── DeMark/                            # Technical analysis
│   └── DemarkPivotModel.cs           # Pivot point detection
├── RSISetups/                         # RSI-based patterns
│   └── DoRSI4Live.cs
├── ML Code/                           # Machine learning infrastructure
│   ├── MLEngineWrapper.cs            # Model loading and prediction
│   └── SampleCode/                   # Example code
├── GoogleSheets/                      # Google Sheets integration
│   ├── WorkSheetGoogleSync/          # Sync logic (read/write)
│   └── GoogleWorksheetSamples/       # Example usage
├── SQLModels/                         # Database entity models
├── VolumeProfileLastFiveDays.cs      # Volume analysis
├── SaveModelsToDatabase.cs           # Database persistence
├── GeneratePNG.cs                    # Chart generation wrapper
└── HistoricalRun.cs                  # Historical backtesting (unused in main flow)
```

## Typical Workflow

1. Maintain ticker list in Google Sheets "Tickers" tab
2. Run PatternTrainer console application daily (likely scheduled)
3. Reviews results in Google Sheets "Sheet1" tab showing triggered setups
4. Examines generated charts in `c:\work\charts\` directory
5. Imports `tickersinplay.txt` into ThinkOrSwim for watchlist
6. Historical data stored in SQL Server for backtesting/analysis
