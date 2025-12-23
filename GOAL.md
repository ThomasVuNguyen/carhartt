## Tech Spec: Minimal Windows Testing Browser (Chromium via WebView2)

### 1) Objective

Deliver a minimal, stable Windows desktop browser suitable for testing websites and debugging, built on the Chromium engine via Microsoft Edge WebView2.

### 2) Scope

**In-scope (MVP):**

* Single-window app with one tab (tabs optional later).
* Address bar with Enter-to-navigate.
* Back / Forward / Reload / Stop.
* F12 opens DevTools.
* Dedicated user profile (cache/cookies) directory per channel (dev/test).
* Basic error handling page for navigation failures.
* Command-line launch: `MyBrowser.exe <url>`.

**Out-of-scope (initial):**

* Extensions, sync, downloads manager, bookmarks, multi-profile UI, built-in adblock, password manager.

### 3) Target Platform

* Windows 10/11 (x64 initially).
* Web engine: WebView2 (Edge/Chromium runtime).

### 4) Technology Choices

* **Language/UI:** C# + WPF (fastest iteration) *or* WinUI 3 (more modern).
  (Spec assumes C# + WPF, but the architecture is UI-framework-agnostic.)
* **Engine:** WebView2 SDK (Evergreen runtime by default).
* **Build:** .NET 8, MSBuild, Visual Studio 2022.

### 5) High-Level Architecture

Use a clean separation so you can later swap WebView2 → CEF (for macOS/Linux) without rewriting browser logic.

**Modules**

1. **AppShell (UI Layer)**

   * Window chrome, toolbar (address bar, buttons), status indicator.
   * Input routing (keyboard shortcuts, mouse nav).
2. **BrowserCore (Platform-Agnostic Logic)**

   * URL normalization (treat plain text as search query vs URL).
   * Navigation commands and state model (`CanGoBack`, `IsLoading`, `Title`, `CurrentUrl`).
   * Session persistence hooks (optional later).
3. **Engine Adapter (Platform-Specific)**

   * `IWebEngineView` interface + `WebView2EngineView` implementation.
   * Bridges engine events → BrowserCore events.

**Core Interface (contract)**

* `Initialize(userDataDir, settings)`
* `Navigate(url)`
* `GoBack() / GoForward() / Reload() / Stop()`
* `OpenDevTools()`
* Events: `UrlChanged`, `TitleChanged`, `LoadingStateChanged`, `NavigationError`

### 6) Data & Configuration

* **User Data Directory:** `%LOCALAPPDATA%\MyBrowser\Profiles\<channel>\`

  * `<channel>` = `dev` / `test` (configurable)
* **Config file:** `%LOCALAPPDATA%\MyBrowser\config.json`

  * default homepage, default search engine template, flags.

### 7) Runtime & Distribution

* Default: **Evergreen WebView2 Runtime** (simplest install footprint).
* Optional (later): Fixed-version runtime for reproducible engine testing.

### 8) Logging & Diagnostics

* File logs: `%LOCALAPPDATA%\MyBrowser\Logs\yyyy-mm-dd.log`
* Include:

  * WebView2 runtime version
  * navigation start/stop timestamps
  * errors + HRESULTs
  * key lifecycle events
* Debug toggles:

  * `--user-data-dir=...`
  * `--log-level=debug`
  * `--disable-gpu` (optional troubleshooting)

### 9) Security/Privacy Baseline (MVP)

* No telemetry by default.
* Clear data by deleting profile folder (documented).
* HTTPS errors handled by default engine behavior (no custom overrides in MVP).

### 10) Acceptance Criteria

MVP is complete when:

* App launches in <2 seconds on a typical dev machine.
* `MyBrowser.exe https://example.com` opens the page reliably.
* Address bar navigation works and updates on redirects.
* Back/Forward/Reload/Stop work correctly.
* F12 opens DevTools for the current page.
* Profile isolation works (separate folders produce separate cookies/session state).
* Navigation failures show a readable error state (not a crash).

### 11) Milestones

1. **M1: Skeleton + Engine Bring-up**

   * Window + embedded WebView2 + hardcoded navigation.
2. **M2: Toolbar + Shortcuts**

   * Address bar, Back/Forward/Reload/Stop, Ctrl+L focus address bar, F12 DevTools.
3. **M3: Profile/Logging + CLI**

   * User data dir selection, persistent config, `exe <url>`, file logging.
4. **M4: Stability Pass**

   * Error handling, edge cases (invalid URLs, offline), basic QA checklist.

### 12) Key Risks / Mitigations

* **Coupling to WebView2 APIs** → enforce `IWebEngineView` boundary early.
* **Runtime availability on target machines** → detect runtime + show install guidance (MVP), consider fixed runtime later.
* **macOS port expectations** → plan to replace Engine Adapter + UI shell; keep BrowserCore portable.

