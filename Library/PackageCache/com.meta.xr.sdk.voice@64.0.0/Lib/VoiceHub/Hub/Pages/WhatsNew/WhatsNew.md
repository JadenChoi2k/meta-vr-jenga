# v64 Release Notes and Upgrade Guide



## Upgrading

### From All-In-One or Standalone UnityPackage
If coming from the Unity All-In-One or standalone Voice SDK .unitypackage file ensure all code is removed that previously existed at: Assets/Oculus/Voice.  Samples are now added directly from the UPM interface instead of viewable within the package itself.

### From UPM
In order to upgrade or downgrade via UPM, simply select the desired version from the Unity Package Manager.  It is always safest to delete all generated sample directories on upgrade or downgrade in order to ensure they are compatible with the current version of the Voice SDK.



## What’s New
### In v64
* Fix handling of 400 error when nothing is spoken.
* Fix error handling for tts & speech endpoints to be more verbose.
* Fix for url encoding bugs.

### In v63
* Adds resampling to AudioBuffer for easier custom audio input implementation and better runtime performance.
* Fixes WitRequest timeout to reset when responses are received.
* Adds support for arrays to ComposerContextMap Get/Set methods.

### In v62
* Improved runtime performance of audio transmission.
* Fix TTS compatibility with WebGL.
* Minor fixes and internal improvements.

### In v61
* TTS event streaming support added.
* TTS viseme event lip sync options added.
* TTS default settings adjusted for better performance.
* VoiceService & DictationService can now wrap mock requests for manual responses.
* All voice requests now return OnRawResponse event.

### In v60
* VoiceService startup latency reduced.
* Conduit now supports nullable parameters.
* RawAudioClipStream added for easier custom tts audio implementation.
* Dictation namespaces swapped from 'Facebook.*' to 'Meta.*'
* Removal of unused stub files.
* Minor fixes and internal improvements.

### In v59
* TTS ISpeaker interface added to allow for additional TTS processors.
* TTS streaming now works with MP3 data in addition to PCM.
* Minor fixes and internal improvements.

### In v57
* Minor fixes and internal improvements.

### In v56
* TTS speakers now have pause & resume functionality.
* TTS speaker events added for OnComplete which is called following load failure, load abort, playback cancellation or playback completion.
* TTS voice sample updated to include pause button, to allow info scrolling & to use tts speaker request events via async sfx toggle.

### In v55
* Samples are now accessible in the Voice Hub.
* All samples have been cleaned up and split into their own directory to make them easier to follow.
* TTS voices sample improved to make trying out different voices & ssml sfx effects easier.
* TTS recently added a set of new voices. TTSWit component now has a dropdown to add new presets with voices you may not have used yet.
