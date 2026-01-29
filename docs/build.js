const glob = require('glob');
const fs = require('fs');
const readline = require('readline');

var spawn = require('child_process').spawn;

async function buildSite() {
    console.log("Running hugo...");
    // Set a build number
    let proc = spawn('hugo');
    proc.stdout.pipe(process.stdout);
    proc.stderr.pipe(process.stderr);

    await new Promise ((resolve, reject) => {
        proc.on('exit', (code, signal) => resolve());
    });
}

function postBuild() {
    console.log('Copying homepage into place...');
    return new Promise((resolve, reject) => {
        fs.copyFile('public/docs/home/index.html', 'public/docs/index.html', resolve);
    });
}

// Updates the mounts entries based on the contents-incl folder
async function makeMounts() {
    console.log("Mounting content directories...");
    const moduleFilePath = 'config/_default/module.toml';
    const tempFilePath = 'config/_default/.module.toml';

    // Read the modules file
    const fileStream = fs.createReadStream(moduleFilePath);
    const rl = readline.createInterface({
        input: fileStream,
        crlfDelay: Infinity
    });
    const dst = fs.createWriteStream(tempFilePath, { flags: 'w' });
    // use {flags: 'a'} to append and {flags: 'w'} to erase and write a new file

    for await (const line of rl) {
        // Write it out down to the marker comment
        dst.write(`${line}\n`);
        if (line == '############ Content includes ############')
            break;
    }    

    // Get the list of folders in the content-incl folder
    for await (const path of glob.sync('./content-incl/*/')) {
        // Write the modules out
        let parts = path.replace('\\', '/').split('/');
        let mod = parts[parts.length - 1];
        dst.write(`[[mounts]]
  source = "content-incl/${mod}"
  target = "content/docs/${mod.replace(/^[^-]*-/, '')}"

`);
    }
    dst.end();

    // Replace the source file with the new dest
    await new Promise((resolve) => fs.rm(moduleFilePath, { force: true }, resolve));
    await new Promise((resolve) => fs.rename(tempFilePath, moduleFilePath, resolve));
}

function errorExit(e) {
    console.error(e);
    process.exit(1);
}

makeMounts()
    .then(buildSite)
    .then(postBuild)
    .catch(errorExit);
