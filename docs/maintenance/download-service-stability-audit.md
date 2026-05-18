# Download service stability audit

Phase 7 scope: audit-only maintenance review of the download pipeline. This document records observed behavior and follow-up recommendations only; it does not change runtime behavior.

## 1. Download pipeline overview

Current flow using repository class and method names:

1. **User action / task creation**
   - `AddToDownloadService.AddToDownload()` validates selected pages, builds a sanitized base file name through `FileName.Builder(...).Set...()` and `Format.FormatFileName(...)`, combines it with the selected directory, optionally adds a numeric suffix for repeated final base paths, creates `DownloadBase`, `Downloading`, and `DownloadingItem`, persists it with `DownloadStorageService.AddDownloading()`, then adds it to `App.DownloadingList` inside `App.PropertyChangeAsync()`.
   - The UI starts, pauses, resumes, retries, and deletes individual downloads through `DownloadingItem.ExecuteStartOrPauseCommand()` and list-level operations through `ViewDownloadingViewModel` commands.

2. **Video stream info acquisition**
   - Existing page parse state is carried in `DownloadingItem.PlayUrl` when the item is added.
   - `DownloadService.SingleDownload()` calls the concrete `Parse()` implementation, which currently delegates to `DownloadService.BaseParse()` in both `BuiltinDownloadService.Parse()` and `AriaDownloadService.Parse()`.
   - `BaseParse()` populates `DownloadingItem.PlayUrl` with `VideoStream.GetVideoPlayUrl()`, `VideoStream.GetVideoPlayUrlWebPage()`, `VideoStream.GetBangumiPlayUrl()`, or `VideoStream.GetCheesePlayUrl()` based on `Downloading.PlayStreamType`.

3. **Download task creation**
   - `BuiltinDownloadService.Start()` and `AriaDownloadService.Start()` call `DownloadService.BaseStart()`, which creates `TokenSource`, assigns `CancellationToken`, and starts `WorkTask = Task.Run(DoWork)`.
   - `DownloadService.DoWork()` loops over `DownloadingList`, gates concurrency with `_downloadSemaphore`, sets eligible tasks to `DownloadStatus.Downloading`, and adds `SingleDownload(downloading).ContinueWith(_ => _downloadSemaphore.Release())` to `DownloadingTasks`.

4. **Built-in downloader or aria2 path**
   - Built-in mode: `BuiltinDownloadService.DownloadAudio()` / `DownloadVideo()` select stream metadata through `BaseDownloadAudio()` / `BaseDownloadVideo()`, derive a GUID temp file name stored in `Downloading.DownloadFiles`, and call `BuiltinDownloadService.DownloadByBuiltin()`.
   - aria2 mode: `AriaDownloadService.DownloadAudio()` / `DownloadVideo()` follow the same metadata and temp-file bookkeeping, then call `AriaDownloadService.DownloadByAria()`.
   - `CustomAriaDownloadService` contains a similar aria2 path for externally configured aria2.

5. **Temporary files**
   - Media segment temp files are GUID-like names without extensions in the final output directory.
   - `Downloading.DownloadFiles` maps stream keys such as `{qualityId}_{codecs}` to temp file names, and `Downloading.DownloadedFiles` records completed stream keys for resume/retry decisions.
   - Cover, subtitle, danmaku, and NFO sidecar files are written directly next to the final output base path, not through a separate temp directory.

6. **FFmpeg mux / merge**
   - DASH audio/video outputs are merged by `DownloadService.BaseMixedFlow()`, which chooses `${DownloadBase.FilePath}.mp4`, `.mp3`, `.flac`, or `.aac`, then calls `FFMpeg.Instance.MergeVideo(audioUid, videoUid, finalFile)`.
   - multi-`durl` video segments are concatenated by `DownloadService.ConcatVideos()`, which calls `FFMpeg.Instance.ConcatVideos(videoUids, finalFile, ...)`.

7. **Final output**
   - On success, `BaseMixedFlow()`/`ConcatVideos()` sets `DownloadingItem.FileSize` from the final file if it exists.
   - `FFMpeg.MergeVideo()` deletes the input temp audio/video files after FFmpeg completes, regardless of whether the caller later verifies the output file.
   - `FFMpeg.ConcatVideos()` writes a temporary ffmpeg concat list file under `Path.GetTempPath()` and deletes only the list file; it does not delete source segments.

