# Updating `LiveTests` for `Record/Playback`

This document will eventually contain a guide to enable Record/Playback for an existing `LiveTest` package. Until then it will contain a local todo for remaining feature work that needs to be finished.

## TODO:

- [x] Identify necessary implementation details
- [x] Add test-proxy class fixture that handles auto-start/stop of the test-proxy per class definition.
- [x] Add start/stop playback and recording to `InitializeAsync` and `Dispose`
- [ ] The current generated client looks like it's using System.ClientModel instead of Azure.Core. Not a huge deal but need to examine further
      to see if we can go around this. Currently we're calling with placeholder values when we should be discovering the file path for the test class
- [ ] Update test-proxy typespec to reflect --http-proxy mode and utilize so we don't have to call with placeholder recordingIds
- [x] Abstract location of assets.json
- [x] Abstract relative path to the recording for a given test.
- [x] Utilize above steps to properly provide relative recording path to StartPlayback() and StartRecording()
- [ ] Add delegating handler to our core http client creation pipeline under debug flags
- [ ] Test combinations of test parameters to ensure our file uniqueness is g2g
- [ ] Integrate
- [ ] Gather feedback
- [ ] Copilot instructions for helping record a test
- [ ] Update readme AND copilot instructions with basic context so that copilot can adequately explain the problem space to users
- [ ] Update readme with basic introduction to utilizing proxy class fixture to enable the test-proxy
