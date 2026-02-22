# BinaryBeat.IntelligentAudio

    IntelligentAudio is a high-performance .NET 8 application designed to bridge professional audio hardware and AI.

    By leveraging OpenAI Whisper and asynchronous data pipelines, it transforms live audio input from high-end interfaces (like the Audient iD14)
    into actionable MIDI data and DAW control commands with surgical precision.

## 🚀 Key Features

    **AI-Driven Chord Recognition:** Utilizes Whisper.net to interpret spoken or played musical terms (e.g., "A Minor", "C# Maj7") with high contextual awareness.
    **Pro Audio Signal Chain:** Optimized for Audient iD14 with 44.1kHz capture, real-time 16kHz resampling, and DC-blocking high-pass filters.
    **Intelligent Noise Gate:** RMS-based gating with Hysteresis and Hold-time logic to eliminate background hiss while ensuring musical tails are preserved for AI analysis.
    **Asynchronous Producer-Consumer Pipeline:** Powered by System.Threading.Channels to decouple audio capture from heavy AI inference, ensuring zero-drop reliability.
    **Musical MIDI Mapping:** Seamlessly converts interpreted text into MIDI note arrays via a logic-based ChordFactory.

## 🛠 Technical Stack

    Runtime: .NET 8 (C# 12)
    AI Engine: Whisper.net (GGML)
    Audio I/O: NAudio
    Dependency Injection: Microsoft.Extensions.DependencyInjection
    Communications: System.Threading.Channels

## 📦 Installation & Setup

    Clone the repository:
    bash:> git clone https://github.com/BinaryBeatSolutions/IntelligentAudio.git

### Prerequisites

    Ensure you have the .NET 8 SDK installed. For MIDI output, a virtual MIDI cable like loopMIDI is recommended.

    Models:
    The application automatically handles model acquisition. On the first run, it will download the required Whisper GGML models (e.g., ggml-tiny.en.bin) via the built-in PathResolver.
    Run:
    bash:> dotnet run --modelName ggml-tiny.en.bin


### 🎹 Internal Architecture

    Capture: MicrophoneSource captures 16-bit PCM data from the iD14.

    Processing: A High-pass filter removes low-end rumble, followed by an RMS calculation to determine if the signal exceeds the Threshold.
    -Queueing: Valid audio buffers are pushed into an unbounded Channel<byte[]>.
    Inference: IntelligentAudio consumes the channel, accumulates buffers, and executes Whisper AI inference using a musical Prompt to force-focus on chord terminology.
    Output: The string "A Minor" is mapped to MIDI notes 69, 72, 76 (A, C, E) and dispatched.


### 🛠 Configuration (CLI)

    modelName: Specify the Whisper model size (tiny, base, small).

    threshold: Set the Noise Gate sensitivity (default: 400).
    modelPath: Custom path for GGML models.

### 🗺 Roadmap

    DAW Voice Control: Map commands like "Record", "Stop", and "Undo" to MIDI CC.

    Confidence Filtering: Implement log-probability checks to ignore uncertain AI guesses.
    MidiPolyphonicExpression (MPE): Advanced note expression support.


### Developed by BinaryBeat Solutions

    Where Music Meets Code.
