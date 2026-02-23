const { spawn } = require('child_process');
const maxApi = require('max-api');

let child = null;
const exePath = "C:\\Program Files (x86)\\BinaryBeat";

maxApi.addHandler("start", () => {
    if (child) return;
    maxApi.post("BinaryBeat: Launching engine...");
    
    child = spawn(exePath, [], {
        detached: true,
        stdio: 'ignore'
    });
    child.unref();
});

maxApi.addHandler("stop", () => {
    if (child) {
        child.kill();
        child = null;
        maxApi.post("BinaryBeat: Stopped.");
    }
});