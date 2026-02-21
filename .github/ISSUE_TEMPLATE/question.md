---
name: Question
about: Ask a question about the Meta SDK training project
title: "[QUESTION] "
labels: question
assignees: AntKinton

---

## Question
How do I fix hand tracking initialization in Meta SDK v85.0.0?

## Context
Working on Module 3 - Hand Tracking exercise. Need to implement hand grabbing for fruits.

## Environment
- Unity Version: 6000.0.27f2
- Meta SDK Version: v85.0.0
- Platform: Meta Quest 2
- Build Type: Debug

## What you've tried
- Added OVRHandPrefab to scene
- Enabled hand tracking in XR Plugin Management
- Modified HandTrackingScript from documentation

## Describe alternatives you've considered
- Using OVRGrabber instead of direct hand tracking
- Implementing custom hand detection with OVRInput
- Downgrading to Meta SDK v76.0.1 (worked in previous exercise)
- Using Unity's XR Hand Tracking instead of Meta's implementation

## Expected vs Actual
- **Expected:** Hands appear and can grab fruits
- **Actual:** NullReferenceException on OVRHand reference

## Additional context
Error occurs in Start() method. Using the exact code from Module 3 documentation. Screenshot of Unity Console attached.
