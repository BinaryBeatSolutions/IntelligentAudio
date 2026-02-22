const path = require('path');
const { spawn } = require('child_process');
const maxApi = require('max-api');

let child = null;

// Sökväg till din installerade EXE
const exePath = "C:\\Program Files (x86)\\BinaryBeat\\BinaryBeat.exe";

maxApi.addHandler("start", () => {
    if (child) return; // Körs redan

    maxApi.post("BinaryBeat: Starting engine...");

    child = spawn(exePath, [], {
        detached: true,
        stdio: 'ignore'
    });

    child.unref(); // Låt den köra oberoende av Max
    maxApi.post("BinaryBeat: Engine launched.");
});

maxApi.addHandler("stop", () => {
    if (child) {
        child.kill();
        child = null;
        maxApi.post("BinaryBeat: Engine stopped.");
    }
});