8. **History / UI status update**
   - `DownloadService.SingleDownload()` validates output sidecar existence, calls `DownloadFailed()` if required outputs are missing, or creates a `DownloadedItem` with finished timestamp and max speed.
   - On success, it removes the active row with `DownloadStorageService.RemoveDownloading()`, writes history with `DownloadStorageService.AddDownloaded()`, then uses `App.PropertyChangeAsync()` to add to `DownloadedList`, remove from `DownloadingList`, and sort finished downloads.

## 2. Large-file memory risk

Overall classification: **P2 edge-case memory pressure**, with no evidence of full video/audio file buffering in the primary download paths.

Findings:

- `WebClient.DownloadFile()` streams cover downloads using `RequestStream(..., HttpCompletionOption.ResponseHeadersRead)` and `stream.CopyTo(fs)`. This does not buffer the entire cover payload in memory. Severity: **P3**.
- `BuiltinDownloadService.DownloadByBuiltin()` uses the `Downloader` package with `MaximumMemoryBufferBytes = 1024 * 1024 * 50`. This bounds in-process buffering to 50 MiB per active built-in downloader instance, but concurrent downloads can multiply that memory pressure. Severity: **P2**. Recommended fix: make the memory budget explicit in documentation/settings or cap aggregate concurrency/memory in a narrow PR. Safe as a small PR: **yes**.
- aria2 mode delegates large payload storage to aria2 and reports status through JSON-RPC. No full media file buffering was observed in `AriaDownloadService.DownloadByAria()` or `AriaManager.GetDownloadStatus()`. Severity: **P3**.
- `FFMpeg.ExtractVideoFrame()` uses a `MemoryStream`, but it is for a single extracted image frame and is not part of the main download/mux path. Severity: **P3**.
- `VideoStream.GetSubtitle()` reads subtitle JSON and builds SRT strings in memory, and `BaseDownloadSubtitle()` writes them with `File.WriteAllText()`. Subtitle payloads are small compared with video/audio; this is not a primary large-file risk. Severity: **P3**.
- `DownloadingItem` stores stream metadata (`PlayUrl`), text metadata (`MovieMetadata`), and progress/status fields. No large binary content is stored in UI model objects. Severity: **P3**.

## 3. Cancellation behavior

Findings:

1. `DownloadService.BaseEndTask()` cancels `TokenSource` and awaits `WorkTask`. `DownloadService.DoWork()` observes `CancellationToken` in its loop, in `_downloadSemaphore.WaitAsync(CancellationToken.Value)`, and before exit waits up to 30 seconds for active `DownloadingTasks`. Observed behavior: service-level cancellation can stop task scheduling, but it does not pass a token into `SingleDownload()` or FFmpeg calls. Risk: active work may continue until polling checks or blocking operations return. Severity: **P1**. Recommended fix: thread a cancellation token through `SingleDownload()`, media download methods, and mux/sidecar phases. Safe as a small PR: **yes, if limited to cancellation wiring**.

2. `BuiltinDownloadService.DownloadByBuiltin()` polls `CancellationToken?.ThrowIfCancellationRequested()` every 100 ms while waiting for `isComplete`, but the `Downloader.DownloadFileTaskAsync()` task is started without awaiting it or passing a cancellation token. Observed behavior: cancellation exits the polling loop by exception, but the underlying downloader may keep running unless pause/delete status is also processed. Risk: background transfer can outlive the service/task status and leave partial or continued writes. Severity: **P1**. Recommended fix: retain and cancel/stop the downloader task/service explicitly on service cancellation. Safe as a small PR: **yes**.

3. Built-in pause and delete are status-driven rather than token-driven. `DownloadingItem.ExecuteStartOrPauseCommand()` sets `DownloadStatus.Pause`; the polling loop calls `downloader.Pause()` and then `Pause(downloading)`, which throws `OperationCanceledException`. Delete removes the item from `App.DownloadingList`; `Pause(downloading)` checks `DownloadingList.Contains(downloading)` and throws if missing. Observed behavior: pause/delete can break out of `SingleDownload()`, but cleanup/status normalization is mostly implicit. Risk: inconsistent status and orphaned temp files. Severity: **P2**. Recommended fix: centralize pause/delete/cancel exits and persist a clear paused/deleted state. Safe as a small PR: **yes**.

