---
name: Bug report
about: Create a report to help us improve
title: "[BUG]"
labels: bug
assignees: AntKinton

---

## Describe the bug
Hand tracking doesn't work when building to Meta Quest 2, but works in Unity Editor.

## To Reproduce
Steps to reproduce the behavior:
1. Open Unity project with Meta SDK v85.0.0
2. Go to Scene: EnvironmentIntegration
3. Enable Hand Tracking in XR Plugin Management
4. Add OVRHandPrefab to scene
5. Build and run on Meta Quest 2
6. See error: Hands don't appear

## Expected behavior
Hands should appear and be trackable in Meta Quest 2 build.

## Actual behavior
Hands are invisible/NullReferenceException when accessing hand data.

## Screenshots
- Unity Console: Shows OVRHand reference error
- Quest 2 view: No hands visible
- XR Plugin settings: Hand tracking enabled

## Environment:
- **Unity Version:** 6000.0.27f2
- **Meta SDK Version:** v85.0.0
- **Platform:** Meta Quest 2
- **Build Type:** Debug
- **Target Device:** Meta Quest 2

## VR/XR Specific Information:
- **XR Plugin Management:** Meta XR Plugin enabled
- **Hand Tracking:** Enabled
- **Passthrough:** Disabled
- **OVR Version:** 85.0.0

## Error Messages
Unity Console: "NullReferenceException: OVRHand reference not set"
Quest Logcat: "Failed to initialize hand tracking system"

## Additional context
Working on Module 3 - Hand Tracking exercise. Was trying to implement hand grabbing for fruits. Recently updated from Meta SDK v76.0.1 to v85.0.0.