4. `AriaDownloadService.DownloadByAria()` passes a polling `Action` into `AriaManager.GetDownloadStatus()`. That action checks `CancellationToken`, pauses aria2 on `DownloadStatus.Pause`, then calls `Pause(downloading)`. Delete path is handled in `AriaDownloadService.IsExist()`, which unpauses, removes the aria2 task, then removes the aria2 result. Observed behavior: aria2 has explicit pause/remove calls for pause/delete, but service cancellation through `CancellationToken` only throws from the polling action; it does not remove or pause the aria2 GID in that cancellation branch. Risk: aria2 downloads may continue after service shutdown/cancellation until `CloseAriaServer()` calls `PauseAllAsync()` and closes the server. Severity: **P1**. Recommended fix: on token cancellation, pause/remove the current GID consistently before leaving `DownloadByAria()`. Safe as a small PR: **yes**.

5. `DownloadService.BaseMixedFlow()` and `FFMpeg.MergeVideo()` have no cancellation token parameter. Once mux starts, user pause/delete/service cancellation does not interrupt the FFmpeg process. Risk: large mux operations can continue after user cancellation and then delete temp inputs. Severity: **P1**. Recommended fix: use FFMpegCore async processing with cancellation or process cancellation support, and check cancellation immediately before and after mux. Safe as a small PR: **yes, if scoped to mux cancellation only**.

6. No failure/cancellation path deletes or quarantines partial media temp files in `SingleDownload()`, `DownloadFailed()`, `DownloadByBuiltin()`, or `DownloadByAria()`. Observed behavior: partial files may remain for resume, but policy is implicit and inconsistent with output temp/final behavior. Risk: disk growth and confusing retry behavior. Severity: **P2**. Recommended fix: define policy: preserve resumable temp files on failure, delete only abandoned/known-invalid partial files on explicit delete/cancel, and record the policy in state. Safe as a small PR: **yes**.

## 4. Retry and failure recovery

Findings:

- HTTP metadata retry: `WebClient.RequestWeb()` retries recursively until `retry <= 0`, defaulting to two attempts, logging `HttpRequestException` and other exceptions. `VideoStream.GetPlayUrl()` uses this for playURL acquisition. Risk: retries are present for metadata, but failed metadata returns an empty response that later deserializes to null/failure without rich user context. Severity: **P2**. Recommended fix: propagate parse failure context into `DownloadFailed()` or user-visible status. Safe as a small PR: **yes**.

- Built-in media retry: `DownloadService.SingleDownload()` retries audio and video up to `Retry = 5` when `DownloadAudio()` / `DownloadVideo()` returns `NullMark`. `BuiltinDownloadService.DownloadByBuiltin()` tries each URL in order, but returns after the first URL loop iteration regardless of success/failure, so backup URLs after the first attempted URL are not reached by that method. The outer retry calls the same URL list again. Risk: common failure when base URL fails but backup URLs would work. Severity: **P1**. Recommended fix: continue to the next URL on failed first URL before returning. Safe as a small PR: **yes**.

- aria2 retry: `DownloadService.SingleDownload()` uses the same outer retry loop for aria2 because `AriaDownloadService.DownloadVideo()` returns `NullMark` on `DownloadResult.FAILED` or `ABORT`. `AriaDownloadService.StartAriaServer()` configures `ContinueDownload = true`, but `MaxTries` is commented out. Risk: aria2 resume is partly supported by aria2 and persisted GID/temp names, but retry count and failure semantics differ from built-in mode. Severity: **P2**. Recommended fix: explicitly set/record aria2 retry policy and map aria2 failure states to DownKyi states. Safe as a small PR: **yes**.

- Resume: both built-in and aria2 store temp file names in `Downloading.DownloadFiles` and completed keys in `Downloading.DownloadedFiles`; `DownloadStorageService.UpdateDownloading()` persists those maps. On service end, active downloads are reset to `WaitForDownload` and progress to zero. Risk: completed component files can be reused, but in-progress partial handling depends on the downloader/aria2 and is not clearly reflected to users. Severity: **P2**. Recommended fix: persist a resumable/partial state and show resume behavior in UI text. Safe as a small PR: **yes**.

- FFmpeg failure handling: `FFMpeg.MergeVideo()` logs arguments and stderr through `NotifyOnError`, deletes input temp files after `ProcessSynchronously(false)`, and returns `true`; `DownloadService.BaseMixedFlow()` ignores the return value and checks `File.Exists(finalFile)`. Risk: if FFmpeg fails after consuming inputs, retry may have to redownload large media files. Severity: **P1**. Recommended fix: delete temp inputs only after confirmed successful final output, and return/consume a success flag. Safe as a small PR: **yes**.

- Failed task status: `DownloadService.DownloadFailed()` sets UI fields, `DownloadStatus.DownloadFailed`, and retry icon. It does not call `DownloadStorageService.UpdateDownloading()`. Risk: failure state may not be persisted until service end or other storage update. Severity: **P2**. Recommended fix: persist failure state immediately in `DownloadFailed()`. Safe as a small PR: **yes**.

## 5. Temporary file and overwrite safety

Findings:

1. Media temp naming uses `Guid.NewGuid().ToString("N")` in both built-in and aria2 paths, stored under `Downloading.DownloadFiles`. Collision risk is low. Severity: **P3**. Recommended fix: optional: add a media-temp prefix/extension for diagnosability. Safe as a small PR: **yes**.

2. Path derivation uses `downloading.DownloadBase.FilePath.TrimEnd(temp[^1].ToCharArray())` / `TrimEnd(temp[temp.Length - 1].ToCharArray())` to derive the directory. `TrimEnd(char[])` removes any trailing combination of filename characters, not the exact filename suffix. Risk: edge-case wrong output directory when parent directory ends with characters contained in the filename. Severity: **P2**. Recommended fix: replace with `Path.GetDirectoryName(downloading.DownloadBase.FilePath)` in a narrow path-safety PR. Safe as a small PR: **yes**.

3. Final output overwrite: `FFMpeg.MergeVideo()` and `FFMpeg.ConcatVideos()` call `OutputToFile(..., true, ...)`, allowing overwrite. `AddToDownloadService.AddToDownload()` can auto-add numeric suffixes if the base filename already exists, but this is optional and checks existing file names without extension at add time only. Risk: final output may overwrite an existing file if repeat suffixing is disabled or a file appears between add and mux. Severity: **P1** if user data is overwritten; otherwise **P2**. Recommended fix: check final path immediately before mux and apply explicit overwrite policy. Safe as a small PR: **yes**.

4. Sidecar overwrite: subtitles, danmaku, cover, and NFO are written directly to final sidecar names with `File.WriteAllText()`, `Bilibili.Create(..., assFile)`, `WebClient.DownloadFile(..., fileName)`, and `XmlWriter.Create(filePath, settings)`. Risk: sidecars can overwrite existing user files with matching names. Severity: **P2**. Recommended fix: route sidecar writes through the same overwrite policy as media output. Safe as a small PR: **yes**.

5. Cleanup after success: `FFMpeg.MergeVideo()` deletes DASH temp audio/video files after mux. `FFMpeg.ConcatVideos()` does not delete input segment files after concatenation; it only deletes the concat list file. Risk: multi-segment `durl` downloads can leave large temp segment files after successful mux/concat. Severity: **P2**. Recommended fix: after confirmed successful concat, delete only the segment temp files referenced by the task. Safe as a small PR: **yes**.

6. Cleanup on failure/cancellation: no explicit cleanup is performed for partially downloaded files or partial final outputs. Risk: disk growth and ambiguous retry source. Severity: **P2**. Recommended fix: implement explicit failure/cancellation cleanup policy. Safe as a small PR: **yes**.

## 6. Aria2 vs built-in downloader consistency

| Area | Built-in downloader | aria2 downloader | Consistency risk |
|---|---|---|---|
| Progress reporting | `DownloadProgressChanged` directly updates `DownloadingItem.Progress`, `DownloadingFileSize`, `SpeedDisplay`, and `MaxSpeed`. | `AriaTellStatus()` finds the item by GID and directly updates the same fields. | Both update UI-bound properties from downloader/polling callbacks without dispatcher marshaling. Severity: **P1/P2** depending on callback thread. |
| Cancellation | Polling loop checks `CancellationToken`; pause calls `downloader.Pause()` then throws through `Pause()`. | Polling action checks `CancellationToken`; pause calls `AriaClient.PauseAsync()` then throws through `Pause()`. | Both rely on exceptions from polling, but only aria2 delete path removes the remote task. Built-in cancellation does not explicitly cancel the downloader task. Severity: **P1**. |
| Retry | Outer `SingleDownload()` loop retries 5 times; `DownloadByBuiltin()` does not proceed to backup URLs after one attempt. | Outer `SingleDownload()` loop retries 5 times; aria2 receives all URLs in one `AddUriAsync()` call. | Backup URL handling differs. Severity: **P1** for built-in common fallback failure. |
| Output path | Both derive `path` by `TrimEnd(...)` and use a GUID temp file name. | Same. | Shared path edge-case. Severity: **P2**. |
| Temporary files | Uses `Downloader` partial behavior and stored temp names. | Uses aria2 with `ContinueDownload = true`, stored GID and temp names. | Resume/partial semantics are not presented consistently to users. Severity: **P2**. |
| Error reporting | `BuiltinDownloadService.DownloadVideo()` catches only `FileNotFoundException` around `DownloadByBuiltin()`. | `AriaManager.GetDownloadStatus()` logs aria2 error message and returns `FAILED`. | Built-in non-`FileNotFoundException` failures may escape to `SingleDownload()` without `DownloadFailed()` handling. Severity: **P2**. |
| Final merge | Both call `BaseMixedFlow()`/`ConcatVideos()` after downloads. | Same. | Mux risks are shared. Severity: **P1/P2**. |

## 7. FFmpeg mux / merge robustness

Findings:

1. `FFMpeg` constructor configures `BinaryFolder = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "ffmpeg")` and immediately calls `FFMpegHelper.VerifyFFMpegExists(...)`. Observed behavior: FFmpeg availability is validated at singleton construction, but the failure path is not caught where `FFMpeg.Instance` is used. Risk: missing FFmpeg can fail the active download phase abruptly. Severity: **P1**. Recommended fix: validate FFmpeg during app startup/settings diagnostics or catch and surface a clear mux-phase error. Safe as a small PR: **yes**.

2. `FFMpeg.MergeVideo()` checks `if (!File.Exists(audio) && !File.Exists(video)) return false;`, but then initially builds arguments with `.FromFileInput(audio).AddFileInput(video)` before replacing arguments for audio-only/video-only cases. If one input is null/missing, argument construction may see a null/missing path before the branch corrects it. Risk: edge-case exception before fallback branch. Severity: **P2**. Recommended fix: branch before building FFmpeg arguments. Safe as a small PR: **yes**.

3. `FFMpeg.MergeVideo()` logs FFmpeg arguments and stderr via `NotifyOnError`, but not stdout. It returns `true` after process execution without checking the process result or final output. `BaseMixedFlow()` checks only `File.Exists(finalFile)`. Risk: diagnostics can miss success output/progress and cannot distinguish corrupt/zero-byte final files from success. Severity: **P2**. Recommended fix: return a structured mux result including process success, output existence, size, and stderr. Safe as a small PR: **yes**.

4. `FFMpeg.MergeVideo()` deletes input temp files after `ProcessSynchronously(false)` even if FFmpeg fails and final output is missing. Risk: retry may require redownloading large media inputs. Severity: **P1**. Recommended fix: delete inputs only after confirmed successful final output and nonzero length. Safe as a small PR: **yes**.

5. `FFMpeg.ConcatVideos()` validates input files, writes a concat list file as `flvlist_{DateTime.Now:yyyyMMddHHmmss}.txt` under `Path.GetTempPath()`, overwrites final output, and deletes the list file in a small `try { File.Delete(listFile); } catch { }`. Risk: list-file name can collide for concurrent concat operations within the same second; cleanup silently swallows delete errors without a comment; source segment cleanup is absent. Severity: **P2**. Recommended fix: use a GUID temp list file, log cleanup failures, and delete source segments only after confirmed concat success. Safe as a small PR: **yes**.

## 8. Concurrency and collection safety

Findings:

1. `DownloadService.DoWork()` enumerates `DownloadingList` while UI commands can add/remove items through `App.PropertyChangeAsync()` or direct UI-thread operations. `ImmutableObservableCollection<T>` uses immutable snapshots for enumeration, reducing collection-modified exceptions, and `DoWork()` catches `InvalidOperationException`. Severity: **P3** for enumeration stability.

2. `_downloadSemaphore` is initialized once from `SettingsManager.GetInstance().GetMaxCurrentDownloads()`. Runtime setting changes are not reflected until service recreation. Risk: maintainability/user expectation issue, not immediate stability failure. Severity: **P3**. Recommended fix: document or re-create semaphore when settings change. Safe as a small PR: **yes**.

3. `DownloadingTasks` is a mutable `List<Task>` used by `DoWork()` only for add/remove and exit waits; `ContinueWith` only releases the semaphore. Low direct race risk in current code, but `ContinueWith` does not observe exceptions from `SingleDownload()` or log them. Severity: **P2**. Recommended fix: make task creation await/log failures in a helper method and release semaphore in `finally`. Safe as a small PR: **yes**.

4. `DownloadService.SingleDownload()` mutates `DownloadingItem` fields from inside `Task.Run(...)`; built-in and aria2 callbacks also mutate the same object. UI commands mutate status and icons concurrently. Risk: race between pause/cancel/delete/complete can produce stale icons/status or writes after deletion. Severity: **P2**. Recommended fix: confine state transitions to a small synchronized state helper and marshal UI-bound properties. Safe as a small PR: **yes**.

5. `Downloading.DownloadFiles` is a normal `Dictionary<string,string>` and `Downloading.DownloadedFiles` is a normal `List<string>`. They are mutated by download tasks and persisted by storage methods. Current design schedules one `SingleDownload()` per item, so concurrent mutation per item is unlikely, but delete/service-end persistence can read while a task mutates. Severity: **P2**. Recommended fix: snapshot these collections before persistence or guard per-item state with a lock. Safe as a small PR: **yes**.

## 9. UI thread update safety

Findings:

1. `AddToDownloadService.AddToDownload()` uses `App.PropertyChangeAsync()` before adding to `App.DownloadingList`. Success completion in `DownloadService.SingleDownload()` uses `App.PropertyChangeAsync()` before moving items between `DownloadedList` and `DownloadingList`. These collection updates are marshaled through the app helper. Severity: **P3** positive finding.

2. `BuiltinDownloadService.DownloadByBuiltin()` updates `DownloadingItem.Progress`, `DownloadingFileSize`, `SpeedDisplay`, and `Downloading.MaxSpeed` inside `DownloadProgressChanged` without `Dispatcher.UIThread` or `App.PropertyChangeAsync()`. `DownloadingItem` property setters call `RaisePropertyChanged()`. Risk: if `Downloader` raises progress on a worker thread, Avalonia bindings can receive property changed events off the UI thread. Severity: **P1**. Recommended fix: marshal progress property updates to the UI thread or add a thread-safe progress sink. Safe as a small PR: **yes**.

3. `AriaDownloadService.AriaTellStatus()` directly updates the same UI-bound properties from `AriaManager.GetDownloadStatus()` polling, which runs within the download task, not the UI thread. Risk: off-UI-thread property changed events. Severity: **P1**. Recommended fix: marshal aria2 progress updates to the UI thread. Safe as a small PR: **yes**.

4. `DownloadService.SingleDownload()` updates `DownloadStatusTitle`, `DownloadContent`, `Progress`, `DownloadingFileSize`, `SpeedDisplay`, `FileSize`, and `StartOrPause` from inside `Task.Run(...)` and helper methods such as `BaseDownloadAudio()`, `BaseDownloadVideo()`, `BaseMixedFlow()`, and `DownloadFailed()`. Risk: broad off-UI-thread property notifications. Severity: **P1**. Recommended fix: centralize `DownloadingItem` UI updates behind a dispatcher helper. Safe as a small PR: **yes, if mechanical and scoped**.

5. `ViewDownloadingViewModel.ExecuteDeleteCommand()` removes from `App.DownloadingList` directly after awaiting a dialog and therefore likely resumes on the UI thread; `ExecuteDeleteAllDownloadingCommand()` uses `Task.Run()` but wraps each removal in `App.PropertyChangeAsync()`. Severity: **P3** positive finding.

## 10. Logging and diagnosability

Findings:

- `DownloadService.DoWork()` logs exceptions with method tags, and `WebClient.RequestWeb()`, `VideoStream.GetSubtitle()`, `AriaManager.GetDownloadStatus()`, and `FFMpeg` log selected errors/details. This provides basic diagnostics.
- `DownloadService.GenerateNfoFile()` swallows all exceptions with an empty `catch (Exception e) { /**/ }`, losing context. Severity: **P3**. Recommended fix: log NFO generation failures with video/path context. Safe as a small PR: **yes**.
- `BaseDownloadAudio()` catches all exceptions for Dolby/Flac fallback and swallows them without comment-specific logging. Severity: **P3**. Recommended fix: add a comment explaining intentionally ignored missing optional audio variants or log at debug level. Safe as a small PR: **yes**.
- `DownloadFailed()` updates UI state but does not log the video id/BVID, target path, download mode, phase, or exception. Severity: **P2**. Recommended fix: accept/record failure reason and log `{bvid,cid,path,mode,phase,exception}`. Safe as a small PR: **yes**.
- `BuiltinDownloadService.DownloadByBuiltin()` progress/error context lacks the current URL index and local file path in logs. Severity: **P3**. Recommended fix: log selected URL host/index and temp path on failures. Safe as a small PR: **yes**.
- `AriaDownloadService.DownloadByAria()` and `AriaManager.GetDownloadStatus()` log some aria2 errors, but `AriaDownloadFinish()` is empty and does not record success/failure details. Severity: **P3**. Recommended fix: either remove unused event subscription or log completion details including GID/path. Safe as a small PR: **yes**.
- FFmpeg logs stderr and arguments, but `BaseMixedFlow()` does not add video id/path/mode context around mux start/failure. Severity: **P2**. Recommended fix: log mux phase with input paths, output path, BVID/CID, and mode. Safe as a small PR: **yes**.

## 11. Risk table

| ID | Severity | Area | File / Method | Risk | Evidence | Recommended follow-up |
|---|---|---|---|---|---|---|
| DSA-01 | P1 | UI thread safety | `DownKyi/Services/Download/BuiltinDownloadService.cs` / `DownloadByBuiltin()` | Progress updates can raise UI-bound property changes from downloader callback threads. | `DownloadProgressChanged` directly sets `downloading.Progress`, `DownloadingFileSize`, and `SpeedDisplay`. | `fix: marshal built-in download progress updates to UI thread` |
| DSA-02 | P1 | UI thread safety | `DownKyi/Services/Download/AriaDownloadService.cs` / `AriaTellStatus()` | aria2 polling updates UI-bound properties from the download worker task. | `AriaManager.GetDownloadStatus()` invokes `TellStatus`; handler directly sets `video.Progress`, `DownloadingFileSize`, and `SpeedDisplay`. | `fix: marshal aria2 progress updates to UI thread` |
| DSA-03 | P1 | Cancellation | `DownKyi.Core/FFMpeg/FFMpeg.cs` / `MergeVideo()` and `DownKyi/Services/Download/DownloadService.cs` / `BaseMixedFlow()` | FFmpeg mux cannot be canceled once started. | No cancellation token is accepted or checked by mux methods. | `fix: support cancellation around ffmpeg mux phase` |
| DSA-04 | P1 | Failure recovery | `DownKyi.Core/FFMpeg/FFMpeg.cs` / `MergeVideo()` | Temp audio/video files can be deleted after FFmpeg failure, forcing large redownloads. | Inputs are deleted after `ProcessSynchronously(false)` without checking final output or process success. | `fix: preserve recoverable files after ffmpeg mux failure` |
| DSA-05 | P1 | Retry | `DownKyi/Services/Download/BuiltinDownloadService.cs` / `DownloadByBuiltin()` | Backup URLs may not be attempted in built-in mode. | Method loops over `urls` but returns `isComplete` after the first iteration. | `fix: try built-in downloader backup URLs before failing` |
| DSA-06 | P1 | Overwrite safety | `DownKyi.Core/FFMpeg/FFMpeg.cs` / `MergeVideo()` and `ConcatVideos()` | Final output can overwrite existing files. | `OutputToFile(..., true, ...)` enables overwrite; final pre-mux conflict check is absent. | `fix: enforce explicit final output overwrite policy` |
| DSA-07 | P1 | Cancellation | `DownKyi/Services/Download/BuiltinDownloadService.cs` / `DownloadByBuiltin()` | Service cancellation may not stop the underlying `Downloader.DownloadService` transfer. | Download task is started with `DownloadFileTaskAsync(...).ConfigureAwait(false)` but not awaited/canceled. | `fix: harden built-in downloader cancellation cleanup` |
| DSA-08 | P1 | Cancellation | `DownKyi/Services/Download/AriaDownloadService.cs` / `DownloadByAria()` | Token cancellation exits polling without consistently pausing/removing current aria2 GID. | Cancellation is checked in the polling action; GID cleanup exists for delete path in `IsExist()`, not token cancellation. | `fix: harden aria2 cancellation cleanup` |
| DSA-09 | P2 | Path safety | `DownKyi/Services/Download/DownloadService.cs`, `DownKyi/Services/Download/BuiltinDownloadService.cs`, `DownKyi/Services/Download/AriaDownloadService.cs` / path derivation | `TrimEnd(char[])` can derive the wrong directory for edge-case filenames. | Directory is built from `DownloadBase.FilePath.TrimEnd(filename.ToCharArray())`. | `fix: use Path.GetDirectoryName for download output directory` |
| DSA-10 | P2 | Temp cleanup | `DownKyi.Core/FFMpeg/FFMpeg.cs` / `ConcatVideos()` | Multi-segment temp files may remain after successful concat. | Concat deletes only the list file, not input segment temp files. | `fix: cleanup successful concat segment temp files` |
| DSA-11 | P2 | Persistence | `DownKyi/Services/Download/DownloadService.cs` / `DownloadFailed()` | Failed status may not be persisted immediately. | `DownloadFailed()` updates in-memory fields but does not call `DownloadStorageService.UpdateDownloading()`. | `fix: persist failed download state immediately` |
| DSA-12 | P2 | Concurrency | `DownKyi/Services/Download/DownloadService.cs` / `SingleDownload()` and storage calls | Per-item dictionaries/lists can be read for persistence while download task mutates them. | `DownloadFiles` and `DownloadedFiles` are mutable collections persisted as JSON. | `fix: snapshot download file maps before persistence` |
| DSA-13 | P2 | FFmpeg robustness | `DownKyi.Core/FFMpeg/FFMpeg.cs` / `ConcatVideos()` | Temp concat list file can collide under concurrent concat operations in the same second. | List file name uses `flvlist_{DateTime.Now:yyyyMMddHHmmss}.txt`. | `fix: use unique concat list temp file names` |
| DSA-14 | P2 | Diagnostics | `DownKyi/Services/Download/DownloadService.cs` / `DownloadFailed()` | Failure logs lack video id, path, mode, phase, and exception details. | `DownloadFailed()` does not log; callers often call it without a reason. | `fix: add contextual download failure logging` |
| DSA-15 | P3 | Logging | `DownKyi/Services/Download/DownloadService.cs` / `GenerateNfoFile()` | NFO generation failures are silently swallowed. | Empty catch block contains only `/**/`. | `fix: log nfo generation failures` |
| DSA-16 | P3 | Maintainability | `DownKyi/Services/Download/AriaDownloadService.cs` / `AriaDownloadFinish()` | Empty completion handler obscures intended aria2 diagnostics. | Handler is subscribed but body is empty. | `fix: log or remove unused aria2 finish handler` |
| DSA-17 | P3 | Memory | `DownKyi/Services/Download/BuiltinDownloadService.cs` / `DownloadByBuiltin()` | Aggregate memory use scales with concurrent built-in downloads. | Per-download `MaximumMemoryBufferBytes` is 50 MiB. | `docs: document built-in downloader memory budget` |

## 12. Follow-up PR plan

Recommended narrow PR order:

1. `fix: marshal download progress updates to UI thread`
   - Scope: `BuiltinDownloadService.DownloadByBuiltin()`, `AriaDownloadService.AriaTellStatus()`, and minimal shared helper if needed.
   - Why first: reduces crash risk from worker-thread property notifications.

2. `fix: harden download cancellation cleanup`
   - Scope: make built-in and aria2 active transfers respond consistently to service cancellation, pause, and delete; no refactor beyond cancellation handling.
   - Why second: cancellation brokenness is a common user-visible stability problem.

3. `fix: preserve recoverable files after ffmpeg mux failure`
   - Scope: `FFMpeg.MergeVideo()` success detection and input deletion timing; `BaseMixedFlow()` consumes result.
   - Why third: prevents expensive redownloads and data loss during mux failures.

4. `fix: try built-in downloader backup URLs before failing`
   - Scope: `BuiltinDownloadService.DownloadByBuiltin()` URL loop only.
   - Why fourth: improves common recovery without changing the overall pipeline.

5. `fix: enforce explicit final output overwrite policy`
   - Scope: pre-mux final file conflict checks for media and sidecars; no schema changes.
   - Why fifth: prevents accidental overwrite/data loss.

6. `fix: use safe output directory derivation`
   - Scope: replace `TrimEnd(...ToCharArray())` path derivation with `Path.GetDirectoryName()` in download path code.
   - Why sixth: small edge-case correctness fix.

7. `fix: make aria2 and built-in downloader failure states consistent`
   - Scope: map built-in, aria2, parse, and mux failures into a common logged failure reason and immediate persisted status.
   - Why seventh: improves recoverability and user trust.

8. `fix: cleanup concat temp files after confirmed success`
   - Scope: unique concat list file naming and post-success segment cleanup.
   - Why eighth: reduces disk growth while preserving recovery safety.

9. `docs: document download failure recovery behavior`
   - Scope: user/developer docs explaining which partial files are preserved, when retry resumes, and when delete removes temp data.
   - Why ninth: should follow implementation of explicit policies.
